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
            int nxm2 = nx - 2;
            int nym2 = ny - 2;
            float keFactor = 1.0f / (2 * m_particleMass * wf.LatticeSpacing * wf.LatticeSpacing);

            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;


            // Compute the next real part in terms of the current imaginary part
            LoopDelegate YLoop1 = (x) =>
            {
                int xp  = (x < nxm1) ? x + 1 : 0;
                int xm  = (x > 0) ? x - 1 : nxm1;
                int xpp = (x < nxm2) ? x + 2 : (x == nxm2) ? 0 : 1;
                int xmm = (x > 1) ? x - 2 : (x > 0) ? nxm1 : nxm2;

                float[] Vx = V[x];
                float[] wfRx    = wf.RealPart[x];
                float[] wfIPx   = wf.ImagPartP[x];
                float[] wfIPxm  = wf.ImagPartP[xm];
                float[] wfIPxp  = wf.ImagPartP[xp];
                float[] wfIPxmm = wf.ImagPartP[xmm];
                float[] wfIPxpp = wf.ImagPartP[xpp];

                for (int y = 0; y < ny; y++)
                {
                    int yp = (y < nym1) ? y + 1 : 0;
                    int ym = (y > 0) ? y - 1 : nym1;
                    int ypp = (y < nym2) ? y + 2 : (y == nym2) ? 0 : 1;
                    int ymm = (y > 1) ? y - 2 : (y > 0) ? nym1 : nym2;

                    // This discretization of the 2nd derivative has better rotational invariance than the standard one
                    float ke = keFactor * (
                        alpha * wfIPx[y] +
                        beta  * (wfIPx[ym] + wfIPx[yp] + wfIPxm[y] + wfIPxp[y]) +
                        delta * (wfIPx[ymm] + wfIPx[ypp] + wfIPxmm[y] + wfIPxpp[y])
                    );

                    float pe = Vx[y] * wfIPx[y];

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
                int xpp = (x < nxm2) ? x + 2 : (x == nxm2) ? 0 : 1;
                int xmm = (x > 1) ? x - 2 : (x > 0) ? nxm1 : nxm2;

                float[] Vx = V[x];
                float[] wfIPx  = wf.ImagPartP[x];
                float[] wfIMx  = wf.ImagPartM[x];
                float[] wfRx   = wf.RealPart[x];
                float[] wfRxm  = wf.RealPart[xm];
                float[] wfRxp  = wf.RealPart[xp];
                float[] wfRxmm = wf.RealPart[xmm];
                float[] wfRxpp = wf.RealPart[xpp];

                for (int y = 0; y < ny; y++)
                {
                    int yp = (y < nym1) ? y + 1 : 0;
                    int ym = (y > 0) ? y - 1 : nym1;
                    int ypp = (y < nym2) ? y + 2 : (y == nym2) ? 0 : 1;
                    int ymm = (y > 1) ? y - 2 : (y > 0) ? nym1 : nym2;

                    // This discretization of the 2nd derivative has better rotational invariance than the standard one
                    float ke = keFactor * (
                        alpha * wfRx[y] +
                        beta  * (wfRx[ym] + wfRx[yp] + wfRxm[y] + wfRxp[y]) +
                        delta * (wfRx[ymm] + wfRx[ypp] + wfRxmm[y] + wfRxpp[y])
                    );

                    float pe = Vx[y] * wfRx[y];

                    wfIPx[y] = wfIMx[y] - dt * (ke + pe);
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
