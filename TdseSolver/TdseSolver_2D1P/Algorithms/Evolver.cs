using System;
using System.Threading.Tasks;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class computes the time evolution of a wavefunction.
    /// </summary>
    class Evolver : TdseSolver.Proc
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
        private bool           m_isTimeDependentV       = false; // Time-dependent potentials are not yet supported
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

            // Main loop
            m_currentTimeStepIndex = 0;
            while (m_currentTimeStepIndex < m_totalNumTimeSteps)
            {
                // Compute the potential at the current time, if necessary
                if ( m_isTimeDependentV && (m_currentTimeStepIndex > 0) ) { V = PrecomputeV(m_currentTimeStepIndex*m_deltaT); }

                // Evolve the wavefunction by one timestep
                EvolveByOneTimeStep(m_visscherWf, V, m_deltaT, m_multiThread);
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
            int nx = m_intialWf.GridSizeX;
            int ny = m_intialWf.GridSizeY;

            float[][] V = new float[nx][];
            for (int i = 0; i < nx; i++) { V[i] = new float[ny]; }

            float a = m_intialWf.LatticeSpacing;
            float domainSizeX = nx * a;
            float domainSizeY = ny * a;

            for (int x = 0; x < nx; x++)
            {
                for (int y = 0; y < ny; y++)
                {
                    V[x][y] = m_potential(x*a, y*a, time, m_particleMass, domainSizeX, domainSizeY);
                }
            }

            return V;
        }



        // Declare a worker delegate needed by the following method
        private delegate void LoopDelegate(int x);

        /// <summary>
        /// Evolves the wavefunction by a single timestep
        /// </summary>
        private void EvolveByOneTimeStep(VisscherWf wf, float[][] V, float dt, bool multiThread = true)
        {
            int nx = wf.GridSizeX;
            int ny = wf.GridSizeY;
            int nxm1 = nx - 1;
            int nym1 = ny - 1;
            float keFactor = 1.0f / (2 * m_particleMass * wf.LatticeSpacing * wf.LatticeSpacing);

            float alpha = -1.0f / 6.0f;
            float gamma = -2.0f / 3.0f;
            float beta  = -4.0f * (alpha + gamma);


            // Compute the next real part in terms of the current imaginary part
            LoopDelegate YLoop1 = (x) =>
            {
                int xp = (x < nxm1) ? x + 1 : 0;
                int xm = (x > 0) ? x - 1 : nxm1;

                float[] Vx = V[x];
                float[] wfRx  = wf.RealPart[x];
                float[] wfIxP0  = wf.ImagPartP[x];
                float[] wfIxPM = wf.ImagPartP[xm];
                float[] wfIxPP = wf.ImagPartP[xp];

                for (int y = 0; y < ny; y++)
                {
                    int yp = (y < nym1) ? y + 1 : 0;
                    int ym = (y > 0) ? y - 1 : nym1;

                    // This discretization of the 2nd derivative has better rotational invariance than the standard one
                    float ke = keFactor * (
                        alpha * (wfIxPM[ym] + wfIxPP[ym] + wfIxPM[yp] + wfIxPP[yp]) +
                        gamma * (wfIxP0[ym] + wfIxP0[yp] + wfIxPM[y] + wfIxPP[y]) +
                        beta * wfIxP0[y]
                    );

                    float pe = Vx[y] * wfIxP0[y];

                    wfRx[y] += dt * (ke + pe);
                }
            };
            if (multiThread)
            {
                Parallel.For(0, nx, x => { YLoop1(x); });
            }
            else
            {
                for (int x = 0; x < nx; x++) { YLoop1(x); }
            }


            // Swap prev and post imaginary parts
            float[][] temp = wf.ImagPartM;
            wf.ImagPartM = wf.ImagPartP;
            wf.ImagPartP = temp;

            // Compute the next imaginary part in terms of the current real part
            LoopDelegate YLoop2 = (x) =>
            {
                int xp = (x < nxm1) ? x + 1 : 0;
                int xm = (x > 0) ? x - 1 : nxm1;

                float[] Vx = V[x];
                float[] wfIxP0 = wf.ImagPartP[x];
                float[] wfIxM0 = wf.ImagPartM[x];
                float[] wfRx0  = wf.RealPart[x];
                float[] wfRxM  = wf.RealPart[xm];
                float[] wfRxP  = wf.RealPart[xp];

                for (int y = 0; y < ny; y++)
                {
                    int yp = (y < nym1) ? y + 1 : 0;
                    int ym = (y > 0) ? y - 1 : nym1;

                    // This discretization of the 2nd derivative has better rotational invariance than the standard one
                    float ke = keFactor * (
                        alpha * (wfRxM[ym] + wfRxP[ym] + wfRxM[yp] + wfRxP[yp]) +
                        gamma * (wfRx0[ym] + wfRx0[yp] + wfRxM[y] + wfRxP[y]) +
                        beta * wfRx0[y]
                    );

                    float pe = Vx[y] * wfRx0[y];

                    wfIxP0[y] = wfIxM0[y] - dt * (ke + pe);
                }
            };
            if (multiThread)
            {
                Parallel.For(0, nx, x => { YLoop2(x); });
            }
            else
            {
                for (int x = 0; x < nx; x++) { YLoop2(x); }
            }


            // Optionally apply the damping factor to suppress reflection and transmission at the borders. 
            if ( (m_dampingBorderWidth > 0) && (m_dampingFactor > 0.0f) )
            {
                int d = m_dampingBorderWidth;
                float[] factors = new float[d];
                for (int i=0; i<d; i++)
                {
                    factors[i] = (float) ( 1.0 - m_dampingFactor*dt*(1.0 - Math.Sin( ((Math.PI/2)*i)/d )) ); 
                }

                // Left border
                for (int x=0; x<d; x++)
                {
                    float[] wfDataIxP = wf.ImagPartP[x];
                    float[] wfDataIxM = wf.ImagPartM[x];
                    float[] wfDataRx  = wf.RealPart[x];

                    for (int y=0; y<ny; y++)
                    {
                        wfDataRx[y]  *= factors[x];
                        wfDataIxP[y] *= factors[x];
                        wfDataIxM[y] *= factors[x];
                    }
                }

                // Right border
                for (int x=nx-d; x<nx; x++)
                {
                    float[] wfDataIxP = wf.ImagPartP[x];
                    float[] wfDataIxM = wf.ImagPartM[x];
                    float[] wfDataRx  = wf.RealPart[x];

                    for (int y=0; y<ny; y++)
                    {
                        wfDataRx[y]  *= factors[nx-1-x];
                        wfDataIxP[y] *= factors[nx-1-x];
                        wfDataIxM[y] *= factors[nx-1-x];
                    }
                }

                // Top and bottom borders
                for (int x=0; x<nx; x++)
                {
                    float[] wfDataIxP = wf.ImagPartP[x];
                    float[] wfDataIxM = wf.ImagPartM[x];
                    float[] wfDataRx  = wf.RealPart[x];

                    for (int y=0; y<d; y++)
                    {
                        wfDataRx[y]  *= factors[y];
                        wfDataIxP[y] *= factors[y];
                        wfDataIxM[y] *= factors[y];
                    }
                    for (int y=ny-d; y<ny; y++)
                    {
                        wfDataRx[y]  *= factors[ny-1-y];
                        wfDataIxP[y] *= factors[ny-1-y];
                        wfDataIxM[y] *= factors[ny-1-y];
                    }
                }

            }
        }
        
    
    }
}
