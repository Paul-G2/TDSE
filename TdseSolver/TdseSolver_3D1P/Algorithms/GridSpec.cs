using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class encapsulates the parameters that define a 3d grid. 
    /// </summary>
    class GridSpec
    {
        public int   SizeX; 
        public int   SizeY; 
        public int   SizeZ;


        /// <summary>
        /// Constructor.
        /// </summary>
        public GridSpec(int sizeX, int sizeY, int sizeZ)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }


        /// <summary>
        /// Copy constructor.
        /// </summary>
        public GridSpec(GridSpec src)
        {
            SizeX = src.SizeX;
            SizeY = src.SizeY;
            SizeZ = src.SizeZ;
        }


        /// <summary>
        /// Compares two GridSpecs by value.
        /// </summary>
        public bool ValueEquals(GridSpec that)
        {
            return ( (this.SizeX == that.SizeX) && (this.SizeY == that.SizeY) && (this.SizeZ == that.SizeZ) );
        }
    }
}
