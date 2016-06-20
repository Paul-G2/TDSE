using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdseSolver_2D1P
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
        public float LatticeSpacing = 2.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        public VisscherWf(int sizeX, int sizeY, float latticeSpacing)
        {
            // Allocate the arrays
            RealPart  = new float[sizeX][];
            ImagPartM = new float[sizeX][];
            ImagPartP = new float[sizeX][];

            for (int i=0; i<sizeX; i++)
            {
                RealPart[i ] = new float[sizeY];
                ImagPartM[i] = new float[sizeY];
                ImagPartP[i] = new float[sizeY];
            }

            LatticeSpacing = latticeSpacing;
        }


        /// <summary>
        /// Constructor. Initializes a Visscher wavefunction from an ordinary wavefunction.
        /// </summary>
        public VisscherWf(WaveFunction inputWf, float[][]V, float mass, float dt, bool multiThread=true)
        {
            LatticeSpacing = inputWf.LatticeSpacing;

            int sizeX = inputWf.GridSizeX;
            int sizeY = inputWf.GridSizeY;

            // Allocate the arrays
            RealPart  = new float[sizeX][];
            ImagPartM = new float[sizeX][];
            ImagPartP = new float[sizeX][];
            for (int i=0; i<sizeX; i++)
            {
                RealPart[i]  = new float[sizeY];
                ImagPartM[i] = new float[sizeY];
                ImagPartP[i] = new float[sizeY];
            }


            // For the real part, just copy the values from the input wavefunction.
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    RealPart[x][y] = inputWf.RealPart[x][y];
                }
            }

            // For the imaginary parts, we need to compute the time evolutions.
            // We use a power series expansion of the time-evolution operator, accurate to 2nd oder in H*dt
            WaveFunction H_PsiIn  = inputWf.ApplyH(V, mass, multiThread);
            WaveFunction H2_PsiIn = H_PsiIn.ApplyH(V, mass, multiThread);

            float halfDt = dt/2;
            float eighthDt2 = dt*dt/8; 
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    this.ImagPartP[x][y] = inputWf.ImagPart[x][y] - halfDt*H_PsiIn.RealPart[x][y] - eighthDt2*H2_PsiIn.ImagPart[x][y]; // [1 - i*H*(dt/2) - (1/2)H^2*(dt/2)^2] * Psi
                    this.ImagPartM[x][y] = inputWf.ImagPart[x][y] + halfDt*H_PsiIn.RealPart[x][y] - eighthDt2*H2_PsiIn.ImagPart[x][y]; // [1 + i*H*(dt/2) - (1/2)H^2*(dt/2)^2] * Psi
                }
            }
        }


        /// <summary>
        /// Gets the number of grid points along the x direction.
        /// </summary>
        public int GridSizeX
        {
            get
            {
                return RealPart.Length;
            }
        }


        /// <summary>
        /// Gets the number of grid points along the y direction.
        /// </summary>
        public int GridSizeY
        {
            get
            {
                return (RealPart.Length > 0) ? RealPart[0].Length : 0;
            }
        }


        /// <summary>
        /// Converts a Visscher wavefunction to a regular wavefunction.
        /// </summary>
        public WaveFunction ToRegularWavefunction()
        {
            int nx = GridSizeX;
            int ny = GridSizeY;

            WaveFunction result = new WaveFunction(nx, ny, LatticeSpacing);

            for (int x = 0; x < nx; x++)
            {
                float[] inWfR   = RealPart[x];
                float[] inWfIm  = ImagPartM[x];
                float[] inWfIp  = ImagPartP[x];
                float[] outWfR  = result.RealPart[x];
                float[] outWfI  = result.ImagPart[x];

                for (int y = 0; y < ny; y++)
                {
                    outWfR[y] = inWfR[y];

                    // Take the square root of Im(t-dt/2) * I(t+dt/2), as this is the quantity that gives a conserved probability
                    float imm = inWfIm[y];
                    float imp = inWfIp[y];
                    float imProduct = imm*imp;
                    float im = (imProduct <= 0.0f) ? 0.0f : (float)Math.Sqrt(imProduct);

                    // Get the sign right
                    if (im != 0.0f)
                    {
                        if ( (imProduct > 0.0f)  ) // They have the same sign
                        {
                            if (imm < 0.0f) { im = -im; }
                        }
                        else // They have opposite signs
                        {
                            int sign = (Math.Abs(imm) > Math.Abs(imp)) ? Math.Sign(imm) : Math.Sign(imp);
                            im *= sign;
                        }
                    }
                    outWfI[y] = im;
                }
            }




            return result;
        }

    }
}
