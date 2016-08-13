using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;


namespace TdseSolver_2D2P
{
    /// <summary>
    /// This class represents a 1-particle, 2-dimensional probability density.
    /// </summary>
    partial class ProbabilityDensity
    {
        /// <summary>
        /// Writes the densities to a file, in VTK format.
        /// </summary>
        public void SaveToVtkFile(string fileSpec)
        {
            SaveToVtkFile( new ProbabilityDensity[]{this}, fileSpec );
        }


        /// <summary>
        /// Writes a set of densities to a file, in VTK format.
        /// </summary>
        public static void SaveToVtkFile(ProbabilityDensity[] probs, string fileSpec)
        {
            unsafe
            {
                using (FileStream fileStream = File.Create(fileSpec))
                {
                    using (BinaryWriter bw = new BinaryWriter(fileStream))
                    {
                        int sx = probs[0].GridSizeX;
                        int sy = probs[0].GridSizeY;
                        string nl = Environment.NewLine;

                        // For performance, we keep a temporary float value with pointers to its bytes
                        float floatVal = 0.0f;
                        byte* floatBytes0 = (byte*)(&floatVal);
                        byte* floatBytes1 = floatBytes0 + 1;
                        byte* floatBytes2 = floatBytes0 + 2;
                        byte* floatBytes3 = floatBytes0 + 3;

                        // Write the header
                        bw.Write(Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("Probability2D2P " + "spacing: " + probs[0].LatticeSpacing.ToString() + nl));
                        bw.Write(Encoding.ASCII.GetBytes("BINARY" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + sx + " " + sy + " 1" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sx*sy + nl));

                        for (int i=0; i<probs.Length; i++)
                        {
                            probs[i].ToVtkStream( bw, "prob_" + i.ToString() );
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Writes the density values to a stream, in vtk format.
        /// </summary>
        private void ToVtkStream(BinaryWriter bw, string name)
        {
            int sx = GridSizeX;
            int sy = GridSizeY;
            string nl = Environment.NewLine;

            unsafe
            {
                // For performance, we keep a temporary float value with pointers to its bytes
                float floatVal = 0.0f;
                byte* floatBytes0 = (byte*)(&floatVal);
                byte* floatBytes1 = floatBytes0 + 1;
                byte* floatBytes2 = floatBytes0 + 2;
                byte* floatBytes3 = floatBytes0 + 3;

                bw.Write(Encoding.ASCII.GetBytes("SCALARS " + name + " float" + nl));
                bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                byte[] bytePlane = new byte[sx*sy*4];
                int n = 0;
                for (int y = 0; y < sy; y++)
                {
                    float[] dataY = m_data[y];

                    for (int x = 0; x < sx; x++)
                    {
                        floatVal = dataY[x];

                        bytePlane[n]   = *floatBytes3; // Reverse the bytes, since VTK wants big-endian data
                        bytePlane[n+1] = *floatBytes2;
                        bytePlane[n+2] = *floatBytes1;
                        bytePlane[n+3] = *floatBytes0;
                        n += 4;
                    }
                }
                bw.Write(bytePlane);
            }
        }

        /// <summary>
        /// Reads probabilty densities from a vtk file.
        /// </summary>
        public static ProbabilityDensity[] ReadFromVtkFile(string fileSpec)
        {

            int sx = -1;
            int sy = -1;
            float latticeSpacing = 0.0f;
            List<ProbabilityDensity> pdList = new List<ProbabilityDensity>();

            using ( BinaryReader br = new BinaryReader(File.Open(fileSpec, FileMode.Open)) )
            {
                // Parse the header
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    string textLine = ReadTextLine(br);

                    if ( textLine.StartsWith("Probability2D2P") ) 
                    {
                        string[] comps = textLine.Split(null);
                        latticeSpacing = Single.Parse(comps[2]);
                    }
                    else if (textLine.StartsWith("DIMENSIONS"))
                    {
                        string[] comps = textLine.Split(null);
                        sx = Int32.Parse(comps[1]);
                        sy = Int32.Parse(comps[2]);
                    }
                    else if (textLine.StartsWith("POINT_DATA"))
                    {
                        break;
                    }
                }

                // Bail out if the header was not what we expected
                if ( (sx < 0) || (sy < 0) )
                {
                    throw new ArgumentException("Invalid Probability Density file, in ProabilityDensity.ReadFromVtkFile.");
                }

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    pdList.Add( FromVtkStream(br, sx, sy, latticeSpacing) );
                }
                
            }

            return pdList.ToArray();
        }



        /// <summary>
        /// Reads density values from a vtk stream.
        /// </summary>
        private static ProbabilityDensity FromVtkStream(BinaryReader br, int sizeX, int sizeY, float latticeSpacing)
        {
            ProbabilityDensity result = new ProbabilityDensity(sizeX, sizeY, latticeSpacing);

            unsafe
            {
                // For performance, we keep a temporary float value with pointers to its bytes
                float floatVal = 0.0f;
                byte* floatBytes0 = (byte*)(&floatVal);
                byte* floatBytes1 = floatBytes0 + 1;
                byte* floatBytes2 = floatBytes0 + 2;
                byte* floatBytes3 = floatBytes0 + 3;

                // Read the density values
                ReadTextLine(br);
                ReadTextLine(br);
                byte[] bytePlane = br.ReadBytes(sizeX*sizeY*4);
                        
                int n = 0;
                for (int y = 0; y < sizeY; y++)
                {
                    float[] dataY = result.Data[y];
                    for (int x = 0; x < sizeX; x++)
                    {
                        if ((n<0) || (n>=bytePlane.Length-3)) 
                        {
                            n = -1;
                        }
                        *floatBytes3 = bytePlane[n];
                        *floatBytes2 = bytePlane[n+1];
                        *floatBytes1 = bytePlane[n+2];
                        *floatBytes0 = bytePlane[n+3];
                        dataY[x] = floatVal;
                        n += 4;
                    }
                }          
            }

            return result;
        }
        
        
        /// <summary>
        /// Reads a text line from a binary file.
        /// </summary>
        private static string ReadTextLine(BinaryReader br)
        {
            List<byte> bytes = new List<byte>();
            while ( !EndsWithNewLine(bytes) )
            {
                bytes.Add(br.ReadByte());
            }

            string line = Encoding.ASCII.GetString( bytes.ToArray() );
            return line;
        }


        /// <summary>
        /// Tests whether a given byte array ends with a newline string.
        /// </summary>
        private static bool EndsWithNewLine(List<byte> bytes)
        {
            byte[] nlBytes = Encoding.ASCII.GetBytes(Environment.NewLine);

            if (bytes.Count < nlBytes.Length) {  return false; }

            bool result = true;
            for (int i=0; i<nlBytes.Length; i++)
            {
                if (bytes[bytes.Count-1-i] != nlBytes[nlBytes.Length-1-i])
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
    }
}
