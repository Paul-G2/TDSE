using System;
using System.Drawing;
using System.Text;
using Vec3 = TdseUtils.Vec3;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class just encapsulates the parameters used for a single wavefunction evolution.
    /// </summary>
    class RunParams
    {
        public GridSpec GridSpec                      = new GridSpec(64, 64, 64);

        public float    LatticeSpacing                = 1.0f;

        public float    ParticleMass                  = 1.0f;

        public Vec3     InitialWavePacketSize         = new Vec3(1,1,1);

        public Vec3     InitialWavePacketCenter       = new Vec3(0,0,0);

        public Vec3     InitialWavePacketMomentum     = new Vec3(0,0,0);

        public int      DampingBorderWidth            = 0;

        public float    DampingFactor                 = 0.0f;

        public float    TimeStep                      = 0.1f;

        public float    TotalTime                     = 1.0f;

        public int      NumFramesToSave               = 2;

        public bool     MultiThread                   = true;

        public WaveFunction.WfSaveFormat SaveFormat   = WaveFunction.WfSaveFormat.REAL_AND_IMAG;

        public string  VCode                          = "";




        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string nl = Environment.NewLine;
            sb.Append("GridSizeX: "                  + GridSpec.SizeX.ToString()              + nl);
            sb.Append("GridSizeY: "                  + GridSpec.SizeY.ToString()              + nl);
            sb.Append("GridSizeZ: "                  + GridSpec.SizeZ.ToString()              + nl);
            sb.Append("LatticeSpacing: "             + LatticeSpacing.ToString()              + nl);
            sb.Append("ParticleMass: "               + ParticleMass.ToString()                + nl);
            sb.Append("InitialWavePacketSizeX: "     + InitialWavePacketSize.X.ToString()     + nl);
            sb.Append("InitialWavePacketSizeY: "     + InitialWavePacketSize.Y.ToString()     + nl);
            sb.Append("InitialWavePacketSizeZ: "     + InitialWavePacketSize.Z.ToString()     + nl);
            sb.Append("InitialWavePacketCenterX: "   + InitialWavePacketCenter.X.ToString()   + nl);
            sb.Append("InitialWavePacketCenterY: "   + InitialWavePacketCenter.Y.ToString()   + nl);
            sb.Append("InitialWavePacketCenterZ: "   + InitialWavePacketCenter.Z.ToString()   + nl);
            sb.Append("InitialWavePacketMomentumX: " + InitialWavePacketMomentum.X.ToString() + nl);
            sb.Append("InitialWavePacketMomentumY: " + InitialWavePacketMomentum.Y.ToString() + nl);
            sb.Append("InitialWavePacketMomentumZ: " + InitialWavePacketMomentum.Z.ToString() + nl);
            sb.Append("DampingBorderWidth: "         + DampingBorderWidth.ToString()          + nl);
            sb.Append("DampingFactor: "              + DampingFactor.ToString()               + nl);
            sb.Append("TimeStep: "                   + TimeStep.ToString()                    + nl);
            sb.Append("TotalTime: "                  + TotalTime.ToString()                   + nl);
            sb.Append("NumFramesToSave: "            + NumFramesToSave.ToString()             + nl);
            sb.Append("MultiThread: "                + MultiThread.ToString()                 + nl);
            sb.Append("SaveFormat: "                 + SaveFormat.ToString()                  + nl);
            sb.Append("\n\n\n\n"                     + VCode                                  + nl);

            return sb.ToString();
        }


        /// <summary>
        /// Creates a RunParams instance from its string representation.
        /// </summary>
        public static RunParams FromString(string stringRep)
        {
            RunParams result = new RunParams();

            string[] lines = stringRep.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            int numLines = lines.Length;
            for ( int lineIndex = 0; lineIndex < numLines; lineIndex++ )
            {
                string line = lines[lineIndex];

                if (line.Contains("GridSizeX"))
                {
                    result.GridSpec.SizeX = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("GridSizeY"))
                {
                    result.GridSpec.SizeY = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("GridSizeZ"))
                {
                    result.GridSpec.SizeZ = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("LatticeSpacing"))
                {
                    result.LatticeSpacing = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("ParticleMass"))
                {
                    result.ParticleMass = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketSizeX"))
                {
                    result.InitialWavePacketSize.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketSizeY"))
                {
                    result.InitialWavePacketSize.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketSizeZ"))
                {
                    result.InitialWavePacketSize.Z = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenterX"))
                {
                    result.InitialWavePacketCenter.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenterY"))
                {
                    result.InitialWavePacketCenter.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenterZ"))
                {
                    result.InitialWavePacketCenter.Z = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentumX"))
                {
                    result.InitialWavePacketMomentum.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentumY"))
                {
                    result.InitialWavePacketMomentum.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentumZ"))
                {
                    result.InitialWavePacketMomentum.Z = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("DampingBorderWidth"))
                {
                    result.DampingBorderWidth = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("DampingFactor"))
                {
                    result.DampingFactor = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("TimeStep"))
                {
                    result.TimeStep = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("TotalTime"))
                {
                    result.TotalTime = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("NumFramesToSave"))
                {
                    result.NumFramesToSave = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("MultiThread"))
                {
                    result.MultiThread = bool.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("SaveFormat"))
                {
                    result.SaveFormat = (WaveFunction.WfSaveFormat) Enum.Parse( typeof(WaveFunction.WfSaveFormat), line.Split(':')[1], true);
                }
                else if ( !string.IsNullOrEmpty(line) )
                {
                    for (; lineIndex<numLines; lineIndex++)
                    {
                        result.VCode += lines[lineIndex] + "\n";
                    }
                }
            }

            return result;
        }
    }
}
