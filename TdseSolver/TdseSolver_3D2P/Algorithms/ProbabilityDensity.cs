using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vec2 = TdseUtils.Vec2;


namespace TdseSolver_3D2P
{
    /// <summary>
    /// This class represents a 1-particle, 3-dimensional probability density.
    /// </summary>
    partial class ProbabilityDensity
    {
        // Class data
        float[][][] m_data;   // Stored in [z][y][x] order, for compatibility with the vtk file format
        float       m_latticeSpacing;




        /// <summary>
        /// Constructor.
        /// </summary>
        public ProbabilityDensity(GridSpec gridSpec, float latticeSpacing)
        {
            m_data = TdseUtils.Misc.Allocate3DArray(gridSpec.SizeZ, gridSpec.SizeY, gridSpec.SizeX);
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ProbabilityDensity(float[][][] data, float latticeSpacing)
        {
            m_data = data;
            m_latticeSpacing = latticeSpacing;
        }




        /// <summary>
        /// Gets the array of density values.
        /// </summary>
        public float[][][] Data
        {
            get
            {
                return m_data;
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
        /// Gets the probability density at a given location.
        /// </summary>
        public float Value(int x, int y, int z)
        {
            return m_data[z][y][x];
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
                int nx = (ny > 0) ? m_data[0][0].Length : 0;

                return new GridSpec(nx,ny,nz);
            }
        }


        /// <summary>
        /// Computes the intergrated probability density.
        /// </summary>
        public float Norm()
        {
            GridSpec gridSpec = GridSpec;
            int sx = gridSpec.SizeX;
            int sy = gridSpec.SizeY;
            int sz = gridSpec.SizeZ;

            float[] norm = new float[sz];

            TdseUtils.Misc.ForLoop(0, sz, z => 
            {
                float xySum = 0.0f;
                for (int y=0; y<sy; y++)
                {
                    float[] dataZY = m_data[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        xySum += dataZY[x];
                    }            
                }
                norm[z] = xySum;
            }, true);

            float normTot = 0.0f;
            for (int z=0; z<sz; z++) { normTot += norm[z]; }
            normTot *= (m_latticeSpacing*m_latticeSpacing*m_latticeSpacing);
            return normTot;
        }


    }
}
