using System;
using System.Drawing;
using System.Threading.Tasks;
using TdseUtils;
using Complex = TdseUtils.Complex;
using Vec2 = TdseUtils.Vec2;
using WaveFunction2D1P = TdseSolver_2D1P.WaveFunction;


namespace TdseSolver_2D2P_NS
{
    /// <summary>
    /// This class provides miscellaneous utility methods for working with wavefunctions.
    /// </summary>
    class WaveFunctionUtils
    {

        /// <summary>
        /// Calculates the value of a (freely evolving) 2D Gaussian wavepacket at a given location and time. 
        /// </summary>
        public static Complex FreeGaussianWavePacketValue(float x, float y, float t, 
            PointF initialCenter, PointF initialWidth, PointF avgMomentum, float mass )
        {
            Complex I = Complex.I;

            Complex effSigmaXSq = initialWidth.X*initialWidth.X + I*(t/mass);
            Complex effSigmaYSq = initialWidth.Y*initialWidth.Y + I*(t/mass);

            float xRel = x - initialCenter.X - avgMomentum.X*t/mass;
            float yRel = y - initialCenter.Y - avgMomentum.Y*t/mass;

            float avgMomentumSq = avgMomentum.X*avgMomentum.X + avgMomentum.Y*avgMomentum.Y;
            Complex expArg = I*(x*avgMomentum.X + y*avgMomentum.Y) - I*t*avgMomentumSq/(2*mass) - (xRel*xRel)/(2*effSigmaXSq) - (yRel*yRel)/(2*effSigmaYSq); 

            float rootPi = (float) Math.Sqrt( Math.PI );
            Complex normX = Complex.Sqrt( initialWidth.X/(rootPi*effSigmaXSq) );
            Complex normY = Complex.Sqrt( initialWidth.Y/(rootPi*effSigmaYSq) );

            Complex wfVal = normX*normY * Complex.Exp(expArg);

            return wfVal;
        }
        

        /// <summary>
        /// Computes a 2D harmonic oscillator wavefunction with given quantum numbers.
        /// </summary>
        public static WaveFunction2D1P GetSHOWaveFunction (
            int gridSizeX, int gridSizeY, float latticeSpacing, float mass,
            Vec2 packetCenter, float sigma, int N, int Lz, bool multiThread=true)
        {
            // Check the input
            if ( (N < 0) || (Lz > N) || (Lz < -N) || ((N+Lz)%2) != 0 )
            {
                throw new ArgumentException("Invalid (N,m) in WaveFunctionUtils.GetSHOWaveFunction.");
            }

            EigenSystem hamX = Diagonalized1dShoHamiltonian(mass, sigma, packetCenter.X, latticeSpacing, gridSizeX);
            EigenSystem hamY = Diagonalized1dShoHamiltonian(mass, sigma, packetCenter.Y, latticeSpacing, gridSizeY);
            Vector eivcX0 = hamX.EigenVector(0);
            Vector eivcY0 = hamY.EigenVector(0);
            
            WaveFunction2D1P result;
            if ( (N == 0) && (Lz == 0) )
            {
                 result = DirectProduct(hamX.EigenVector(0), hamY.EigenVector(0), latticeSpacing);
            }
            else if ( (N == 1) && (Lz == 1) )
            {
                 WaveFunction2D1P psi_10 = DirectProduct(hamX.EigenVector(1), hamY.EigenVector(0), latticeSpacing);
                 WaveFunction2D1P psi_01 = DirectProduct(hamX.EigenVector(0), hamY.EigenVector(1), latticeSpacing);
                 result = psi_10 + Complex.I*psi_01;
            }
            else if ( (N == 2) && (Lz == 0) )
            {
                 WaveFunction2D1P psi_20 = DirectProduct(hamX.EigenVector(2), hamY.EigenVector(0), latticeSpacing);
                 WaveFunction2D1P psi_02 = DirectProduct(hamX.EigenVector(0), hamY.EigenVector(2), latticeSpacing);
                 result = psi_20 + psi_02;
            }
            else if ( (N == 2) && (Lz == 2) )
            {
                 WaveFunction2D1P psi_11 = DirectProduct(hamX.EigenVector(1), hamY.EigenVector(1), latticeSpacing);
                 WaveFunction2D1P psi_20 = DirectProduct(hamX.EigenVector(2), hamY.EigenVector(0), latticeSpacing);
                 WaveFunction2D1P psi_02 = DirectProduct(hamX.EigenVector(0), hamY.EigenVector(2), latticeSpacing);
                 result = psi_20 - psi_02 + (Math.Sqrt(2)*Complex.I)*psi_11;
            }
            else
            {
                throw new ArgumentException("Invalid (N,Lz) in WaveFunctionUtils.GetSHOWaveFunction.");
            }

            result.Normalize();
            return result;
        }


        /// <summary>
        /// Creates and diagonalizes a 1D SHO Hamiltonian.
        /// </summary>
        private static EigenSystem Diagonalized1dShoHamiltonian(float mass, float sigma, float center, float latticeSpacing, int hamSize)
        {
            float Vcoeff = (float) ( 1.0/(2*mass*Math.Pow(sigma,4)) );
            float alpha = 2.5f;
            float beta  = -4.0f / 3.0f;
            float gamma = 1.0f / 12.0f;
            float keFactor = 1.0f / (2 * mass *latticeSpacing * latticeSpacing);

            Matrix ham = new Matrix(hamSize, hamSize);
            for (int i=0; i<hamSize; i++)
            {
                float V = Vcoeff * (i*latticeSpacing - center) * (i*latticeSpacing - center);

                ham[i,i] = V + keFactor*alpha;

                int ip  = (i  < hamSize-1) ?  i + 1 : 0;
                int ipp = (ip < hamSize-1) ? ip + 1 : 0;
                int im  = (i  > 0) ?  i - 1 : hamSize-1;
                int imm = (im > 0) ? im - 1 : hamSize-1;

                ham[i, im]  = ham[im, i]  = keFactor*beta;
                ham[i, ip]  = ham[ip, i]  = keFactor*beta;
                ham[i, imm] = ham[imm, i] = keFactor*gamma;
                ham[i, ipp] = ham[ipp, i] = keFactor*gamma;
            }

            return new EigenSystem(ham);
        }


        /// <summary>
        /// Forms the direct product of two wavefunctions
        /// </summary>
        private static WaveFunction2D1P DirectProduct(Vector wfx, Vector wfy, float latticeSpacing)
        {
            // Construct the 2D wavefunction
            int sx = wfx.Length;
            int sy = wfy.Length;
            WaveFunction2D1P wfdp = new WaveFunction2D1P(sx, sy, latticeSpacing);

            for (int y = 0; y < sy; y++)
            {                
                double wfyValue = wfy[y];
                float[] wfdpDataY = wfdp.Data[y];

                for (int x = 0; x < sx; x++)
                {
                    double wfxValue = wfx[x];

                    wfdpDataY[2*x]   = (float) (wfxValue * wfyValue);
                    wfdpDataY[2*x+1] = 0.0f;
                }
            }

            return wfdp;
        }
    }
}
