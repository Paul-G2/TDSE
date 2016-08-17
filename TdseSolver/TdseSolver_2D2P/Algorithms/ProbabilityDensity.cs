using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Vec2 = TdseUtils.Vec2;


namespace TdseSolver_2D2P
{
    /// <summary>
    /// This class represents a 1-particle, 2-dimensional probability density.
    /// </summary>
    partial class ProbabilityDensity
    {
        // Class data
        float[][] m_data;   // Stored in [y][x] order, for compatibility with the vtk file format
        float     m_latticeSpacing;




        /// <summary>
        /// Constructor.
        /// </summary>
        public ProbabilityDensity(int gridSizeX, int gridSizeY, float latticeSpacing)
        {
            m_data = TdseUtils.Misc.Allocate2DArray(gridSizeY, gridSizeX);
            m_latticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ProbabilityDensity(float[][] data, float latticeSpacing)
        {
            m_data = data;
            m_latticeSpacing = latticeSpacing;
        }




        /// <summary>
        /// Gets the array of density values.
        /// </summary>
        public float[][] Data
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
        public float Value(int x, int y)
        {
            return m_data[y][x];
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
        /// Computes the intergrated probability density.
        /// </summary>
        public float Norm()
        {
            float norm = 0.0f;

            int sy = GridSizeY;
            int sx = GridSizeX;

            for (int y=0; y<sy; y++)
            {
                float[] dataY = m_data[y];

                for (int x = 0; x < sx; x++)
                {
                    norm += dataY[x];
                }            
            }

            norm *= (m_latticeSpacing*m_latticeSpacing);
            return norm;
        }




       

    }
}
