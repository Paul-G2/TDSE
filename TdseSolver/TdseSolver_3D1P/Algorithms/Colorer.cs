using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class creates wavefunction vtk files with specified color schemes. 
    /// </summary>
    class Colorer : TdseUtils.Proc
    {
        // Class data
        string                 m_inputDir           = "";
        TdseUtils.ColorBuilder m_colorBuilder       = null;
        int                    m_numFilesToProcess  = 0;
        int                    m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Colorer(string inputDir, TdseUtils.ColorBuilder colorBuilder)
        {
            m_inputDir = inputDir;
            m_colorBuilder = colorBuilder;
        }

        /// <summary>
        /// Gets the current progress.
        /// </summary>
        public int Progress
        {
            get
            {
                return (m_numFilesToProcess == 0) ? 0 : (100 * (m_currentFileIndex + 1)) / m_numFilesToProcess;
            }
        }

        /// <summary>
        /// Worker method.
        /// </summary>
        protected override void WorkerMethod()
        {
            string[] vtkFiles = Directory.GetFiles(m_inputDir, "*.vtk");
            if ( (vtkFiles==null) || (vtkFiles.Length == 0) ) { return; }

            // Write the color parameters to a file
            string outputDir = CreateOutputDir(m_inputDir);
            File.WriteAllText(Path.Combine(outputDir, "ColorParams.txt"), m_colorBuilder.GetLastSavedCode().Replace("\n", "\r\n"));

            // Copy the RunParams file to the output dir
            string paramsFile = Path.Combine(m_inputDir, "Params.txt");
            if (File.Exists(paramsFile))
            {
                File.Copy(paramsFile, Path.Combine(outputDir, Path.GetFileName(paramsFile)));
            }

            WaveFunction.ColorDelegate colorFunc = (re, im, maxAmpl) => { return m_colorBuilder.CalcColor(re,im,maxAmpl); };
            if (colorFunc(1, 1, 1) == Color.Empty) { colorFunc = null; }

            int chunkSize = Environment.ProcessorCount;
            m_numFilesToProcess = vtkFiles.Length;
            for (int iStart = 0; iStart < m_numFilesToProcess; iStart += chunkSize)
            {
                int iEnd = Math.Min(iStart+chunkSize, m_numFilesToProcess);
                Parallel.For(iStart, iEnd, i=> 
                {
                    string inFile = vtkFiles[i];
                    string outFile = Path.Combine(outputDir, Path.GetFileName(inFile));
                    ProcessFile(inFile, outFile, colorFunc);
                });

                // Report progress to the caller
                m_currentFileIndex = iEnd - 1;
                ReportProgress();
                if (IsCancelled) { return; }
            }
        }



        /// <summary>
        /// Creates a directory to hold the results.
        /// </summary>
        static string CreateOutputDir(string baseDir)
        {
            int index = 0;
            string dir = Path.Combine( baseDir, "ReColored_" + index.ToString("D4") );
            while ( Directory.Exists(dir) )
            {
                index++;
                dir = Path.Combine( baseDir, "ReColored_" + index.ToString("D4") );
            }
            Directory.CreateDirectory(dir);

            return dir;
        }
        

        /// <summary>
        /// Processes a single input file.
        /// </summary>
        private void ProcessFile(string inFile, string outFile, WaveFunction.ColorDelegate colorFunc)
        {
            unsafe
            {
                int sx = -1;
                int sy = -1;
                int sz = -1;
                string inFormat = "";
                float latticeSpacing = 0.0f;
                long inDataStartPos = 0;
                string nl = Environment.NewLine;

                float fa = 0.0f, fb = 0.0f;
                byte* fa0 = (byte*)(&fa);
                byte* fa1 = fa0 + 1;
                byte* fa2 = fa0 + 2;
                byte* fa3 = fa0 + 3;
                byte* fb0 = (byte*)(&fb);
                byte* fb1 = fb0 + 1;
                byte* fb2 = fb0 + 2;
                byte* fb3 = fb0 + 3;

                
                using (BinaryReader br = new BinaryReader(File.Open(inFile, FileMode.Open)))
                {
                    // Parse the header of the input file
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        string textLine = WaveFunction.ReadTextLine(br);

                        if (textLine.StartsWith("Wavefunction3D"))
                        {
                            string[] comps = textLine.Split(null);
                            inFormat = comps[1];
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
                    if (string.IsNullOrEmpty(inFormat) || (sx < 0) || (sy < 0) || (sz < 0))
                    {
                        throw new ArgumentException("Invalid Wavefunction file, in Colorer.ProcessFile.");
                    }
                    if (inFormat != "REAL_AND_IMAG")
                    {
                        throw new ArgumentException("Unsupported Wavefunction format, in Colorer.ProcessFile. " + "(" + inFormat + ")");
                    }
                    inDataStartPos = br.BaseStream.Position;




                    // Create output file
                    using (FileStream fileStream = File.Create(outFile))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fileStream))
                        {
                            // Write the output header
                            string outFormat = (colorFunc == null) ? "AMPLITUDE_ONLY" : "AMPLITUDE_AND_COLOR";
                            bw.Write(Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("Wavefunction3D " + outFormat.ToString() + " " + "spacing: " + latticeSpacing.ToString() + nl));
                            bw.Write(Encoding.ASCII.GetBytes("BINARY" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + sx + " " + sy + " " + sz + nl));
                            bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sx*sy*sz + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));


                            // Read {re,im} pairs from the input file, and write the corresponding ampl values to the output file
                            // (Avoid reading-in the whole input file up-front, as it may be extremely large.)
                            byte[] outPlane = new byte[sx*sy*4];
                            float maxAmpl = 0.0f;
                            for (int z = 0; z < sz; z++)
                            {
                                byte[] inPlane = br.ReadBytes(sx*sy*8);
                                int ni = 0, no = 0;;
                                for (int y = 0; y < sy; y++)
                                {
                                    for (int x = 0; x < sx; x++)
                                    {
                                        *fa3 = inPlane[ni];   // Reverse the byte order
                                        *fa2 = inPlane[ni+1];
                                        *fa1 = inPlane[ni+2];
                                        *fa0 = inPlane[ni+3];

                                        *fb3 = inPlane[ni+4];
                                        *fb2 = inPlane[ni+5];
                                        *fb1 = inPlane[ni+6];
                                        *fb0 = inPlane[ni+7];
                                        ni += 8;

                                        fa = (float) Math.Sqrt(fa*fa + fb*fb);
                                        if (fa > maxAmpl) { maxAmpl = fa; }
                                        outPlane[no]   = *fa3;
                                        outPlane[no+1] = *fa2;
                                        outPlane[no+2] = *fa1;
                                        outPlane[no+3] = *fa0;
                                        no += 4;
                                    }
                                }
                                bw.Write(outPlane);
                            }


                            // Now write the colors
                            if (colorFunc != null)
                            {
                                bw.Write(Encoding.ASCII.GetBytes("SCALARS colors unsigned_char 3" + nl));
                                bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                                br.BaseStream.Seek(inDataStartPos, SeekOrigin.Begin);
                                byte[] colorPlane = new byte[sx*sy*3];
                                for (int z = 0; z < sz; z++)
                                {
                                    byte[] inPlane = br.ReadBytes(sx*sy*8);
                                    int ni = 0, no = 0; ;
                                    for (int y = 0; y < sy; y++)
                                    {
                                        for (int x = 0; x < sx; x++)
                                        {
                                            *fa3 = inPlane[ni];    // Real part
                                            *fa2 = inPlane[ni+1];
                                            *fa1 = inPlane[ni+2];
                                            *fa0 = inPlane[ni+3];

                                            *fb3 = inPlane[ni+4];  // Imaginary part
                                            *fb2 = inPlane[ni+5];
                                            *fb1 = inPlane[ni+6];
                                            *fb0 = inPlane[ni+7];
                                            ni += 8;

                                            Color color = (colorFunc == null) ? Color.Blue : colorFunc(fb, fa, maxAmpl);
                                            colorPlane[no]   = color.R;
                                            colorPlane[no+1] = color.G;
                                            colorPlane[no+2] = color.B;
                                            no += 3;
                                        }
                                    }
                                    bw.Write(colorPlane);
                                }
                            }
                        }
                    }
                }
            }
        }


    }
}
