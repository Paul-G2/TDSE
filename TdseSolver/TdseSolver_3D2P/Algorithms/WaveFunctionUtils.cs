using System;
using System.Drawing;
using System.Threading.Tasks;
using Complex = TdseUtils.Complex;
using Vec3 = TdseUtils.Vec3;


namespace TdseSolver_3D2P
{
    /// <summary>
    /// This class provides miscellaneous utility methods for working with wavefunctions.
    /// </summary>
    class WaveFunctionUtils
    {
        /// <summary>
        /// Creates a Gaussian wavepacket with given properties.
        /// </summary>
        public static WaveFunction CreateGaussianWavePacket(
            GridSpec gridSpec, float latticeSpacing, bool originAtLatticeCenter, float mass,
            Vec3 packetCenter, Vec3 packetWidth, Vec3 avgMomentum, bool multiThread=true)
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
            float norm = (float) Math.Sqrt( 1.0/(rootPi*packetWidth.X * rootPi*packetWidth.Y * rootPi*packetWidth.Z) );
            int halfGridSizeX = (sx - 1)/2;
            int halfGridSizeY = (sy - 1)/2;
            int halfGridSizeZ = (sz - 1)/2;

            TdseUtils.Misc.ForLoop(0, sz, (z) =>
            {
                float zf = (originAtLatticeCenter) ? (z - halfGridSizeZ)*latticeSpacing : (z * latticeSpacing);
                Complex expArgZ = I*zf*avgMomentum.Z - (zf - packetCenter.Z)*(zf - packetCenter.Z)/(2*sigmaZSq);

                for (int y=0; y<sy; y++)
                {
                    float yf = (originAtLatticeCenter) ? (y - halfGridSizeY)*latticeSpacing : (y * latticeSpacing);
                    Complex expArgZY = expArgZ + I*yf*avgMomentum.Y - (yf - packetCenter.Y)*(yf - packetCenter.Y)/(2*sigmaYSq);

                    float[] wfDataZY = wf.Data[z][y];
                    for (int x = 0; x < sx; x++)
                    {
                        float xf = (originAtLatticeCenter) ? (x - halfGridSizeX)*latticeSpacing : (x * latticeSpacing);

                        Complex expArgZYX = expArgZY + I*xf*avgMomentum.X - (xf - packetCenter.X)*(xf - packetCenter.X)/(2*sigmaXSq);
                        Complex wfVal = norm * Complex.Exp(expArgZYX);

                        wfDataZY[2*x]   = wfVal.Re;
                        wfDataZY[2*x+1] = wfVal.Im;
                    }
                }
            }, multiThread );

            wf.Normalize();
            return wf;
        }


    }
}
