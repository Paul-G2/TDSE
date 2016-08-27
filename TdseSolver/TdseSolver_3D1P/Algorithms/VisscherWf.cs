using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 3 dimensions.
    /// 
    /// It differs from the Wavefunction class by its use of the Visscher representation, ie, 
    /// the real and imaginary parts of the wavefunction are evaluated at slightly different time points.
    /// (P.B. Visscher, Computers in Physics, 5 (6), 596-598.)
    /// </summary>
    class VisscherWf
    {
        public float[][][] ImagPartM; // Imaginary part at time t - dt/2
        public float[][][] RealPart;  // Real part at time t
        public float[][][] ImagPartP; // Imaginary part at time t + dt/2
        public float LatticeSpacing = 1.0f;


        /// <summary>
        /// Constructor. Initializes a Visscher wavefunction from an ordinary wavefunction.
        /// </summary>
        public VisscherWf(WaveFunction inputWf, float[][][] V, float mass, float dt, bool multiThread=true)
        {
            int sx = inputWf.GridSpec.SizeX;
            int sy = inputWf.GridSpec.SizeY;
            int sz = inputWf.GridSpec.SizeZ;
            LatticeSpacing = inputWf.LatticeSpacing;

            // Allocate the arrays
            RealPart  = TdseUtils.Misc.Allocate3DArray(sz, sy, sx);
            ImagPartM = TdseUtils.Misc.Allocate3DArray(sz, sy, sx);
            ImagPartP = TdseUtils.Misc.Allocate3DArray(sz, sy, sx);

            // For the real part, just copy the values from the input wavefunction.
            for (int z = 0; z < sz; z++)
            {
                for (int y = 0; y < sy; y++)
                {
                    float[] realPartZY = RealPart[z][y];
                    float[] inputWfZY = inputWf.Data[z][y];
                    for (int x = 0; x < sx; x++)
                    {
                        realPartZY[x] = inputWfZY[2*x];
                    }
                }
            }

            // For the imaginary parts, we need to compute the time evolutions.
            // We use a power series expansion of the time-evolution operator, accurate to 2nd order in H*dt
            WaveFunction H_PsiIn  = inputWf.ApplyH(V, mass, multiThread);
            WaveFunction H2_PsiIn = H_PsiIn.ApplyH(V, mass, multiThread);

            float halfDt = dt/2;
            float eighthDt2 = dt*dt/8;

            TdseUtils.Misc.ForLoop(0, sz, (z) =>
            {
                for (int y = 0; y < sy; y++)
                {
                    float[] imPZY      = this.ImagPartP[z][y];
                    float[] imMZY      = this.ImagPartM[z][y];
                    float[] inputWfZY  = inputWf.Data[z][y];
                    float[] H_PsiInZY  = H_PsiIn.Data[z][y];
                    float[] H2_PsiInZY = H2_PsiIn.Data[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        int x2 = 2*x;
                        int x2p1 = x2 + 1;

                        float dt0Term  = inputWfZY[x2p1];
                        float dt1Term  = halfDt * H_PsiInZY[x2];
                        float dt2Term  = eighthDt2 * H2_PsiInZY[x2p1];

                        imPZY[x] = dt0Term - dt1Term - dt2Term; // [1 - i*H*(dt/2) - (1/2)H^2*(dt/2)^2] * Psi
                        imMZY[x] = dt0Term + dt1Term - dt2Term; // [1 + i*H*(dt/2) - (1/2)H^2*(dt/2)^2] * Psi
                    }
                }
            }, multiThread );           
        }


        /// <summary>
        /// Gets the number of grid points along each direction.
        /// </summary>
        public GridSpec GridSpec
        {
            get
            {
                int nz = RealPart.Length;
                int ny = (nz > 0) ? RealPart[0].Length : 0;
                int nx = (ny > 0) ? RealPart[0][0].Length : 0;

                return new GridSpec(nx,ny,nz);
            }
        }


        /// <summary>
        /// Converts a Visscher wavefunction to a regular wavefunction.
        /// </summary>
        public WaveFunction ToRegularWavefunction(bool multiThread=true)
        {
            int sx = GridSpec.SizeX;
            int sy = GridSpec.SizeY;
            int sz = GridSpec.SizeZ;

            WaveFunction result = new WaveFunction(GridSpec, LatticeSpacing);

            TdseUtils.Misc.ForLoop(0, sz, (z) =>
            {
                for (int y = 0; y < sy; y++)
                {
                    float[] thisRzy   = RealPart[z][y];
                    float[] thisIMzy  = ImagPartM[z][y];
                    float[] thisIPzy  = ImagPartP[z][y];
                    float[] outWfzy   = result.Data[z][y];

                    for (int x = 0; x < sx; x++)
                    {
                        outWfzy[2*x] = thisRzy[x];

                        // Take the square root of Im(t-dt/2) * I(t+dt/2), as this is the quantity that gives a conserved probability. 
                        float IM = thisIMzy[x];
                        float IP = thisIPzy[x];
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

                        outWfzy[2*x+1] = Iout;
                    }
                }
            }, multiThread );           

            return result;
        }

    }
}
