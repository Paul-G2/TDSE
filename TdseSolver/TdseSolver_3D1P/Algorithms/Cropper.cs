using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class provides methods for cropping a wavefunction to a smaller grid.
    /// </summary>
    class Cropper : TdseUtils.Proc
    {
        // Class data
        string m_inputDir           = "";
        string m_outputDir          = "";
        int    m_xCrop1             = 0;
        int    m_xCrop2             = 0;
        int    m_yCrop1             = 0;
        int    m_yCrop2             = 0;
        int    m_zCrop1             = 0;
        int    m_zCrop2             = 0;
        int    m_numFilesToProcess  = 0;
        int    m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Cropper(string inputDir, int xCrop1, int xCrop2, int yCrop1, int yCrop2, int zCrop1, int zCrop2)
        {
            m_inputDir  = inputDir;
            m_xCrop1    = xCrop1;
            m_xCrop2    = xCrop2;
            m_yCrop1    = yCrop1;
            m_yCrop2    = yCrop2;
            m_zCrop1    = zCrop1;
            m_zCrop2    = zCrop2;
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
        /// Gets the most recent output directory.
        /// </summary>
        public string LastOutputDir
        {
            get {  return m_outputDir; }
        }


        /// <summary>
        /// Worker method.
        /// </summary>
        protected override void WorkerMethod()
        {
            string[] vtkFiles = Directory.GetFiles(m_inputDir, "*.vtk");
            if ( (vtkFiles==null) || (vtkFiles.Length == 0) ) { return; }

            // Copy the RunParams file to the output dir
            m_outputDir = CreateOutputDir(m_inputDir);
            string paramsFile = Path.Combine(m_inputDir, "Params.txt");
            if (File.Exists(paramsFile))
            {
                File.Copy(paramsFile, Path.Combine(m_outputDir, Path.GetFileName(paramsFile)));
            }

            m_numFilesToProcess = vtkFiles.Length;
            int chunkSize = Environment.ProcessorCount;
            for (int iStart = 0; iStart < m_numFilesToProcess; iStart += chunkSize)
            {
                int iEnd = Math.Min(iStart+chunkSize, m_numFilesToProcess);
                Parallel.For(iStart, iEnd, i=> 
                {
                    string inFile = vtkFiles[i];
                    string outFile = Path.Combine(m_outputDir, Path.GetFileName(inFile));
                    ProcessFile(inFile, outFile);
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
            string dir = Path.Combine( baseDir, "Cropped_" + index.ToString("D4") );
            while ( Directory.Exists(dir) )
            {
                index++;
                dir = Path.Combine( baseDir, "Cropped_" + index.ToString("D4") );
            }
            Directory.CreateDirectory(dir);

            return dir;
        }


        /// <summary>
        /// Processes a single input file.
        /// </summary>
        private void ProcessFile(string inFile, string outFile)
        {
            unsafe
            {
                int sxIn = -1, syIn = -1, szIn = -1;
                int sxOut = -1, syOut = -1, szOut = -1;
                string format = "";
                float latticeSpacing = 0.0f;
                string nl = Environment.NewLine;
                
                using (BinaryReader br = new BinaryReader(File.Open(inFile, FileMode.Open)))
                {
                    // Parse the header of the input file
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        string textLine = WaveFunction.ReadTextLine(br);

                        if (textLine.StartsWith("Wavefunction3D"))
                        {
                            string[] comps = textLine.Split(null);
                            format = comps[1];
                            latticeSpacing = Single.Parse(comps[3]);
                        }
                        else if (textLine.StartsWith("DIMENSIONS"))
                        {
                            string[] comps = textLine.Split(null);
                            sxIn = Int32.Parse(comps[1]);
                            syIn = Int32.Parse(comps[2]);
                            szIn = Int32.Parse(comps[3]);
                        }
                        else if (textLine.StartsWith("LOOKUP_TABLE default"))
                        {
                            break;
                        }
                    }
                    // Bail out if the header was not what we expected
                    if (string.IsNullOrEmpty(format) || (sxIn < 0) || (syIn < 0) || (szIn < 0))
                    {
                        throw new ArgumentException("Invalid Wavefunction file, in Cropper.ProcessFile.");
                    }



                    // Create output file
                    sxOut =  sxIn - m_xCrop1 - m_xCrop2;
                    syOut =  syIn - m_yCrop1 - m_yCrop2;
                    szOut =  szIn - m_zCrop1 - m_zCrop2;

                    using (FileStream fileStream = File.Create(outFile))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fileStream))
                        {
                            // Write the output header
                            bw.Write(Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("Wavefunction3D " + format.ToString() + " " + "spacing: " + latticeSpacing.ToString() + nl));
                            bw.Write(Encoding.ASCII.GetBytes("BINARY" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + sxOut + " " + syOut + " " + szOut + nl));
                            bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sxOut*syOut*szOut + nl));


                            // Read values from the input file, and write (some of) them to the output file.
                            // (Avoid reading-in the whole input file up-front, as it may be extremely large.)
                            if (format == "REAL_AND_IMAG")
                            {
                                bw.Write(Encoding.ASCII.GetBytes("SCALARS wf float 2" + nl));
                                bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                                byte[] outPlane = new byte[sxOut*syOut*8];
                                for (int z = 0; z < szIn-m_zCrop2; z++)
                                {
                                    byte[] inPlane = br.ReadBytes(sxIn*syIn*8);

                                    if (z < m_zCrop1) { continue; }

                                    for (int y = m_yCrop1; y < syIn-m_yCrop2; y++)
                                    {
                                        Buffer.BlockCopy(inPlane, (y*sxIn + m_xCrop1)*8, outPlane, (y-m_yCrop1)*sxOut*8, sxOut*8);
                                    }
                                    bw.Write(outPlane);
                                }
                            }
                            else if ( (format == "AMPLITUDE_ONLY") || (format == "AMPLITUDE_AND_COLOR") )
                            {
                                bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                                bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                                byte[] outPlane = new byte[sxOut*syOut*4];
                                for (int z = 0; z < szIn; z++)
                                {
                                    byte[] inPlane = br.ReadBytes(sxIn*syIn*4);

                                    if ( (z < m_zCrop1) || (z > szIn-m_zCrop2-1) ) { continue; }

                                    for (int y = m_yCrop1; y < syIn-m_yCrop2; y++)
                                    {
                                        Buffer.BlockCopy(inPlane, (y*sxIn + m_xCrop1)*4, outPlane, (y-m_yCrop1)*sxOut*4, sxOut*4);
                                    }
                                    bw.Write(outPlane);
                                }

                                if ( format == "AMPLITUDE_AND_COLOR" )
                                {
                                    bw.Write(Encoding.ASCII.GetBytes("SCALARS colors unsigned_char 3" + nl));
                                    bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                                    outPlane = new byte[sxOut*syOut*3];
                                    for (int z = 0; z < szIn-m_zCrop2; z++)
                                    {
                                        byte[] inPlane = br.ReadBytes(sxIn*syIn*3);

                                        if (z < m_zCrop1) { continue; }

                                        for (int y = m_yCrop1; y < syIn-m_yCrop2; y++)
                                        {
                                            Buffer.BlockCopy(inPlane, (y*sxIn + m_xCrop1)*3, outPlane, (y-m_yCrop1)*sxOut*3, sxOut*3);
                                        }
                                        bw.Write(outPlane);
                                    }
                                }

                            }
                            else
                            {
                                throw new ArgumentException("Unsupported Wavefunction format, in Cropper.ProcessFile. " + "(" + format + ")");
                            }
                        }
                    }
                }
            }
        }
        
        
        
                            


    }
}
