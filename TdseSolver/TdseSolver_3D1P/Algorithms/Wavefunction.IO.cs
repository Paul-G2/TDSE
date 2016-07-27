using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class represents a 1-particle wavefunction in 3 dimensions, defined on a cubic grid.
    /// </summary>
    partial class WaveFunction
    {
        public enum WfSaveFormat
        {
            REAL_AND_IMAG,
            AMPLITUDE_ONLY,
            AMPLITUDE_AND_COLOR,
        };


        // Declare a delegate for computing the color
        public delegate Color ColorDelegate(float re, float im, float maxAmpl);




        /// <summary>
        /// Writes the wavefunction to a file, in VTK format.
        /// </summary>
        public void SaveToVtkFile(string fileSpec, WfSaveFormat format, ColorDelegate colorFunc = null)
        {
            unsafe
            {
                using (FileStream fileStream = File.Create(fileSpec))
                {
                    using (BinaryWriter bw = new BinaryWriter(fileStream))
                    {
                        int sx = GridSpec.SizeX;
                        int sy = GridSpec.SizeY;
                        int sz = GridSpec.SizeZ;
                        int sx2 = 2*sx;
                        string nl = Environment.NewLine;

                        // For performance, we keep a temporary float value with pointers to its bytes
                        float floatVal = 0.0f;
                        byte* floatBytes0 = (byte*)(&floatVal);
                        byte* floatBytes1 = floatBytes0 + 1;
                        byte* floatBytes2 = floatBytes0 + 2;
                        byte* floatBytes3 = floatBytes0 + 3;

                        // Write the header
                        bw.Write(Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("Wavefunction3D " + format.ToString() + " " + "spacing: " + m_latticeSpacing.ToString() + nl));
                        bw.Write(Encoding.ASCII.GetBytes("BINARY" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + GridSpec.SizeX + " " + GridSpec.SizeY + " " + GridSpec.SizeZ + nl));
                        bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                        bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sx*sy*sz + nl));


                        if (format == WfSaveFormat.REAL_AND_IMAG)
                        {
                            // Write out the real and imaginary parts
                            bw.Write(Encoding.ASCII.GetBytes("SCALARS wf float 2" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                            byte[] bytePlane = new byte[sx*sy*8];
                            for (int z = 0; z < sz; z++)
                            {
                                int n = 0;                                
                                for (int y = 0; y < sy; y++)
                                {
                                    float[] dataZY = m_data[z][y];

                                    for (int nx = 0; nx < sx2; nx++)
                                    {
                                        floatVal = dataZY[nx];

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


                        else if ((format == WfSaveFormat.AMPLITUDE_ONLY) || (format == WfSaveFormat.AMPLITUDE_AND_COLOR))
                        {
                            // Write out the amplitudes
                            bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                            byte[] bytePlane = new byte[sx*sy*4];
                            float maxAmpl = 0.0f;

                            for (int z = 0; z < sz; z++)
                            {
                                int n = 0;
                                for (int y = 0; y < sy; y++)
                                {
                                    float[] dataZY = m_data[z][y];

                                    for (int nx = 0; nx < sx2; nx+=2)
                                    {
                                        float re = dataZY[nx];
                                        float im = dataZY[nx+1];
                                        floatVal = (float)Math.Sqrt(re*re + im*im);
                                        if (floatVal > maxAmpl) { maxAmpl = floatVal; }

                                        bytePlane[n]   = *floatBytes3;
                                        bytePlane[n+1] = *floatBytes2;
                                        bytePlane[n+2] = *floatBytes1;
                                        bytePlane[n+3] = *floatBytes0;
                                        n += 4;
                                    }
                                }
                                bw.Write(bytePlane);
                            }

                            // Maybe write out the colors
                            if (format == WfSaveFormat.AMPLITUDE_AND_COLOR)
                            {
                                bw.Write(Encoding.ASCII.GetBytes("SCALARS colors unsigned_char 3" + nl));
                                bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                                byte[] colorPlane = new byte[sx*sy*3];

                                for (int z = 0; z < sz; z++)
                                {
                                    int n = 0;
                                    for (int y = 0; y < sy; y++)
                                    {
                                        float[] dataZY = m_data[z][y];

                                        for (int nx = 0; nx < sx2; nx+=2)
                                        {
                                            Color color = (colorFunc == null) ? Color.Blue : colorFunc(dataZY[nx], dataZY[nx+1], maxAmpl);

                                            colorPlane[n]   = color.R;
                                            colorPlane[n+1] = color.G;
                                            colorPlane[n+2] = color.B;
                                            n += 3;
                                        }
                                    }
                                    bw.Write(colorPlane);
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
        }



        /// <summary>
        /// Reads a wavefunction from a VTK file.
        /// </summary>
        public static WaveFunction ReadFromVtkFile(string fileSpec)
        {
            int sx = -1;
            int sy = -1;
            int sz = -1;
            float[][][] wfData = null;
            string format = "";
            float latticeSpacing = 0.0f;

            using ( BinaryReader br = new BinaryReader(File.Open(fileSpec, FileMode.Open)) )
            {
                // Parse the header
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    string textLine = ReadTextLine(br);

                    if ( textLine.StartsWith("Wavefunction3D") ) 
                    {
                        string[] comps = textLine.Split(null);
                        format = comps[1];
                        latticeSpacing = Single.Parse(comps[3]);
                    }
                    else if (textLine.StartsWith("DIMENSIONS"))
                    {
                        string[] comps = textLine.Split(null);
                        sx = Int32.Parse(comps[1]);
                        sy = Int32.Parse(comps[2]);
                        sz = Int32.Parse(comps[3]);
                    }
                    else if (textLine.StartsWith("LOOKUP_TABLE default"))
                    {
                        break;
                    }
                }

                // Bail out if the header was not what we expected
                if ( string.IsNullOrEmpty(format) || (sx < 0) || (sy < 0) || (sz < 0) )
                {
                    throw new ArgumentException("Invalid Wavefunction file, in Wavefunction.ReadFromVtkFile.");
                }
                if (format != "REAL_AND_IMAG")
                {
                    throw new ArgumentException("Unsupported Wavefunction format, in Wavefunction.ReadFromVtkFile. " + "(" + format + ")");
                }

                // Allocate the wf data array
                wfData = TdseUtils.Misc.Allocate3DArray(sz, sy, 2*sx);

                unsafe
                {
                    float floatVal = 0.0f;
                    byte* floatBytes0 = (byte*)(&floatVal);
                    byte* floatBytes1 = floatBytes0 + 1;
                    byte* floatBytes2 = floatBytes0 + 2;
                    byte* floatBytes3 = floatBytes0 + 3;
                    int sx2 = 2*sx;

                    // Read the real and imaginary parts
                    for (int z = 0; z < sz; z++)
                    {
                        byte[] bytePlane = br.ReadBytes(sx*sy*8);
                        
                        int n = 0;
                        for (int y = 0; y < sy; y++)
                        {
                            float[] dataZY = wfData[z][y];
                            for (int nx = 0; nx < sx2; nx++)
                            {
                                *floatBytes3 = bytePlane[n];
                                *floatBytes2 = bytePlane[n+1];
                                *floatBytes1 = bytePlane[n+2];
                                *floatBytes0 = bytePlane[n+3];
                                dataZY[nx] = floatVal;
                                n += 4;
                            }
                        }
                    }                    
                }

            }

            return new WaveFunction(wfData, latticeSpacing);
        }


        /// <summary>
        /// Reads a text line from a binary file.
        /// </summary>
        public static string ReadTextLine(BinaryReader br)
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
        public static bool EndsWithNewLine(List<byte> bytes)
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
