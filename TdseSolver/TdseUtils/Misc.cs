using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdseUtils
{
    /// <summary>
    /// This class provides miscellaneous utility methods.
    /// </summary>
    public class Misc
    {
        /// <summary>
        /// Allocates a 2D array of floats.
        /// </summary>
        public static float[][] Allocate2DArray(int n0, int n1)
        {
            float[][] a = new float[n0][];

            for (int i = 0; i < n0; i++) 
            { 
                a[i] = new float[n1]; 
            }

            return a;
        }


        /// <summary>
        /// Allocates a 3D array of floats.
        /// </summary>
        public static float[][][] Allocate3DArray(int n0, int n1, int n2)
        {
            float[][][] a = new float[n0][][];
            for (int i = 0; i < n0; i++) 
            { 
                a[i] = new float[n1][]; 
                for (int j = 0; j < n1; j++) 
                { 
                    a[i][j] = new float[n2]; 
                }
            }

            return a;
        }


        /// <summary>
        /// Copies a 2D array of floats.
        /// </summary>
        public static void Copy2DArray(float[][] src, float[][] dest)
        {
            int n0 = src.Length;
            int n1 = src[0].Length;

            if ( (n0 != dest.Length) || (n1 != dest[0].Length) )
            {
                throw new ArgumentException("Array size mismatch, in TdseUtils.Misc.Copy2DArray");
            }

            for (int i = 0; i < n0; i++) 
            { 
                Buffer.BlockCopy(src[i], 0, dest[i], 0, 4*n1);
            }
        }

    }
}
