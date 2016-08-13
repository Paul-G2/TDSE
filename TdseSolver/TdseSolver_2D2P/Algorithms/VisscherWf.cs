using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdseSolver_2D2P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 2 dimensions.
    /// 
    /// It differs from the Wavefunction class by its use of the Visscher representation, ie, 
    /// the real and imaginary parts of the wavefunction are evaluated at slightly different time points.
    /// (P.B. Visscher, Computers in Physics, 5 (6), 596-598.)
    /// </summary>
    class VisscherWf
    {
        public float[][] ImagPartM; // Imaginary part at time t - dt/2
        public float[][] RealPart;  // Real part at time t
        public float[][] ImagPartP; // Imaginary part at time t + dt/2
        public float LatticeSpacing = 1.0f;



        /// <summary>
        /// Constructor. Initializes a Visscher wavefunction from an ordinary wavefunction.
        /// </summary>
        public VisscherWf(WaveFunction inputWf, float[][] V, float mass, float dt, bool multiThread=true)
        {
            int sx = inputWf.GridSizeX;
            int sy = inputWf.GridSizeY;
            LatticeSpacing = inputWf.LatticeSpacing;

            // Allocate the arrays
            RealPart  = TdseUtils.Misc.Allocate2DArray(sy, sx);
            ImagPartM = TdseUtils.Misc.Allocate2DArray(sy, sx);
            ImagPartP = TdseUtils.Misc.Allocate2DArray(sy, sx);

            // For the real part, just copy the values from the input wavefunction.
            for (int y = 0; y < sy; y++)
            {
                float[] realPartY = RealPart[y];
                float[] inputWfY = inputWf.Data[y];
                for (int x = 0; x < sx; x++)
                {
                    realPartY[x] = inputWfY[2*x];
                }
            }
            

            // For the imaginary parts, we need to compute the time evolutions.
            // We use a power series expansion of the time-evolution operator, accurate to 2nd oder in H*dt
            WaveFunction H_PsiIn  = inputWf.ApplyH(V, mass, multiThread);
            WaveFunction H2_PsiIn = H_PsiIn.ApplyH(V, mass, multiThread);

            float halfDt = dt/2;
            float eighthDt2 = dt*dt/8;

            TdseUtils.Misc.ForLoop(0, sy, (y) =>
            {
                float[] imPY      = this.ImagPartP[y];
                float[] imMY      = this.ImagPartM[y];
                float[] inputWfY  = inputWf.Data[y];
                float[] H_PsiInY  = H_PsiIn.Data[y];
                float[] H2_PsiInY = H2_PsiIn.Data[y];

                for (int x = 0; x < sx; x++)
                {
                    int x2 = 2*x;
                    int x2p1 = x2 + 1;

                    float dt0Term  = inputWfY[x2p1];
                    float dt1Term  = halfDt * H_PsiInY[x2];
                    float dt2Term  = eighthDt2 * H2_PsiInY[x2p1];

                    imPY[x] = dt0Term - dt1Term - dt2Term; // [1 - i*H*(dt/2) - (1/2)H^2*(dt/2)^2] * Psi
                    imMY[x] = dt0Term + dt1Term - dt2Term; // [1 + i*H*(dt/2) - (1/2)H^2*(dt/2)^2] * Psi
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


        /// <summary>
        /// Converts a Visscher wavefunction to a regular wavefunction.
        /// </summary>
        public WaveFunction ToRegularWavefunction(bool multiThread=true)
        {
            int sx = GridSizeX;
            int sy = GridSizeY;

            WaveFunction result = new WaveFunction(sx, sy, LatticeSpacing);

            TdseUtils.Misc.ForLoop(0, sy, (y) =>
            {
                float[] thisRy  = RealPart[y];
                float[] thisIMy = ImagPartM[y];
                float[] thisIPy = ImagPartP[y];
                float[] outWfy  = result.Data[y];

                for (int x = 0; x < sx; x++)
                {
                    outWfy[2*x] = thisRy[x];

                    // Take the square root of Im(t-dt/2) * I(t+dt/2), as this is the quantity that gives a conserved probability. 
                    float IM = thisIMy[x];
                    float IP = thisIPy[x];
                    float Iproduct = IM * IP;
                        
                    float Iout;
                    if (Iproduct > 0.0f)
                    {
                        Iout = (float) Math.Sqrt(Iproduct);
                        if (IM < 0.0f) { Iout = -Iout; }
                    }
                    else
                    {
                        Iout = 0.0f;
                    }

                    outWfy[2*x+1] = Iout;
                }
            }, multiThread );

            return result;
        }

    }
}
