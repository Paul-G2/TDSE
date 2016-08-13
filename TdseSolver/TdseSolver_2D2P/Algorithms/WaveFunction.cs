using System;
using System.Threading.Tasks;



namespace TdseSolver_2D2P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 2 dimensions, defined on a rectangular grid.
    /// </summary>
    partial class WaveFunction
    {
        // Class data
        float[][] m_data;   // (Re,Im) pairs, stored in [y][x] order, for compatibility with the vtk file format
        float     m_latticeSpacing;




        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(int gridSizeX, int gridSizeY, float latticeSpacing)
        {
            m_data = TdseUtils.Misc.Allocate2DArray(gridSizeY, 2*gridSizeX);
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(float[][] data, float latticeSpacing)
        {
            m_data = data;
            m_latticeSpacing = latticeSpacing;
        }




        /// <summary>
        /// Gets the array of wavefunction values.
        /// </summary>
        public float[][] Data
        {
            get
            {
                return m_data;
            }
        }


        /// <summary>
        /// Gets the wavefunction amplitude at a given location.
        /// </summary>
        public float Ampl(int x, int y)
        {
            float[] dataY = m_data[y];
            float re = dataY[2*x];
            float im = dataY[2*x+1];

            return (float) Math.Sqrt(re*re + im*im);
        }


        /// <summary>
        /// Gets the squared wavefunction amplitude at a given location.
        /// </summary>
        public float Prob(int x, int y)
        {
            float[] dataY = m_data[y];
            float re = dataY[2*x];
            float im = dataY[2*x+1];

            return (re*re + im*im);
        }


        /// <summary>
        /// Gets the array of probability values.
        /// </summary>
        public ProbabilityDensity GetProbabilityDensity()
        {
            int sx = GridSizeX;
            int sy = GridSizeY;
            float[][] probs = TdseUtils.Misc.Allocate2DArray(sy, sx);

            for (int y=0; y<sy; y++)
            {
                float[] dataY = m_data[y];
                float[] probsY = probs[y];

                for (int x = 0; x < sx; x++)
                {
                    float re = dataY[2*x];
                    float im = dataY[2*x + 1];
                    probsY[x] = re*re + im*im;
                }            
            }

            return new ProbabilityDensity(probs, m_latticeSpacing);
        }


        /// <summary>
        /// Gets the phase angle at a given location. (Range is -Pi to Pi).
        /// </summary>
        public float Phase(int x, int y)
        {
            float[] dataY = m_data[y];

            return (float) Math.Atan2(dataY[2*x+1], dataY[2*x]);
        }


        /// <summary>
        /// Gets the number of grid points along the x direction.
        /// </summary>
        public int GridSizeX
        {
            get
            {
                return (m_data.Length > 0) ? m_data[0].Length/2 : 0;
            }
        }


        /// <summary>
        /// Gets the number of grid points along the y direction.
        /// </summary>
        public int GridSizeY
        {
            get
            {
                return m_data.Length;
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

            int sy = GridSizeY;
            int sx2 = 2*GridSizeX;

            for (int y=0; y<sy; y++)
            {
                float[] dataY = m_data[y];

                for (int nx = 0; nx < sx2; nx++)
                {
                    float val = dataY[nx];
                    normSq += val*val;
                }            
            }

            normSq *= (m_latticeSpacing*m_latticeSpacing);
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
            int sy = GridSizeY;
            int sx2 = 2*GridSizeX;

            for (int y=0; y<sy; y++)
            {
                float[] dataY = m_data[y];

                for (int nx = 0; nx < sx2; nx++)
                {
                    dataY[nx] *= factor;
                }        
            }
        }


        /// <summary>
        /// Computes the result of applying a given Hamiltonian operator to this wavefunction.
        /// </summary>
        public WaveFunction ApplyH(float[][] V, float mass, bool multiThread=true)
        {
            // Initialize locals
            int sx = GridSizeX;
            int sy = GridSizeY;
            int sxm1  = sx - 1;
            int sym1  = sy - 1;
            int sxm2  = sx - 2;
            int sym2  = sy - 2;
            int sx2   = 2*sx;
            int sx2m2 = sx2 - 2;

            float keFactor = 1.0f / (2 * mass *m_latticeSpacing * m_latticeSpacing);

            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;

            WaveFunction outWf = new WaveFunction(sx, sy, m_latticeSpacing);


            // Compute H * Wf
            TdseUtils.Misc.ForLoop( 0, sy, (y) =>
            {
                int yp  = (y  < sym1) ?  y + 1 : 0;
                int ypp = (yp < sym1) ? yp + 1 : 0;
                int ym  = (y  > 0) ?  y - 1 : sym1;
                int ymm = (ym > 0) ? ym - 1 : sym1;

                float[] inWf_y   = m_data[y];
                float[] inWf_ym  = m_data[ym];
                float[] inWf_yp  = m_data[yp];
                float[] inWf_ymm = m_data[ymm];
                float[] inWf_ypp = m_data[ypp];
                float[] outWf_y  = outWf.m_data[y];
                float[] V_y      = V[y];

                for (int rx = 0; rx < sx2; rx+=2)
                {
                    int rxp  = (rx  < sx2m2) ?  rx + 2  : 0;
                    int rxpp = (rxp < sx2m2) ?  rxp + 2 : 0;
                    int rxm  = (rx  > 0) ?  rx - 2 : sx2m2;
                    int rxmm = (rxm > 0) ? rxm - 2 : sx2m2;
                    int x = rx/2;

                    // Kinetic energy terms. 
                    float kR = keFactor * (
                        alpha * inWf_y[rx] +
                        beta  * (inWf_y[rxm] + inWf_y[rxp] + inWf_ym[rx] + inWf_yp[rx]) +
                        delta * (inWf_y[rxmm] + inWf_y[rxpp] + inWf_ymm[rx] + inWf_ypp[rx]) 
                    );

                    int ix = rx + 1;
                    float kI = keFactor * (
                        alpha * inWf_y[ix] +
                        beta  * (inWf_y[rxm+1] + inWf_y[rxp+1] + inWf_ym[ix] + inWf_yp[ix]) +
                        delta * (inWf_y[rxmm+1] + inWf_y[rxpp+1] + inWf_ymm[ix] + inWf_ypp[ix]) 
                    );

                    // Potential energy terms
                    float vR = V_y[x] * inWf_y[rx];
                    float vI = V_y[x] * inWf_y[ix];

                    outWf_y[rx] = kR + vR;
                    outWf_y[ix] = kI + vI;
                }
            }, multiThread );

            return outWf;
        }
    }
}
