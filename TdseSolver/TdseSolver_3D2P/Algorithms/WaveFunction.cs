using System;
using System.Threading.Tasks;



namespace TdseSolver_3D2P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 3 dimensions, defined on a rectangular grid.
    /// </summary>
    partial class WaveFunction
    {
        // Class data
        float[][][]  m_data;   // (Re,Im) pairs, stored in [z][y][x] order, for compatibility with the vtk file format
        float        m_latticeSpacing;



        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(GridSpec gridSpec, float latticeSpacing)
        {
            m_data = TdseUtils.Misc.Allocate3DArray(gridSpec.SizeZ, gridSpec.SizeY, 2*gridSpec.SizeX);
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveFunction(float[][][] data, float latticeSpacing)
        {
            m_data = data;
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Gets the array of wavefunction values.
        /// </summary>
        public float[][][] Data
        {
            get
            {
                return m_data;
            }
        }


        /// <summary>
        /// Gets the wavefunction amplitude at a given location.
        /// </summary>
        public float Ampl(int x, int y, int z)
        {
            float[] dataZY = m_data[z][y];
            float re = dataZY[2*x];
            float im = dataZY[2*x+1];

            return (float) Math.Sqrt(re*re + im*im);
        }


        /// <summary>
        /// Gets the squared wavefunction amplitude at a given location.
        /// </summary>
        public float Prob(int x, int y, int z)
        {
            float[] dataZY = m_data[z][y];
            float re = dataZY[2*x];
            float im = dataZY[2*x+1];

            return (re*re + im*im);
        }


        /// <summary>
        /// Gets the array of probability values.
        /// </summary>
        public ProbabilityDensity GetProbabilityDensity()
        {
            int sx = GridSpec.SizeX;
            int sy = GridSpec.SizeY;
            int sz = GridSpec.SizeZ;
            float[][][] probs = TdseUtils.Misc.Allocate3DArray(sz, sy, sx);

            TdseUtils.Misc.ForLoop(0, sz, z=>
            {
                for (int y=0; y<sy; y++)
                {
                    float[] dataZY = m_data[z][y];
                    float[] probsZY = probs[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        float re = dataZY[2*x];
                        float im = dataZY[2*x + 1];
                        probsZY[x] = re*re + im*im;
                    }
                }
            }, true );

            return new ProbabilityDensity(probs, m_latticeSpacing);
        }


        /// <summary>
        /// Gets the phase angle at a given location. (Range is -Pi to Pi).
        /// </summary>
        public float Phase(int x, int y, int z)
        {
            float[] dataZY = m_data[z][y];

            return (float) Math.Atan2(dataZY[2*x+1], dataZY[2*x]);
        }


        /// <summary>
        /// Gets the number of grid points along each direction.
        /// </summary>
        public GridSpec GridSpec
        {
            get
            {
                int nz = m_data.Length;
                int ny = (nz > 0) ? m_data[0].Length : 0;
                int nx = (ny > 0) ? m_data[0][0].Length/2 : 0;

                return new GridSpec(nx,ny,nz);
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
            int sx = GridSpec.SizeX;
            int sy = GridSpec.SizeY;
            int sz = GridSpec.SizeZ;
            int sx2 = 2*sx;
            float[] normSq = new float[sz];
            
            Parallel.For(0, sz, z =>
            {
                float xySum = 0.0f;
                for (int y = 0; y < sy; y++)
                {
                    float[] dataZY = m_data[z][y];

                    for (int nx = 0; nx < sx2; nx++)
                    {
                        float val = dataZY[nx];
                        xySum += val*val;
                    }            
                }
                normSq[z] = xySum;
            });

            float normSqTot = 0.0f;
            for (int z=0; z<sz; z++) { normSqTot += normSq[z]; }
            normSqTot *= (m_latticeSpacing*m_latticeSpacing*m_latticeSpacing);

            return normSqTot;
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
            int sx = GridSpec.SizeX;
            int sy = GridSpec.SizeY;
            int sz = GridSpec.SizeZ;
            int sx2 = 2*sx;

            Parallel.For(0, sz, z =>
            {
                for (int y = 0; y < sy; y++)
                {
                    float[] dataZY = m_data[z][y];

                    for (int nx = 0; nx < sx2; nx++)
                    {
                        dataZY[nx] *= factor;
                    }
                }
            });
        }


        /// <summary>
        /// Computes the result of applying a given Hamiltonian operator to this wavefunction.
        /// </summary>
        public WaveFunction ApplyH(float[][][] V, float mass, bool multiThread=true)
        {
            // Initialize locals
            int sx = GridSpec.SizeX;
            int sy = GridSpec.SizeY;
            int sz = GridSpec.SizeZ;
            int sx2 = 2*sx;
            
            int sx2m2 = sx2 - 2;
            int sym1  = sy  - 1;
            int szm1  = sz  - 1;

            float keFactor = (1.0f/12.0f) / (2 * mass *m_latticeSpacing * m_latticeSpacing);

            WaveFunction outWf = new WaveFunction(GridSpec, m_latticeSpacing);


            // Compute H * Wf
            TdseUtils.Misc.ForLoop(0, sz, z =>
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
                    
                    float[] inWf_z_y   = m_data[z][y];
                    float[] inWf_z_ym  = m_data[z][ym];
                    float[] inWf_z_yp  = m_data[z][yp];
                    float[] inWf_z_ymm = m_data[z][ymm];
                    float[] inWf_z_ypp = m_data[z][ypp];
                    float[] inWf_zm_y  = m_data[zm][y];
                    float[] inWf_zp_y  = m_data[zp][y];
                    float[] inWf_zmm_y = m_data[zmm][y];
                    float[] inWf_zpp_y = m_data[zpp][y];
                    float[] outWf_z_y  = outWf.m_data[z][y];
                    float[] V_z_y      = V[z][y];

                    for (int rx = 0; rx < sx2; rx+=2)
                    {
                        int rxp  = (rx  < sx2m2) ?  rx + 2  : 0;
                        int rxpp = (rxp < sx2m2) ?  rxp + 2 : 0;
                        int rxm  = (rx  > 0) ?  rx - 2 : sx2m2;
                        int rxmm = (rxm > 0) ? rxm - 2 : sx2m2;
                        int x = rx/2;

                        // Kinetic energy terms. 
                        float kR = keFactor * (
                            90.0f * inWf_z_y[rx] -
                            16.0f * (inWf_zm_y[rx] + inWf_zp_y[rx] + inWf_z_yp[rx] + inWf_z_ym[rx] + inWf_z_y[rxm] + inWf_z_y[rxp]) +
                            (inWf_zmm_y[rx] + inWf_zpp_y[rx] + inWf_z_ypp[rx] + inWf_z_ymm[rx] + inWf_z_y[rxmm] + inWf_z_y[rxpp]) 
                        );

                        int ix = rx + 1;
                        float kI = keFactor * (
                            90.0f * inWf_z_y[ix] -
                            16.0f * (inWf_zm_y[ix] + inWf_zp_y[ix] + inWf_z_yp[ix] + inWf_z_ym[ix] + inWf_z_y[rxm+1] + inWf_z_y[rxp+1]) +
                            (inWf_zmm_y[ix] + inWf_zpp_y[ix] + inWf_z_ypp[ix] + inWf_z_ymm[ix] + inWf_z_y[rxmm+1] + inWf_z_y[rxpp+1]) 
                        );

                        // Potential energy terms
                        float vR = V_z_y[x] * inWf_z_y[rx];
                        float vI = V_z_y[x] * inWf_z_y[ix];

                        outWf_z_y[rx] = kR + vR;
                        outWf_z_y[ix] = kI + vI;
                    }
                }
            }, multiThread);

            return outWf;
        }
    
    }
}
