using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 2 dimensions, defined on a rectangular grid.
    /// </summary>
    partial class WaveFunction
    {
        public enum WfSaveFormat
        {
            AMPLITUDE_ONLY,
            AMPLITUDE_AND_PHASE,
            REAL_AND_IMAG,
            AMPLITUDE_AND_COLOR,
            AMPLITUDE_PHASE_AND_COLOR
        };


        // Declare a delegate for computing the color
        public delegate Color ColorDelegate(float re, float im, float maxAmpl);




        /// <summary>
        /// Writes the wavefunction to a file, in VTK format.
        /// </summary>
        public void SaveToVtkFile(string fileSpec, WfSaveFormat format=WfSaveFormat.REAL_AND_IMAG, ColorDelegate colorFunc = null)
        {
            using (FileStream fileStream = File.Create(fileSpec))
            {
                using (BinaryWriter bw = new BinaryWriter(fileStream) )
                {
                    string nl = Environment.NewLine;
                    bw.Write( Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl) );
                    bw.Write( Encoding.ASCII.GetBytes( "Wavefunction " + format.ToString() + " " + "spacing: " + m_latticeSpacing.ToString() + " " + DateTime.Now.ToString() + nl) );
                    bw.Write( Encoding.ASCII.GetBytes("BINARY" + nl) );
                    bw.Write( Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl) );
                    bw.Write( Encoding.ASCII.GetBytes("DIMENSIONS " + GridSizeX + " " + GridSizeY + " 1" + nl) );
                    bw.Write( Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl) );
                    bw.Write( Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl) );
                    bw.Write( Encoding.ASCII.GetBytes("POINT_DATA " +  GridSizeX*GridSizeY + nl) );


                    if ( format == WfSaveFormat.REAL_AND_IMAG )
                    {
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS real_part float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                byte[] bytes = BitConverter.GetBytes(m_realPart[x][y]);
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }

                        bw.Write(Encoding.ASCII.GetBytes("SCALARS imag_part float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                byte[] bytes = BitConverter.GetBytes(m_imagPart[x][y]);
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }
                    }


                    else if ( format == WfSaveFormat.AMPLITUDE_ONLY )
                    {
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                byte[] bytes = BitConverter.GetBytes( Ampl(x,y) );
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }
                    }

                    else if ( format == WfSaveFormat.AMPLITUDE_AND_PHASE )
                    {
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                byte[] bytes = BitConverter.GetBytes( Ampl(x,y) );
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }

                        bw.Write(Encoding.ASCII.GetBytes("SCALARS phase float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                byte[] bytes = BitConverter.GetBytes( Phase(x,y) );
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }
                    }

                    else if ( format == WfSaveFormat.AMPLITUDE_AND_COLOR )
                    {
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        float maxAmpl = 0.0f;
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                float ampl = Ampl(x,y);
                                if (ampl > maxAmpl) { maxAmpl = ampl; }

                                byte[] bytes = BitConverter.GetBytes( ampl );
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }

                        // Color part
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS colors unsigned_char 3" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                Color color = (colorFunc == null) ? Color.Blue : colorFunc(m_realPart[x][y], m_imagPart[x][y], maxAmpl);

                                bw.Write( color.R );
                                bw.Write( color.G );
                                bw.Write( color.B );
                            }
                        }
                    }

                    else if ( format == WfSaveFormat.AMPLITUDE_PHASE_AND_COLOR )
                    {
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        float maxAmpl = 0.0f;
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                float ampl = Ampl(x,y);
                                if (ampl > maxAmpl) { maxAmpl = ampl; }

                                byte[] bytes = BitConverter.GetBytes( ampl );
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }

                        bw.Write(Encoding.ASCII.GetBytes("SCALARS phase float" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                byte[] bytes = BitConverter.GetBytes( Phase(x,y) );
                                Array.Reverse(bytes); // VTK wants big-endian data
                                bw.Write(bytes);
                            }
                        }

                        // Color part
                        bw.Write(Encoding.ASCII.GetBytes("SCALARS colors unsigned_char 3" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                        for (int y = 0; y < GridSizeY; y++)
                        {
                            for (int x = 0; x < GridSizeX; x++)
                            {
                                Color color = (colorFunc == null) ? Color.Blue : colorFunc(m_realPart[x][y], m_imagPart[x][y], maxAmpl);

                                bw.Write( color.R );
                                bw.Write( color.G );
                                bw.Write( color.B );
                            }
                        }
                    }

                    else
                    {
                        throw new ArgumentException("Unsupported format in Wavefunction.SaveToVtkFile");
                    }

                }
            }
        }



        /// <summary>
        /// Reads a wavefunction from a VTK file.
        /// </summary>
        public static WaveFunction ReadFromVtkFile(string fileSpec)
        {
            int gridSizeX = -1;
            int gridSizeY = -1;
            float[][] realPart = null;
            float[][] imagPart = null;
            string format = "";
            float latticeSpacing = 0.0f;

            using ( BinaryReader br = new BinaryReader(File.Open(fileSpec, FileMode.Open)) )
            {
                // Parse the header
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    string textLine = ReadTextLine(br);

                    if ( textLine.StartsWith("Wavefunction") ) 
                    {
                        string[] comps = textLine.Split(null);
                        format = comps[1];
                        latticeSpacing = Single.Parse(comps[3]);
                    }
                    else if (textLine.StartsWith("DIMENSIONS"))
                    {
                        string[] comps = textLine.Split(null);
                        gridSizeX = Int32.Parse(comps[1]);
                        gridSizeY = Int32.Parse(comps[2]);
                    }
                    else if (textLine.StartsWith("LOOKUP_TABLE default"))
                    {
                        break;
                    }
                }

                // Bail out if the header was not what we expected
                if ( string.IsNullOrEmpty(format) || (gridSizeX < 0) || (gridSizeY < 0) )
                {
                    throw new ArgumentException("Invalid Wavefunction file, in Wavefunction.ReadFromVtkFile.");
                }


                // Allocate arrays
                realPart = new float[gridSizeX][];
                imagPart = new float[gridSizeX][];
                for (int i = 0; i < gridSizeX; i++) 
                { 
                    realPart[i] = new float[gridSizeY]; 
                    imagPart[i] = new float[gridSizeY];
                }

                if (format == "REAL_AND_IMAG")
                {
                    // Read the real parts
                    for (int y = 0; y < gridSizeY; y++)
                    {
                        for (int x = 0; x < gridSizeX; x++)
                        {
                            byte[] bytes = br.ReadBytes(4);
                            Array.Reverse(bytes); // Convert from big-endian
                            realPart[x][y] = BitConverter.ToSingle(bytes, 0);
                        }
                    }

                    // Read the imaginary parts
                    ReadTextLine(br);
                    ReadTextLine(br);
                    for (int y = 0; y < gridSizeY; y++)
                    {
                        for (int x = 0; x < gridSizeX; x++)
                        {
                            byte[] bytes = br.ReadBytes(4);
                            Array.Reverse(bytes); // Convert from big-endian
                            imagPart[x][y] = BitConverter.ToSingle(bytes, 0);
                        }
                    }
                }

                else if (format == "AMPLITUDE_AND_PHASE" || format == "AMPLITUDE_PHASE_AND_COLOR")
                {
                    float[][] ampl  = new float[gridSizeX][];
                    float[][] phase = new float[gridSizeX][];
                    for (int i = 0; i < gridSizeX; i++) 
                    { 
                        ampl[i]  = new float[gridSizeY]; 
                        phase[i] = new float[gridSizeY];
                    }

                    // Read the amplitudes
                    for (int y = 0; y < gridSizeY; y++)
                    {
                        for (int x = 0; x < gridSizeX; x++)
                        {
                            byte[] bytes = br.ReadBytes(4);
                            Array.Reverse(bytes); // Convert from big-endian
                            ampl[x][y] = BitConverter.ToSingle(bytes, 0);
                        }
                    }

                    // Read the phases
                    ReadTextLine(br);
                    ReadTextLine(br);
                    for (int y = 0; y < gridSizeY; y++)
                    {
                        for (int x = 0; x < gridSizeX; x++)
                        {
                            byte[] bytes = br.ReadBytes(4);
                            Array.Reverse(bytes); // Convert from big-endian
                            phase[x][y] = BitConverter.ToSingle(bytes, 0);
                        }
                    }

                    // Convert to real and imaginary parts
                    for (int y = 0; y < gridSizeY; y++)
                    {
                        for (int x = 0; x < gridSizeX; x++)
                        {
                            float aVal = ampl[x][y];
                            float pVal = phase[x][y];
                            realPart[x][y] = (float) ( aVal*Math.Cos(pVal) );
                            imagPart[x][y] = (float) ( aVal*Math.Sin(pVal) );
                        }
                    }
                }

                else
                {
                    throw new ArgumentException("Unsupported format " + format + ", in Wavefunction.ReadFromVtkFile.");
                }

            }
            return new WaveFunction(realPart, imagPart, latticeSpacing);
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
