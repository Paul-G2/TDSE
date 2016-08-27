using System;
using System.Linq;
using System.Threading.Tasks;



namespace TdseSolver_2D2P_NS
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 2 dimensions, defined on a rectangular grid.
    /// </summary>
    partial class WaveFunction
    {
        // Class data
        float[][][][] m_data;   // (Re,Im) pairs, stored in [y2][x2][y1][x1] order
        float m_latticeSpacing;




        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(int gridSizeX, int gridSizeY, float latticeSpacing)
        {
            m_data = TdseUtils.Misc.Allocate4DArray(gridSizeY, gridSizeX, gridSizeY, 2*gridSizeX);
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(float[][][][] data, float latticeSpacing)
        {
            m_data = data;
            m_latticeSpacing = latticeSpacing;
        }




        /// <summary>
        /// Gets the raw array of wavefunction values.
        /// </summary>
        public float[][][][] Data
        {
            get
            {
                return m_data;
            }
        }


        /// <summary>
        /// Gets the wavefunction amplitude at a given location.
        /// </summary>
        public float Ampl(int x1, int y1, int x2, int y2)
        {
            float[] data = m_data[y2][x2][y1];
            float re = data[2*x1];
            float im = data[2*x1 + 1];

            return (float) Math.Sqrt(re*re + im*im);
        }


        /// <summary>
        /// Gets the squared wavefunction amplitude at a given location.
        /// </summary>
        public float Prob(int x1, int y1, int x2, int y2)
        {
            float[] data = m_data[y2][x2][y1];
            float re = data[2*x1];
            float im = data[2*x1 + 1];

            return (re*re + im*im);
        }


        /// <summary>
        /// Gets the phase angle at a given location. (Range is -Pi to Pi).
        /// </summary>
        public float Phase(int x1, int y1, int x2, int y2)
        {
            float[] data = m_data[y2][x2][y1];
            float re = data[2*x1];
            float im = data[2*x1 + 1];

            return (float) Math.Atan2(im, re);
        }


        /// <summary>
        /// Gets the number of grid points along the x direction.
        /// </summary>
        public int GridSizeX
        {
            get
            {
                return (m_data.Length > 0) ? m_data[0].Length : 0;
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
        public float NormSq(bool multiThread=true)
        {
            int sy = GridSizeY;
            int sx = GridSizeX;
            int sx2 = 2*sx;

            float[] ySums = new float[sy];

            TdseUtils.Misc.ForLoop(0, sy, y2 =>
            {
                float sum = 0.0f;
                for (int x2=0; x2<sx; x2++)
                {
                    for (int y1=0; y1<sy; y1++)
                    {
                        float[] dataY = m_data[y2][x2][y1];

                        for (int nx1 = 0; nx1 < sx2; nx1++)
                        {
                            float val = dataY[nx1];
                            sum += val*val;
                        }
                    }
                }
                ySums[y2] = sum;
            }, multiThread );

            float normSq = (float) ( ySums.Sum() * Math.Pow(m_latticeSpacing, 4) );
            return normSq;
        }


        /// <summary>
        /// Normalizes the wavefunction.
        /// </summary>
        public void Normalize(bool multiThread=true)
        {
            ScaleBy( (float)( 1.0/Math.Sqrt(NormSq(multiThread)) ) );
        }


        /// <summary>
        /// Multiplies the wavefunction by a given factor.
        /// </summary>
        public void ScaleBy(float factor, bool multiThread=true)
        {
            int sy = GridSizeY;
            int sx = GridSizeX;
            int sx2 = 2*sx;

            float[] partialSums = new float[sy];

            TdseUtils.Misc.ForLoop(0, sy, y2 =>
            {
                for (int x2=0; x2<sx; x2++)
                {
                    for (int y1=0; y1<sy; y1++)
                    {
                        float[] dataY = m_data[y2][x2][y1];

                        for (int nx1 = 0; nx1 < sx2; nx1++)
                        {
                            dataY[nx1] *= factor;
                        }
                    }
                }
            }, multiThread );
        }


        /// <summary>
        /// Computes the result of applying a given Hamiltonian operator to this wavefunction.
        /// </summary>
        public WaveFunction ApplyH(float[][][][] V, float mass1, float mass2, bool multiThread=true)
        {
            // Initialize locals
            int sx = GridSizeX;
            int sy = GridSizeY;
            int sxm1  = sx - 1;
            int sym1  = sy - 1;
            int sx2   = 2*sx;
            int sx2m2 = sx2 - 2;

            // Coefficents used when calculating the derivatives
            float alpha = 5.0f;
            float beta  = -4.0f / 3.0f;
            float delta = 1.0f / 12.0f;

            float keFactor1 = 1.0f / (2 * mass1 *m_latticeSpacing * m_latticeSpacing);
            float keFactor2 = 1.0f / (2 * mass2 *m_latticeSpacing * m_latticeSpacing);

            WaveFunction outWf = new WaveFunction(sx, sy, m_latticeSpacing);


            // Compute H * Wf
            TdseUtils.Misc.ForLoop( 0, sy, y2 =>
            {
                int y2p  = (y2  < sym1) ?  y2 + 1 : 0;
                int y2pp = (y2p < sym1) ? y2p + 1 : 0;
                int y2m  = (y2  > 0) ?  y2 - 1 : sym1;
                int y2mm = (y2m > 0) ? y2m - 1 : sym1;

                for (int x2 = 0; x2 < sx; x2++)
                {
                    int x2p  = (x2  < sxm1) ?  x2 + 1 : 0;
                    int x2pp = (x2p < sxm1) ? x2p + 1 : 0;
                    int x2m  = (x2  > 0) ?  x2 - 1 : sxm1;
                    int x2mm = (x2m > 0) ? x2m - 1 : sxm1;

                    float[][] inWf_y2_x2   = m_data[y2][x2];

                    for (int y1 = 0; y1 < sy; y1++)
                    {
                        int y1p  = (y1  < sym1) ?  y1 + 1 : 0;
                        int y1pp = (y1p < sym1) ? y1p + 1 : 0;
                        int y1m  = (y1  > 0) ?  y1 - 1 : sym1;
                        int y1mm = (y1m > 0) ? y1m - 1 : sym1;

                        float[] inWf_y2_x2_y1   = inWf_y2_x2[y1];
                        float[] inWf_y2_x2_y1m  = inWf_y2_x2[y1m];
                        float[] inWf_y2_x2_y1p  = inWf_y2_x2[y1p];
                        float[] inWf_y2_x2_y1mm = inWf_y2_x2[y1mm];
                        float[] inWf_y2_x2_y1pp = inWf_y2_x2[y1pp];

                        float[] inWf_y2_x2p_y1  = m_data[y2][x2p][y1];
                        float[] inWf_y2_x2m_y1  = m_data[y2][x2m][y1];
                        float[] inWf_y2_x2mm_y1 = m_data[y2][x2mm][y1];
                        float[] inWf_y2_x2pp_y1 = m_data[y2][x2pp][y1];
                        float[] inWf_y2m_x2_y1  = m_data[y2m][x2][y1];
                        float[] inWf_y2p_x2_y1  = m_data[y2p][x2][y1];
                        float[] inWf_y2mm_x2_y1 = m_data[y2mm][x2][y1];
                        float[] inWf_y2pp_x2_y1 = m_data[y2pp][x2][y1];
                        
                        float[] outWf_y2_x2_y1  = outWf.m_data[y2][x2][y1];
                        float[] V_y2_x2_y1      = V[y2][x2][y1];


                        for (int rx1 = 0; rx1 < sx2; rx1 += 2)
                        {
                            int rx1p  = (rx1  < sx2m2) ?  rx1 + 2  : 0;
                            int rx1pp = (rx1p < sx2m2) ?  rx1p + 2 : 0;
                            int rx1m  = (rx1  > 0) ?  rx1 - 2 : sx2m2;
                            int rx1mm = (rx1m > 0) ? rx1m - 2 : sx2m2;
                            int x1 = rx1/2;

                            // Kinetic energy terms
                            float kR1 = keFactor1 * (
                                alpha * inWf_y2_x2_y1[rx1] +
                                beta  * (inWf_y2_x2_y1[rx1m] + inWf_y2_x2_y1[rx1p] + inWf_y2_x2_y1m[rx1] + inWf_y2_x2_y1p[rx1]) +
                                delta * (inWf_y2_x2_y1[rx1mm] + inWf_y2_x2_y1[rx1pp] + inWf_y2_x2_y1mm[rx1] + inWf_y2_x2_y1pp[rx1])

                            );

                            float kR2 = keFactor2 * (
                                alpha * inWf_y2_x2_y1[rx1] +
                                beta  * (inWf_y2_x2m_y1[rx1]  + inWf_y2_x2p_y1[rx1]  + inWf_y2m_x2_y1[rx1]  + inWf_y2p_x2_y1[rx1]) +
                                delta * (inWf_y2_x2mm_y1[rx1] + inWf_y2_x2pp_y1[rx1] + inWf_y2mm_x2_y1[rx1] + inWf_y2pp_x2_y1[rx1])
                            );

                            int ix1 = rx1 + 1;
                            float kI1 = keFactor1 * (
                                alpha * inWf_y2_x2_y1[ix1] +
                                beta  * (inWf_y2_x2_y1[rx1m+1] + inWf_y2_x2_y1[rx1p+1] + inWf_y2_x2_y1m[ix1] + inWf_y2_x2_y1p[ix1]) +
                                delta * (inWf_y2_x2_y1[rx1mm+1] + inWf_y2_x2_y1[rx1pp+1] + inWf_y2_x2_y1mm[ix1] + inWf_y2_x2_y1pp[ix1])
                            );

                            float kI2 = keFactor2 * (
                                alpha * inWf_y2_x2_y1[ix1] +
                                beta  * (inWf_y2_x2m_y1[ix1]  + inWf_y2_x2p_y1[ix1]  + inWf_y2m_x2_y1[ix1]  + inWf_y2p_x2_y1[ix1]) +
                                delta * (inWf_y2_x2mm_y1[ix1] + inWf_y2_x2pp_y1[ix1] + inWf_y2mm_x2_y1[ix1] + inWf_y2pp_x2_y1[ix1])
                            );


                            // Potential energy terms
                            float vR = V_y2_x2_y1[x1] * inWf_y2_x2_y1[rx1];
                            float vI = V_y2_x2_y1[x1] * inWf_y2_x2_y1[ix1];

                            outWf_y2_x2_y1[rx1] = kR1 + kR2 + vR;
                            outWf_y2_x2_y1[ix1] = kI1 + kI2 + vI;
                        }
                    }
                }
            }, multiThread );

            return outWf;
        }
    }
}
