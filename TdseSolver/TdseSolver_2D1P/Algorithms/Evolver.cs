using System;
using System.Threading.Tasks;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class computes the time evolution of a wavefunction.
    /// </summary>
    class Evolver : TdseUtils.Proc
    {
        // Declare a delegate for computing the potential energy
        public delegate float VDelegate(float x, float y, float t, float mass, float sx, float sy);


        // Class data
        private WaveFunction   m_intialWf              = null;
        private float          m_totalTime             = float.NaN;
        private float          m_deltaT                = float.NaN;
        private int            m_currentTimeStepIndex  = 0;
        private int            m_totalNumTimeSteps     = 0;
        private VDelegate      m_potential             = null;
        private bool           m_isTimeDependentV      = false; // Time-dependent potentials are not yet supported
        private float          m_particleMass          = 0.0f;
        private int            m_reportInterval        = -1;
        private int            m_dampingBorderWidth    = 0;
        private float          m_dampingFactor         = 0.0f;
        private bool           m_multiThread           = true;
        private VisscherWf     m_visscherWf            = null;



        /// <summary>
        /// Constructor.
        /// </summary>
        public Evolver(WaveFunction initialWf, float totalTime, float timeStep, VDelegate V, bool isVTimeDependent, float particleMass, int progressReportInterval, 
            int dampingBorderWidth = 0, float dampingFactor=0.0f, bool multiThread=true)
        {
            m_intialWf           = initialWf;
            m_totalTime          = totalTime;
            m_deltaT             = timeStep;
            m_totalNumTimeSteps  = (int) Math.Round(totalTime/timeStep) + 1;
            m_potential          = V;
            m_isTimeDependentV   = isVTimeDependent;
            m_particleMass       = particleMass;
            m_reportInterval     = progressReportInterval;
            m_dampingBorderWidth = dampingBorderWidth;
            m_dampingFactor      = dampingFactor;
            m_multiThread        = multiThread;

        }


        /// <summary>
        /// Gets the total number of timesteps that the Evolver will step through.
        /// </summary>
        public int TotalNumTimeSteps
        {
            get { return m_totalNumTimeSteps; }
        }


        /// <summary>
        /// Gets the current time step index.
        /// </summary>
        public int CurrentTimeStepIndex
        {
            get { return m_currentTimeStepIndex; }
        }


        /// <summary>
        /// Gets the wavefunction that's being evolved.
        /// </summary>
        public WaveFunction Wf
        {
            get
            {
                return (m_visscherWf == null) ? null : m_visscherWf.ToRegularWavefunction();
            }
        }


        /// <summary>
        /// Worker method.
        /// </summary>
        protected override void WorkerMethod()
        {
            // Precompute the potential everywhere on the grid
            float[][] V = PrecomputeV(0);

            // Create a Visscher wf from the input wf
            m_visscherWf = new VisscherWf(m_intialWf, V, m_particleMass, m_deltaT, m_multiThread);
            m_intialWf = null; // Allow m_initialWf to be garbage collected

            // Main loop
            m_currentTimeStepIndex = 0;
            while (m_currentTimeStepIndex < m_totalNumTimeSteps)
            {
                // Compute the potential at the current time, if necessary
                if ( m_isTimeDependentV && (m_currentTimeStepIndex > 0) ) { V = PrecomputeV(m_currentTimeStepIndex*m_deltaT); }

                // Evolve the wavefunction by one timestep
                EvolveByOneTimeStep(m_visscherWf, V);
                m_currentTimeStepIndex++;

                // Report progress to the caller
                if ( m_currentTimeStepIndex%m_reportInterval == 0 )
                {
                    ReportProgress();
                    if (IsCancelled) { return; }
                }
            }

        }


        /// <summary>
        /// Computes the potential at all grid locations. 
        /// </summary>
        private float[][] PrecomputeV(float time)
        {
            int sx = m_intialWf.GridSizeX;
            int sy = m_intialWf.GridSizeY;
            float[][] V = TdseUtils.Misc.Allocate2DArray(sy, sx);

            float a = m_intialWf.LatticeSpacing;
            float domainSizeX = sx * a;
            float domainSizeY = sy * a;

            TdseUtils.Misc.ForLoop(0, sy, (y) =>
            {
                float[] Vy = V[y];
                for (int x = 0; x < sx; x++)
                {
                    Vy[x] = m_potential(x*a, y*a, time, m_particleMass, domainSizeX, domainSizeY);
                }
            }, m_multiThread );

            return V;
        }



        /// <summary>
        /// Evolves the wavefunction by a single timestep
        /// </summary>
        private void EvolveByOneTimeStep(VisscherWf wf, float[][] V)
        {
            int sx = wf.GridSizeX;
            int sy = wf.GridSizeY;
            int sxm1 = sx - 1;
            int sym1 = sy - 1;
            float keFactor = 1.0f / (2 * m_particleMass * wf.LatticeSpacing * wf.LatticeSpacing);

            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;

            // Compute the next real part in terms of the current imaginary part
            TdseUtils.Misc.ForLoop(0, sy, y =>
            {
                int yp  = (y  < sym1) ?  y + 1 : 0;
                int ypp = (yp < sym1) ? yp + 1 : 0;
                int ym  = (y  > 0) ?  y - 1 : sym1;
                int ymm = (ym > 0) ? ym - 1 : sym1;

                float[] V_y     = V[y];
                float[] wfR_y   = wf.RealPart[y];
                float[] wfI_y   = wf.ImagPartP[y];
                float[] wfI_ym  = wf.ImagPartP[ym];
                float[] wfI_yp  = wf.ImagPartP[yp];
                float[] wfI_ymm = wf.ImagPartP[ymm];
                float[] wfI_ypp = wf.ImagPartP[ypp];

                for (int x = 0; x < sx; x++)
                {
                    int xp  = (x  < sxm1) ?  x + 1 : 0;
                    int xpp = (xp < sxm1) ? xp + 1 : 0;
                    int xm  = (x  > 0) ?  x - 1 : sxm1;
                    int xmm = (xm > 0) ? xm - 1 : sxm1;

                    // A discretization of the 2nd derivative, whose error term is O(a^6)
                    float ke = keFactor * (
                        alpha * wfI_y[x] +
                        beta  * (wfI_y[xm] + wfI_y[xp] + wfI_ym[x] + wfI_yp[x]) +
                        delta * (wfI_y[xmm] + wfI_y[xpp] + wfI_ymm[x] + wfI_ypp[x])
                    );
                        
                    float pe = V_y[x] * wfI_y[x];

                    wfR_y[x] += m_deltaT * (ke + pe);
                }
            }, m_multiThread );


            // Swap prev and post imaginary parts
            float[][] temp = wf.ImagPartM;
            wf.ImagPartM = wf.ImagPartP;
            wf.ImagPartP = temp;


            // Compute the next imaginary part in terms of the current real part
            TdseUtils.Misc.ForLoop(0, sy, y =>
            {
                int yp  = (y  < sym1) ?  y + 1 : 0;
                int ypp = (yp < sym1) ? yp + 1 : 0;
                int ym  = (y  > 0) ?  y - 1 : sym1;
                int ymm = (ym > 0) ? ym - 1 : sym1;

                float[] V_y      =  V[y];
                float[] wfIM_y   =  wf.ImagPartM[y];
                float[] wfIP_y   =  wf.ImagPartP[y];
                float[] wfR_y    =  wf.RealPart[y];
                float[] wfR_ym   =  wf.RealPart[ym];
                float[] wfR_yp   =  wf.RealPart[yp];
                float[] wfR_ymm  =  wf.RealPart[ymm];
                float[] wfR_ypp  =  wf.RealPart[ypp];

                for (int x = 0; x < sx; x++)
                {
                    int xp  = (x  < sxm1) ?  x + 1 : 0;
                    int xpp = (xp < sxm1) ? xp + 1 : 0;
                    int xm  = (x  > 0) ?  x - 1 : sxm1;
                    int xmm = (xm > 0) ? xm - 1 : sxm1;

                    // A discretization of the 2nd derivative, whose error term is O(a^6)
                    float ke = keFactor * (
                        alpha * wfR_y[x] +
                        beta  * (wfR_y[xm] + wfR_y[xp] + wfR_ym[x] + wfR_yp[x]) +
                        delta * (wfR_y[xmm] + wfR_y[xpp] + wfR_ymm[x] + wfR_ypp[x])
                    );

                    float pe = V_y[x] * wfR_y[x];

                    wfIP_y[x] = wfIM_y[x] - m_deltaT * (ke + pe);
                }
            }, m_multiThread );


            // Optionally perform damping to suppress reflection and transmission at the borders. 
            if ( (m_dampingBorderWidth > 0) && (m_dampingFactor > 0.0f) )
            {
                ApplyDamping(wf);
            }

        }

        /// <summary>
        /// Damps the wavefunction amplitude near the region boundary.
        /// </summary>
        private void ApplyDamping(VisscherWf wf)
        {
            int sx = wf.GridSizeX;
            int sy = wf.GridSizeY;
            int d = m_dampingBorderWidth;

            float[] factors = new float[d];
            for (int i=0; i<d; i++)
            {
                factors[i] = (float) ( 1.0 - m_dampingFactor*m_deltaT*(1.0 - Math.Sin( ((Math.PI/2)*i)/d )) ); 
            }

            // Top border
            for (int y=0; y<d; y++)
            {
                float[] wfDataIPy = wf.ImagPartP[y];
                float[] wfDataIMy = wf.ImagPartM[y];
                float[] wfDataRy  = wf.RealPart[y];
                float f = factors[y];
 
                for (int x=0; x<sx; x++)
                {
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
            }

            // Bottom border
            for (int y=sy-d; y<sy; y++)
            {
                float[] wfDataIPy = wf.ImagPartP[y];
                float[] wfDataIMy = wf.ImagPartM[y];
                float[] wfDataRy  = wf.RealPart[y];
                float f = factors[sy-1-y];

                for (int x=0; x<sx; x++)
                {
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
            }

            // Left and right borders
            for (int y=0; y<sy; y++)
            {
                float[] wfDataIPy = wf.ImagPartP[y];
                float[] wfDataIMy = wf.ImagPartM[y];
                float[] wfDataRy  = wf.RealPart[y];

                for (int x=0; x<d; x++)
                {
                    float f = factors[x];
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
                for (int x=sx-d; x<sx; x++)
                {
                    float f = factors[sx-1-x];
                    wfDataRy[x]  *= f;
                    wfDataIPy[x] *= f;
                    wfDataIMy[x] *= f;
                }
            }

        }
        
        
    
    }
}
