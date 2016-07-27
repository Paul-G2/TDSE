using System;
using System.Drawing;
using System.Threading.Tasks;
using Complex = TdseUtils.Complex;
using Vec3 = TdseUtils.Vec3;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class provides miscellaneous utility methods for wavefunctions.
    /// </summary>
    class WaveFunctionUtils
    {
        /// <summary>
        /// Creates a Gaussian wavepacket with given properties.
        /// </summary>
        public static WaveFunction CreateGaussianWavePacket(
            GridSpec gridSpec, float latticeSpacing, float mass, Vec3 packetCenter, Vec3 packetWidth, Vec3 avgMomentum, bool multiThread=true)
        {
            WaveFunction wf = new WaveFunction(gridSpec, latticeSpacing);

            int sx = gridSpec.SizeX;
            int sy = gridSpec.SizeY;
            int sz = gridSpec.SizeZ;
            float[][][] wfData = wf.Data;

            Complex I = Complex.I;
            float rootPi = (float) Math.Sqrt( Math.PI );
            float sigmaXSq = packetWidth.X * packetWidth.X;
            float sigmaYSq = packetWidth.Y * packetWidth.Y;
            float sigmaZSq = packetWidth.Z * packetWidth.Z;
            float norm = (float) Math.Sqrt( (packetWidth.X/(rootPi*sigmaXSq)) * (packetWidth.Y/(rootPi*sigmaYSq)) * (packetWidth.Z/(rootPi*sigmaZSq)) );

            TdseUtils.Misc.LoopDelegate ZLoop = (z) =>
            {
                float zf = z * latticeSpacing;
                Complex expArgZ = I*zf*avgMomentum.Z - (zf - packetCenter.Z)*(zf - packetCenter.Z)/(2*sigmaZSq);
                
                for (int y = 0; y < sy; y++)
                {
                    float yf = y * latticeSpacing;
                    Complex expArgZY = expArgZ + I*yf*avgMomentum.Y - (yf - packetCenter.Y)*(yf - packetCenter.Y)/(2*sigmaYSq);
                    
                    float[] wfDataZY = wfData[z][y];
                    for (int x = 0; x < sx; x++)
                    {
                        float xf = x * latticeSpacing;

                        Complex expArgZYX = expArgZY + I*xf*avgMomentum.X - (xf - packetCenter.X)*(xf - packetCenter.X)/(2*sigmaXSq);
                        Complex wfVal = norm * Complex.Exp(expArgZYX);
 
                        wfDataZY[2*x]   = wfVal.Re;
                        wfDataZY[2*x+1] = wfVal.Im;
                    }
                }
            };
            if (multiThread)
            {
                Parallel.For(0, sz, z => { ZLoop(z); });
            }
            else
            {
                for (int z = 0; z < sz; z++) { ZLoop(z); }
            }

            wf.Normalize();
            return wf;
        }


        /// <summary>
        /// Calculates the value of a (freely evolving) Gaussian wavepacket at a given location and time. 
        /// </summary>
        public static Complex FreeGaussianWavePacketValue(float x, float y, float z, float t, 
            Vec3 initialCenter, Vec3 initialWidth, Vec3 avgMomentum, float mass )
        {
            Complex I = Complex.I;

            Complex effSigmaXSq = initialWidth.X*initialWidth.X + I*(t/mass);
            Complex effSigmaYSq = initialWidth.Y*initialWidth.Y + I*(t/mass);
            Complex effSigmaZSq = initialWidth.Z*initialWidth.Z + I*(t/mass);

            float xRel = x - initialCenter.X - avgMomentum.X*t/mass;
            float yRel = y - initialCenter.Y - avgMomentum.Y*t/mass;
            float zRel = z - initialCenter.Z - avgMomentum.Z*t/mass;

            float avgMomentumSq = avgMomentum.NormSq();

            Complex expArg = I*(x*avgMomentum.X + y*avgMomentum.Y + z*avgMomentum.Z) - I*t*avgMomentumSq/(2*mass) - 
                (xRel*xRel)/(2*effSigmaXSq) - (yRel*yRel)/(2*effSigmaYSq) - (zRel*zRel)/(2*effSigmaZSq); 

            float rootPi = (float) Math.Sqrt( Math.PI );
            Complex normX = Complex.Sqrt( initialWidth.X/(rootPi*effSigmaXSq) );
            Complex normY = Complex.Sqrt( initialWidth.Y/(rootPi*effSigmaYSq) );
            Complex normZ = Complex.Sqrt( initialWidth.Z/(rootPi*effSigmaZSq) );

            Complex wfVal = normX*normY*normZ * Complex.Exp(expArg);

            return wfVal;
        }


    }
}
