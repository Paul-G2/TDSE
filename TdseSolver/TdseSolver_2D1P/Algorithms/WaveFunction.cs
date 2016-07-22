using System;
using System.Threading.Tasks;



namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 2 dimensions, defined on a rectangular grid.
    /// </summary>
    partial class WaveFunction
    {
        #region Class data
        float[][] m_realPart;
        float[][] m_imagPart;
        float m_latticeSpacing;
        #endregion Class data


        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(int gridSizeX, int gridSizeY, float latticeSpacing)
        {
            m_realPart = new float[gridSizeX][];
            m_imagPart = new float[gridSizeX][];
            m_latticeSpacing = latticeSpacing;

            for (int i=0; i<gridSizeX; i++)
            {
                m_realPart[i] = new float[gridSizeY];
                m_imagPart[i] = new float[gridSizeY];
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(float[][] realPart, float[][] imagPart, float latticeSpacing)
        {
            // Check the inputs
            if ( (realPart == null) != (imagPart == null) )
            {
                throw new ArgumentException("Invalid array(s) passed to Wavefunction ctor.");
            }
            if (realPart != null)
            {
                if (realPart.Length != imagPart.Length)
                {
                    throw new ArgumentException("Invalid array(s) passed to Wavefunction ctor.");
                }
                for (int i=0; i<realPart.Length; i++)
                {
                    if ( (realPart[i] == null) || (imagPart[i] == null) ||  (realPart[i].Length != imagPart[i].Length) )
                    {
                        throw new ArgumentException("Invalid array(s) passed to Wavefunction ctor.");
                    }
                }
            }

            // Accept the inputs
            m_realPart = realPart;
            m_imagPart = imagPart;
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Gets the real part of the wavefunction.
        /// </summary>
        public float[][] RealPart
        {
            get
            {
                return m_realPart;
            }
        }

        /// <summary>
        /// Gets the imaginary part of the wavefunction.
        /// </summary>
        public float[][] ImagPart
        {
            get
            {
                return m_imagPart;
            }
        }


        /// <summary>
        /// Gets the wavefunction amplitude at a given location.
        /// </summary>
        public float Ampl(int x, int y)
        {
            float re = m_realPart[x][y];
            float im = m_imagPart[x][y];

            return (float) Math.Sqrt(re*re + im*im);
        }


        /// <summary>
        /// Gets the squared wavefunction amplitude at a given location.
        /// </summary>
        public float Prob(int x, int y)
        {
            float re = m_realPart[x][y];
            float im = m_imagPart[x][y];

            return (re*re + im*im);
        }


        /// <summary>
        /// Gets the phase angle at a given location. (Range is -Pi to Pi).
        /// </summary>
        public float Phase(int x, int y)
        {
            return (float) Math.Atan2(m_realPart[x][y], m_imagPart[x][y]);
        }


        /// <summary>
        /// Gets the number of grid points along the x direction.
        /// </summary>
        public int GridSizeX
        {
            get
            {
                return m_realPart.Length;
            }
        }


        /// <summary>
        /// Gets the number of grid points along the y direction.
        /// </summary>
        public int GridSizeY
        {
            get
            {
                return (m_realPart.Length > 0) ? m_realPart[0].Length : 0;
            }
        }


        /// <summary>
        /// Gets the lattice spacing.
        /// </summary>
        public float LatticeSpacing
        {
            get
            {
                return m_latticeSpacing;
            }
        }


        /// <summary>
        /// Computes the total squared norm of the wavefunction.
        /// </summary>
        public float NormSq()
        {
            float normSq = 0.0f;

            int nx = GridSizeX;
            int ny = GridSizeY;
            for (int x = 0; x < nx; x++)
            {
                float[] ReX = m_realPart[x];
                float[] ImX = m_imagPart[x];
                for (int y = 0; y < ny; y++)
                {
                    float re = ReX[y];
                    float im = ImX[y];
                    normSq += (re*re + im*im);
                }
            }

            normSq *= m_latticeSpacing*m_latticeSpacing;
            return normSq;
        }


        /// <summary>
        /// Normalizes the wavefunction.
        /// </summary>
        public void Normalize()
        {
            ScaleBy( (float)( 1.0/Math.Sqrt(NormSq()) ) );
        }


        /// <summary>
        /// Multiplies the wavefunction by a given factor.
        /// </summary>
        public void ScaleBy(float factor)
        {
            int nx = GridSizeX;
            int ny = GridSizeY;
            for (int x = 0; x < nx; x++)
            {
                float[] ReX = m_realPart[x];
                float[] ImX = m_imagPart[x];
                for (int y = 0; y < ny; y++)
                {
                    ReX[y] *= factor;
                    ImX[y] *= factor;
                }
            }
        }

        
        // Worker delegate needed by the following method
        private delegate void LoopDelegate(int x);

        /// <summary>
        /// Computes the result of applying a given Hamiltonian operator to this wavefunction.
        /// </summary>
        public WaveFunction ApplyH(float[][] V, float mass, bool multiThread=true)
        {
            // Initialize locals
            int nx = GridSizeX;
            int ny = GridSizeY;
            int nxm1 = nx - 1;
            int nym1 = ny - 1;
            int nxm2 = nx - 2;
            int nym2 = ny - 2;

            float keFactor = 1.0f / (2 * mass *m_latticeSpacing * m_latticeSpacing);

            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;

            WaveFunction outWf = new WaveFunction(nx, ny, m_latticeSpacing);


            // Compute H * Wf
            LoopDelegate YLoop = (x) =>
            {
                int xp = (x < nxm1) ? x + 1 : 0;
                int xm = (x > 0) ? x - 1 : nxm1;
                int xpp = (x < nxm2) ? x + 2 : (x == nxm2) ? 0 : 1;
                int xmm = (x > 1) ? x - 2 : (x > 0) ? nxm1 : nxm2;

                float[] Vx = V[x];
                float[] inWfRx   = m_realPart[x];
                float[] inWfRxm  = m_realPart[xm];
                float[] inWfRxp  = m_realPart[xp];
                float[] inWfRxmm = m_realPart[xmm];
                float[] inWfRxpp = m_realPart[xpp];
                float[] inWfIx   = m_imagPart[x];
                float[] inWfIxm  = m_imagPart[xm];
                float[] inWfIxp  = m_imagPart[xp];
                float[] inWfIxmm = m_imagPart[xmm];
                float[] inWfIxpp = m_imagPart[xpp];
                float[] outWfRx  = outWf.RealPart[x];
                float[] outWfIx  = outWf.ImagPart[x];

                for (int y = 0; y < ny; y++)
                {
                    int yp = (y < nym1) ? y + 1 : 0;
                    int ym = (y > 0) ? y - 1 : nym1;
                    int ypp = (y < nym2) ? y + 2 : (y == nym2) ? 0 : 1;
                    int ymm = (y > 1) ? y - 2 : (y > 0) ? nym1 : nym2;

                    // Kinetic energy terms. 
                    // (This discretization of the 2nd derivative has better rotational invariance than the standard one)
                    float kR = keFactor * (
                        alpha * inWfRx[y] +
                        beta  * (inWfRx[ym] + inWfRx[yp] + inWfRxm[y] + inWfRxp[y]) +
                        delta * (inWfRx[ymm] + inWfRx[ypp] + inWfRxmm[y] + inWfRxpp[y])
                    );

                    float kI = keFactor * (
                        alpha * inWfIx[y] +
                        beta  * (inWfIx[ym] + inWfIx[yp] + inWfIxm[y] + inWfIxp[y]) +
                        delta * (inWfIx[ymm] + inWfIx[ypp] + inWfIxmm[y] + inWfIxpp[y])
                    );

                    // Potential energy terms
                    float vR = Vx[y] * inWfRx[y];
                    float vI = Vx[y] * inWfIx[y];

                    outWfRx[y] = kR + vR;
                    outWfIx[y] = kI + vI;
                }
            };
            if (multiThread)
            {
                Parallel.For(0, nx, x => { YLoop(x); });
            }
            else
            {
                for (int x = 0; x < nx; x++) { YLoop(x); }
            }

            return outWf;
        }
    }
}
