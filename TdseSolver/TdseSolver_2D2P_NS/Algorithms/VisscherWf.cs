using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveFunction2D1P = TdseSolver_2D1P.WaveFunction;

namespace TdseSolver_2D2P_NS
{
    /// <summary>
    /// This class represents a 2-particle wavefunction in 2 dimensions.
    /// 
    /// It differs from the Wavefunction class by its use of the Visscher representation, ie, 
    /// the real and imaginary parts of the wavefunction are evaluated at slightly different time points.
    /// (P.B. Visscher, Computers in Physics, 5 (6), 596-598.)
    /// </summary>
    class VisscherWf
    {
        public float[][][][] ImagPartM; // Imaginary part at time t - dt/2
        public float[][][][] RealPart;  // Real part at time t
        public float[][][][] ImagPartP; // Imaginary part at time t + dt/2
        public float LatticeSpacing = 1.0f;



        /// <summary>
        /// Constructor. Initializes a Visscher wavefunction from a direct product of two ordinary wavefunctions.
        /// </summary>
        public VisscherWf(WaveFunction2D1P inputWf1, WaveFunction2D1P inputWf2, float[][] V1, float[][] V2, float mass1, float mass2, float dt, bool multiThread=true)
        {
            int sx = inputWf1.GridSizeX;
            int sy = inputWf1.GridSizeY;
            LatticeSpacing = inputWf1.LatticeSpacing;

            // Allocate the arrays
            RealPart  = TdseUtils.Misc.Allocate4DArray(sy, sx, sy, sx);
            ImagPartM = TdseUtils.Misc.Allocate4DArray(sy, sx, sy, sx);
            ImagPartP = TdseUtils.Misc.Allocate4DArray(sy, sx, sy, sx);

            // Get the real part of the total wf from the direct product of wf1*wf2, at time 0
            TdseUtils.Misc.ForLoop(0, sy, y2 =>
            {
                for (int x2 = 0; x2 < sx; x2++)
                {
                    float[][] RealPart_y2_x2 = RealPart[y2][x2];
                    float wf2_y2_x2r = inputWf2.Data[y2][2*x2];
                    float wf2_y2_x2i = inputWf2.Data[y2][2*x2 + 1];

                    for (int y1 = 0; y1 < sy; y1++)
                    {
                        float[] wf1_y1 = inputWf1.Data[y1];
                        for (int x1 = 0; x1 < sx; x1++)
                        {
                            RealPart_y2_x2[y1][x1] = wf1_y1[2*x1] * wf2_y2_x2r  -  wf1_y1[2*x1 + 1] * wf2_y2_x2i;
                        }
                    }
                }
            }, multiThread);
            

            // Get the imaginary parts of the total wf from the direct product of wf1*wf2, at time +/- dt.
            // To compute the latter, use a power series expansion of the time-evolution operator, accurate to 2nd order in H*dt
            if (V1 == null) { V1 = TdseUtils.Misc.Allocate2DArray(sy, sx); } // Set V1 to zero
            WaveFunction2D1P H_Wf1   = inputWf1.ApplyH(V1, mass1, multiThread);
            WaveFunction2D1P H2_Wf1  = H_Wf1.ApplyH(V1, mass1, multiThread);
            WaveFunction2D1P Wf1_Adv = inputWf1 + (dt/2)*H_Wf1 + (dt*dt/8)*H2_Wf1;
            WaveFunction2D1P Wf1_Ret = inputWf1 - (dt/2)*H_Wf1 + (dt*dt/8)*H2_Wf1;

            if (V2 == null) { V2 = TdseUtils.Misc.Allocate2DArray(sy, sx); } // Set V2 to zero
            WaveFunction2D1P H_Wf2  = inputWf2.ApplyH(V2, mass2, multiThread);
            WaveFunction2D1P H2_Wf2 = H_Wf2.ApplyH(V2, mass2, multiThread);
            WaveFunction2D1P Wf2_Adv = inputWf2 + (dt/2)*H_Wf2 + (dt*dt/8)*H2_Wf2;
            WaveFunction2D1P Wf2_Ret = inputWf2 - (dt/2)*H_Wf2 + (dt*dt/8)*H2_Wf2;

            TdseUtils.Misc.ForLoop(0, sy, y2 =>
            {
                for (int x2 = 0; x2 < sx; x2++)
                {
                    float[][] ImagPartP_y2_x2 = ImagPartP[y2][x2];
                    float[][] ImagPartM_y2_x2 = ImagPartM[y2][x2];

                    float wf2adv_y2_x2r = Wf2_Adv.Data[y2][2*x2];
                    float wf2adv_y2_x2i = Wf2_Adv.Data[y2][2*x2 + 1];

                    float wf2ret_y2_x2r = Wf2_Ret.Data[y2][2*x2];
                    float wf2ret_y2_x2i = Wf2_Ret.Data[y2][2*x2 + 1];

                    for (int y1 = 0; y1 < sy; y1++)
                    {
                        float[] wf1adv_y1 = Wf1_Adv.Data[y1];
                        float[] wf1ret_y1 = Wf1_Ret.Data[y1];

                        for (int x1 = 0; x1 < sx; x1++)
                        {
                            ImagPartP_y2_x2[y1][x1] = wf1adv_y1[2*x1+1] * wf2adv_y2_x2r  +  wf1adv_y1[2*x1] * wf2adv_y2_x2i;
                            ImagPartM_y2_x2[y1][x1] = wf1ret_y1[2*x1+1] * wf2ret_y2_x2r  +  wf1ret_y1[2*x1] * wf2ret_y2_x2i;
                        }
                    }
                }
            }, multiThread);
          
        }


        /// <summary>
        /// Gets the number of grid points along the y direction.
        /// </summary>
        public int GridSizeY
        {
            get
            {
                return RealPart.Length;
            }
        }


        /// <summary>
        /// Gets the number of grid points along the x direction.
        /// </summary>
        public int GridSizeX
        {
            get
            {
                return (RealPart.Length > 0) ? RealPart[0].Length : 0;
            }
        }


    }
}
