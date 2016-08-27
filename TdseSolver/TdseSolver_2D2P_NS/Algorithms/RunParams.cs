using System;
using System.Drawing;
using System.Text;
using Vec2 = TdseUtils.Vec2;


namespace TdseSolver_2D2P_NS
{
    /// <summary>
    /// This class just encapsulates the parameters used for a single run.
    /// </summary>
    class RunParams
    {
        public Size     GridSize                      = new Size(64,64);

        public float    LatticeSpacing                = 1.0f;

        public float    Mass1                         = 1.0f;

        public float    Mass2                         = 1.0f;

        public Vec2     InitialWavePacketSize         = new Vec2(1,1);

        public Vec2     InitialWavePacketCenter1      = new Vec2(0,0);

        public Vec2     AtomCenter                    = new Vec2(0,0);

        public Vec2     InitialWavePacketMomentum1    = new Vec2(0,0);

        public int      Atom_N                        = 0;

        public int      Atom_Lz                       = 0;

        public float    AtomSize                      = 1.0f;

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
            sb.Append("GridSizeX: "                    + GridSize.Width.ToString()               + nl);
            sb.Append("GridSizeY: "                    + GridSize.Height.ToString()              + nl);
            sb.Append("LatticeSpacing: "               + LatticeSpacing.ToString()               + nl);
            sb.Append("Mass1: "                        + Mass1.ToString()                        + nl);
            sb.Append("Mass2: "                        + Mass2.ToString()                        + nl);
            sb.Append("InitialWavePacketSizeX: "       + InitialWavePacketSize.X.ToString()      + nl);
            sb.Append("InitialWavePacketSizeY: "       + InitialWavePacketSize.Y.ToString()      + nl);
            sb.Append("InitialWavePacketCenter1x: "    + InitialWavePacketCenter1.X.ToString()   + nl);
            sb.Append("InitialWavePacketCenter1y: "    + InitialWavePacketCenter1.Y.ToString()   + nl);
            sb.Append("AtomCenterX: "                  + AtomCenter.X.ToString()                 + nl);
            sb.Append("AtomCenterY: "                  + AtomCenter.Y.ToString()                 + nl);
            sb.Append("InitialWavePacketMomentum1x: "  + InitialWavePacketMomentum1.X.ToString() + nl);
            sb.Append("InitialWavePacketMomentum1y: "  + InitialWavePacketMomentum1.Y.ToString() + nl);
            sb.Append("Atom_N: "                       + Atom_N.ToString()                       + nl);
            sb.Append("Atom_Lz: "                      + Atom_Lz.ToString()                      + nl);
            sb.Append("AtomSize: "                     + AtomSize.ToString()                     + nl);
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
                    result.GridSize.Width = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("GridSizeY"))
                {
                    result.GridSize.Height = int.Parse(line.Split(':')[1]);
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
                else if (line.Contains("InitialWavePacketCenter1x"))
                {
                    result.InitialWavePacketCenter1.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketCenter1y"))
                {
                    result.InitialWavePacketCenter1.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("AtomCenterX"))
                {
                    result.AtomCenter.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("AtomCenterY"))
                {
                    result.AtomCenter.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum1x"))
                {
                    result.InitialWavePacketMomentum1.X = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("InitialWavePacketMomentum1y"))
                {
                    result.InitialWavePacketMomentum1.Y = Single.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("Atom_N"))
                {
                    result.Atom_N = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("Atom_Lz"))
                {
                    result.Atom_Lz = int.Parse(line.Split(':')[1]);
                }
                else if (line.Contains("AtomSize"))
                {
                    result.AtomSize = Single.Parse(line.Split(':')[1]);
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
