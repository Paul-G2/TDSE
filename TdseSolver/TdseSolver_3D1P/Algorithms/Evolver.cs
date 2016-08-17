using System;
using System.Threading.Tasks;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class computes the time evolution of a wavefunction.
    /// </summary>
    class Evolver : TdseUtils.Proc
    {
        // Declare a delegate for computing the potential energy
        public delegate float VDelegate(float x, float y, float z, float t, float mass, float sx, float sy, float sz);


        // Class data
        private WaveFunction   m_intialWf              = null;
        private VisscherWf     m_visscherWf            = null;
        private VDelegate      m_potential             = null;
        private bool           m_isTimeDependentV      = false; // Time-dependent potentials are not yet supported
        private float          m_particleMass          = 0.0f;
        private float          m_totalTime             = float.NaN;
        private float          m_deltaT                = float.NaN;
        private int            m_currentTimeStepIndex  = 0;
        private int            m_totalNumTimeSteps     = 0;
        private int            m_reportInterval        = -1;
        private int            m_dampingBorderWidth    = 0;
        private float          m_dampingFactor         = 0.0f;
        private bool           m_multiThread           = true;



        /// <summary>
        /// Constructor.
        /// </summary>
        public Evolver(WaveFunction initialWf, float totalTime, float timeStep, VDelegate V, bool isVTimeDependent, float particleMass, int progressReportInterval, 
            int dampingBorderWidth = 0, float dampingFactor=0.0f, bool multiThread=true)
        {
            m_intialWf              = initialWf;
            m_visscherWf            = null;
            m_potential             = V;
            m_isTimeDependentV      = isVTimeDependent;
            m_particleMass          = particleMass;
            m_totalTime             = totalTime;
            m_deltaT                = timeStep;
            m_currentTimeStepIndex  = 0;
            m_totalNumTimeSteps     = (int) Math.Round(totalTime/timeStep) + 1;
            m_reportInterval        = progressReportInterval;
            m_dampingBorderWidth    = dampingBorderWidth;
            m_dampingFactor         = dampingFactor;
            m_multiThread           = multiThread;
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
            float[][][] V = PrecomputeV(0);

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
        private float[][][] PrecomputeV(float time)
        {
            int sx = m_intialWf.GridSpec.SizeX;
            int sy = m_intialWf.GridSpec.SizeY;
            int sz = m_intialWf.GridSpec.SizeZ;
            float[][][] V = TdseUtils.Misc.Allocate3DArray(sz, sy, sx);

            float a = m_intialWf.LatticeSpacing;
            float domainSizeX = sx * a;
            float domainSizeY = sy * a;
            float domainSizeZ = sz * a;

            TdseUtils.Misc.ForLoop(0, sz, (z) =>
            {
                for (int y = 0; y < sy; y++)
                {
                    float[] Vzy = V[z][y];
                    for (int x = 0; x < sx; x++)
                    {
                        Vzy[x] = m_potential(x*a, y*a, z*a, time, m_particleMass, domainSizeX, domainSizeY, domainSizeZ);
                    }
                }
            }, m_multiThread );

            return V;
        }



        /// <summary>
        /// Evolves the wavefunction by a single timestep
        /// </summary>
        private void EvolveByOneTimeStep(VisscherWf wf, float[][][] V)
        {
            int sx = wf.GridSpec.SizeX;
            int sy = wf.GridSpec.SizeY;
            int sz = wf.GridSpec.SizeZ;

            int sxm1 = sx - 1;
            int sym1 = sy - 1;
            int szm1 = sz - 1;

            float keFactor = (1.0f/12.0f) / (2 * m_particleMass * wf.LatticeSpacing * wf.LatticeSpacing);


            // Compute the next real part in terms of the current imaginary part
            TdseUtils.Misc.ForLoop(0, sz, (z) =>
            {
                int zp  = (z  < szm1) ?  z + 1 : 0;
                int zpp = (zp < szm1) ? zp + 1 : 0;
                int zm  = (z  > 0) ?  z - 1 : szm1;
                int zmm = (zm > 0) ? zm - 1 : szm1;

                for (int y = 0; y < sy; y++)
                {
                    int yp  = (y  < sym1) ?  y + 1 : 0;
                    int ypp = (yp < sym1) ? yp + 1 : 0;
                    int ym  = (y  > 0) ?  y - 1 : sym1;
                    int ymm = (ym > 0) ? ym - 1 : sym1;

                    float[] V_z_y      = V[z][y];
                    float[] wfR_z_y    =  wf.RealPart[z][y];
                    float[] wfI_z_y    =  wf.ImagPartP[z][y];
                    float[] wfI_z_ym   =  wf.ImagPartP[z][ym];
                    float[] wfI_z_yp   =  wf.ImagPartP[z][yp];
                    float[] wfI_z_ymm  =  wf.ImagPartP[z][ymm];
                    float[] wfI_z_ypp  =  wf.ImagPartP[z][ypp];
                    float[] wfI_zm_y   =  wf.ImagPartP[zm][y];
                    float[] wfI_zp_y   =  wf.ImagPartP[zp][y];
                    float[] wfI_zmm_y  =  wf.ImagPartP[zmm][y];
                    float[] wfI_zpp_y  =  wf.ImagPartP[zpp][y];

                    for (int x = 0; x < sx; x++)
                    {
                        int xp  = (x  < sxm1) ?  x + 1 : 0;
                        int xpp = (xp < sxm1) ? xp + 1 : 0;
                        int xm  = (x  > 0) ?  x - 1 : sxm1;
                        int xmm = (xm > 0) ? xm - 1 : sxm1;

                        // Discretization of the 2nd derivative
                        float ke = keFactor * (
                            90.0f * wfI_z_y[x] -
                            16.0f * (wfI_zm_y[x] + wfI_zp_y[x] + wfI_z_yp[x] + wfI_z_ym[x] + wfI_z_y[xm] + wfI_z_y[xp]) +
                            (wfI_zmm_y[x] + wfI_zpp_y[x] + wfI_z_ypp[x] + wfI_z_ymm[x] + wfI_z_y[xmm] + wfI_z_y[xpp]) 
                        );

                        float pe = V_z_y[x] * wfI_z_y[x];

                        wfR_z_y[x] += m_deltaT * (ke + pe);
                    }
                }
            }, m_multiThread );


            // Swap prev and post imaginary parts
            float[][][] temp = wf.ImagPartM;
            wf.ImagPartM = wf.ImagPartP;
            wf.ImagPartP = temp;


            // Compute the next imaginary part in terms of the current real part
            TdseUtils.Misc.ForLoop(0, sz, (z) =>
            {
                int zp  = (z  < szm1) ?  z + 1 : 0;
                int zpp = (zp < szm1) ? zp + 1 : 0;
                int zm  = (z  > 0) ?  z - 1 : szm1;
                int zmm = (zm > 0) ? zm - 1 : szm1;

                for (int y = 0; y < sy; y++)
                {
                    int yp  = (y  < sym1) ?  y + 1 : 0;
                    int ypp = (yp < sym1) ? yp + 1 : 0;
                    int ym  = (y  > 0) ?  y - 1 : sym1;
                    int ymm = (ym > 0) ? ym - 1 : sym1;

                    float[] V_z_y      =  V[z][y];
                    float[] wfIM_z_y   =  wf.ImagPartM[z][y];
                    float[] wfIP_z_y   =  wf.ImagPartP[z][y];
                    float[] wfR_z_y    =  wf.RealPart[z][y];
                    float[] wfR_z_ym   =  wf.RealPart[z][ym];
                    float[] wfR_z_yp   =  wf.RealPart[z][yp];
                    float[] wfR_z_ymm  =  wf.RealPart[z][ymm];
                    float[] wfR_z_ypp  =  wf.RealPart[z][ypp];
                    float[] wfR_zm_y   =  wf.RealPart[zm][y];
                    float[] wfR_zp_y   =  wf.RealPart[zp][y];
                    float[] wfR_zmm_y  =  wf.RealPart[zmm][y];
                    float[] wfR_zpp_y  =  wf.RealPart[zpp][y];

                    for (int x = 0; x < sx; x++)
                    {
                        int xp  = (x  < sxm1) ?  x + 1 : 0;
                        int xpp = (xp < sxm1) ? xp + 1 : 0;
                        int xm  = (x  > 0) ?  x - 1 : sxm1;
                        int xmm = (xm > 0) ? xm - 1 : sxm1;

                        // Discretization of the 2nd derivative
                        float ke = keFactor * (
                            90.0f * wfR_z_y[x] -
                            16.0f * (wfR_zm_y[x] + wfR_zp_y[x] + wfR_z_yp[x] + wfR_z_ym[x] + wfR_z_y[xm] + wfR_z_y[xp]) +
                            (wfR_zmm_y[x] + wfR_zpp_y[x] + wfR_z_ypp[x] + wfR_z_ymm[x] + wfR_z_y[xmm] + wfR_z_y[xpp]) 
                        );

                        float pe = V_z_y[x] * wfR_z_y[x];

                        wfIP_z_y[x] = wfIM_z_y[x] - m_deltaT * (ke + pe);
                    }
                }
            }, m_multiThread );


            // Optionally perform damping to suppress reflection and transmission at the borders. 
            if ( (m_dampingBorderWidth > 0) && (m_dampingFactor > 0.0f) )
            {
                ApplyDamping(wf);
            }
        }



        /// <summary>
        /// Damps the wavefunction apmlitude near the region boundary.
        /// </summary>
        private void ApplyDamping(VisscherWf wf)
        {
            int sx = wf.GridSpec.SizeX;
            int sy = wf.GridSpec.SizeY;
            int sz = wf.GridSpec.SizeZ;

            int d = m_dampingBorderWidth;
            float[] factors = new float[d];
            for (int i=0; i<d; i++)
            {
                factors[i] = (float) ( 1.0 - m_dampingFactor*m_deltaT*(1.0 - Math.Sin( ((Math.PI/2)*i)/d )) ); 
            }

            // Front Z border
            for (int z=0; z<d; z++)
            {
                float f = factors[z];

                for (int y = 0; y < sy; y++)
                {
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }

            // Back Z border
            for (int z=sz-d; z<sz; z++)
            {
                float f = factors[sz-1-z];

                for (int y = 0; y < sy; y++)
                {
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }

            // Y borders
            for (int z=0; z<sz; z++)
            {
                for (int y=0; y<d; y++)
                {
                    float f = factors[y];
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
                for (int y=sy-d; y<sy; y++)
                {
                    float f = factors[sy-1-y];
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }


            // X borders
            for (int z=0; z<sz; z++)
            {
                for (int y=0; y<sy; y++)
                {
                    float[] wfDataIPzy = wf.ImagPartP[z][y];
                    float[] wfDataIMzy = wf.ImagPartM[z][y];
                    float[] wfDataRzy  = wf.RealPart[z][y];

                    for (int x = 0; x < d; x++)
                    {
                        float f = factors[x];
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }

                    for (int x = sx-d; x < sx; x++)
                    {
                        float f = factors[sx-1-x];
                        wfDataRzy[x]  *= f;
                        wfDataIPzy[x] *= f;
                        wfDataIMzy[x] *= f;
                    }
                }
            }

        }
        
    
    }
}
