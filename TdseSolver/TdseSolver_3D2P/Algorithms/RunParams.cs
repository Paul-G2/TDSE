using System;
using System.Drawing;
using System.Text;
using Vec3 = TdseUtils.Vec3;


namespace TdseSolver_3D2P
{
    /// <summary>
    /// This class just encapsulates the parameters used for a single wavefunction evolution.
    /// </summary>
    class RunParams
    {
        public GridSpec GridSpec                      = new GridSpec(64, 64, 64);

        public float    LatticeSpacing                = 1.0f;

        public float    Mass1                         = 1.0f;

        public float    Mass2                         = 1.0f;

        public Vec3     InitialWavePacketSize         = new Vec3(1,1,1);

        public Vec3     InitialWavePacketCenter1      = new Vec3(0,0,0);

        public Vec3     InitialWavePacketCenter2      = new Vec3(0,0,0);

        public Vec3     InitialWavePacketMomentum1    = new Vec3(0,0,0);

        public Vec3     InitialWavePacketMomentum2    = new Vec3(0,0,0);

        public int      DampingBorderWidth            = 0;

        public float    DampingFactor                 = 0.0f;

        public float    TimeStep                      = 0.1f;

        public float    TotalTime                     = 1.0f;

        public int      NumFramesToSave               = 2;

        public bool     MultiThread                   = true;

        public string   VCode                         = "";




        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string nl = Environment.NewLine;
            sb.Append("GridSizeX: "                    + GridSpec.SizeX.ToString()               + nl);
            sb.Append("GridSizeY: "                    + GridSpec.SizeY.ToString()               + nl);
            sb.Append("GridSizeZ: "                    + GridSpec.SizeZ.ToString()               + nl);
            sb.Append("LatticeSpacing: "               + LatticeSpacing.ToString()               + nl);
            sb.Append("Mass1: "                        + Mass1.ToString()                        + nl);
            sb.Append("Mass2: "                        + Mass2.ToString()                        + nl);
            sb.Append("InitialWavePacketSizeX: "       + InitialWavePacketSize.X.ToString()      + nl);
            sb.Append("InitialWavePacketSizeY: "       + InitialWavePacketSize.Y.ToString()      + nl);
            sb.Append("InitialWavePacketSizeZ: "       + InitialWavePacketSize.Z.ToString()      + nl);
            sb.Append("InitialWavePacketCenter1x: "    + InitialWavePacketCenter1.X.ToString()   + nl);
            sb.Append("InitialWavePacketCenter1y: "    + InitialWavePacketCenter1.Y.ToString()   + nl);
            sb.Append("InitialWavePacketCenter1z: "    + InitialWavePacketCenter1.Z.ToString()   + nl);
            sb.Append("InitialWavePacketCenter2x: "    + InitialWavePacketCenter2.X.ToString()   + nl);
            sb.Append("InitialWavePacketCenter2y: "    + InitialWavePacketCenter2.Y.ToString()   + nl);
            sb.Append("InitialWavePacketCenter2z: "    + InitialWavePacketCenter2.Z.ToString()   + nl);
            sb.Append("InitialWavePacketMomentum1x: "  + InitialWavePacketMomentum1.X.ToString() + nl);
            sb.Append("InitialWavePacketMomentum1y: "  + InitialWavePacketMomentum1.Y.ToString() + nl);
            sb.Append("InitialWavePacketMomentum1z: "  + InitialWavePacketMomentum1.Z.ToString() + nl);
            sb.Append("InitialWavePacketMomentum2x: "  + InitialWavePacketMomentum2.X.ToString() + nl);
            sb.Append("InitialWavePacketMomentum2y: "  + InitialWavePacketMomentum2.Y.ToString() + nl);
            sb.Append("InitialWavePacketMomentum2z: "  + InitialWavePacketMomentum2.Z.ToString() + nl);
            sb.Append("DampingBorderWidth: "           + DampingBorderWidth.ToString()           + nl);
            sb.Append("DampingFactor: "                + DampingFactor.ToString()                + nl);
            sb.Append("TimeStep: "                     + TimeStep.ToString()                     + nl);
            sb.Append("TotalTime: "                    + TotalTime.ToString()                    + nl);
            sb.Append("NumFramesToSave: "              + NumFramesToSave.ToString()              + nl);
            sb.Append("MultiThread: "                  + MultiThread.ToString()                  + nl);
            sb.Append("\n\n\n\n"                       + VCode                                   + nl);

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
                else if (line.Contains("Mass1"))
                {
                    result.Mass1 = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("Mass2"))
                {
                    result.Mass2 = Single.Parse(line.Split(':')[1]);
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
                else if (line.Contains("InitialWavePacketCenter1x"))
                {
                    result.InitialWavePacketCenter1.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenter1y"))
                {
                    result.InitialWavePacketCenter1.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenter1z"))
                {
                    result.InitialWavePacketCenter1.Z = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenter2x"))
                {
                    result.InitialWavePacketCenter2.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenter2y"))
                {
                    result.InitialWavePacketCenter2.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenter2z"))
                {
                    result.InitialWavePacketCenter2.Z = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum1x"))
                {
                    result.InitialWavePacketMomentum1.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum1y"))
                {
                    result.InitialWavePacketMomentum1.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum1z"))
                {
                    result.InitialWavePacketMomentum1.Z = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum2x"))
                {
                    result.InitialWavePacketMomentum2.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum2y"))
                {
                    result.InitialWavePacketMomentum2.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum2z"))
                {
                    result.InitialWavePacketMomentum2.Z = Single.Parse(line.Split(':')[1]);
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
                    // Do nothing (This is for backwards compatibility)
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
