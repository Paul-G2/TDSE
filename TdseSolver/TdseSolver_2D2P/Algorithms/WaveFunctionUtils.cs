﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using Complex = TdseUtils.Complex;
using Vec2 = TdseUtils.Vec2;


namespace TdseSolver_2D2P
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
            int gridSizeX, int gridSizeY, float latticeSpacing, bool originAtLatticeCenter, float mass,
            Vec2 packetCenter, Vec2 packetWidth, Vec2 avgMomentum, bool multiThread=true)
        {
            WaveFunction wf = new WaveFunction(gridSizeX, gridSizeY, latticeSpacing);

            Complex I = Complex.I;
            float rootPi = (float) Math.Sqrt( Math.PI );
            float sigmaXSq = packetWidth.X * packetWidth.X;
            float sigmaYSq = packetWidth.Y * packetWidth.Y;
            float norm = (float) Math.Sqrt( (packetWidth.X/(rootPi*sigmaXSq)) * (packetWidth.Y/(rootPi*sigmaYSq)) );
            int halfGridSizeX = (gridSizeX - 1)/2;
            int halfGridSizeY = (gridSizeY - 1)/2;

            TdseUtils.Misc.ForLoop(0, gridSizeY, (y) =>
            {                
                float yf = (originAtLatticeCenter) ? (y - halfGridSizeY)*latticeSpacing : (y * latticeSpacing);
                Complex expArgY = I*yf*avgMomentum.Y - (yf - packetCenter.Y)*(yf - packetCenter.Y)/(2*sigmaYSq);
                    
                float[] wfDataY = wf.Data[y];
                for (int x = 0; x < gridSizeX; x++)
                {
                    float xf = (originAtLatticeCenter) ? (x - halfGridSizeX)*latticeSpacing : (x * latticeSpacing);

                    Complex expArgYX = expArgY + I*xf*avgMomentum.X - (xf - packetCenter.X)*(xf - packetCenter.X)/(2*sigmaXSq);
                    Complex wfVal = norm * Complex.Exp(expArgYX);
 
                    wfDataY[2*x]   = wfVal.Re;
                    wfDataY[2*x+1] = wfVal.Im;
                }             
            }, multiThread );

            wf.Normalize();
            return wf;
        }



        /// <summary>
        /// Calculates the value of a (freely evolving) Gaussian wavepacket at a given location and time. 
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

    }
}
