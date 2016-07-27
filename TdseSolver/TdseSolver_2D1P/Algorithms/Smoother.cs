using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class provides methods for upsampling a wavefunction (so that it looks smoother).
    /// </summary>
    class Smoother : TdseUtils.Proc
    {
        // Class data
        string m_inputDir           = "";
        string m_outputDir          = "";
        double m_smoothingFactor    = 1.0;
        int    m_numFilesToProcess  = 0;
        int    m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Smoother(string inputDir, double smoothingFactor)
        {
            m_inputDir = inputDir;
            m_smoothingFactor = smoothingFactor;
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
            string dir = Path.Combine( baseDir, "Smoothed_" + index.ToString("D4") );
            while ( Directory.Exists(dir) )
            {
                index++;
                dir = Path.Combine( baseDir, "Smoothed_" + index.ToString("D4") );
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
                int sx = -1,  sy = -1;
                string format = "";
                float latticeSpacing = 0.0f;
                string nl = Environment.NewLine;
                
                using (BinaryReader br = new BinaryReader(File.Open(inFile, FileMode.Open)))
                {
                    // Parse the header of the input file
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        string textLine = WaveFunction.ReadTextLine(br);

                        if (textLine.StartsWith("Wavefunction2D"))
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
                        }
                        else if (textLine.StartsWith("LOOKUP_TABLE default"))
                        {
                            break;
                        }
                    }
                    // Bail out if the header was not what we expected
                    if (string.IsNullOrEmpty(format) || (sx < 0) || (sy < 0))
                    {
                        throw new ArgumentException("Invalid Wavefunction file, in Colorer.ProcessFile.");
                    }
                    if ( (format != "AMPLITUDE_ONLY") && (format != "AMPLITUDE_AND_COLOR") )
                    {
                        throw new ArgumentException("Unsupported Wavefunction format, in Smoother.ProcessFile. " + "(" + format + ")");
                    }

                    // Read the amplitude values
                    byte[] bytePlane = br.ReadBytes(sx*sy*4);
                    float[][] amplitudes = TdseUtils.Misc.Allocate2DArray(sy,sx);

                    float floatVal = 0.0f;
                    byte* floatBytes0 = (byte*)(&floatVal);
                    byte* floatBytes1 = floatBytes0 + 1;
                    byte* floatBytes2 = floatBytes0 + 2;
                    byte* floatBytes3 = floatBytes0 + 3;
                        
                    int n = 0;
                    for (int y = 0; y < sy; y++)
                    {
                        float[] amplitudesY = amplitudes[y];
                        for (int x = 0; x < sx; x++)
                        {
                            *floatBytes3 = bytePlane[n];
                            *floatBytes2 = bytePlane[n+1];
                            *floatBytes1 = bytePlane[n+2];
                            *floatBytes0 = bytePlane[n+3];
                            amplitudesY[x] = floatVal;
                            n += 4;
                        }
                    }          
                
                    // Smooth the amplitudes
                    float[][] smoothedAmplitudes = Smooth(amplitudes, m_smoothingFactor, true);

                    // Create and open the output file
                    using (FileStream fileStream = File.Create(outFile))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fileStream))
                        {
                            // Write the output header
                            bw.Write(Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("Wavefunction2D " + format + " " + "spacing: " + latticeSpacing.ToString() + nl));
                            bw.Write(Encoding.ASCII.GetBytes("BINARY" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + sx + " " + sy + " 1" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sx*sy + nl));

                            // Write out the smoothed amplitudes
                            bw.Write(Encoding.ASCII.GetBytes("SCALARS amplitude float" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                            n = 0;
                            for (int y = 0; y < sy; y++)
                            {
                                float[] smY = smoothedAmplitudes[y];

                                for (int x = 0; x < sx; x++)
                                {
                                    floatVal = smY[x];
                                    bytePlane[n]   = *floatBytes3;
                                    bytePlane[n+1] = *floatBytes2;
                                    bytePlane[n+2] = *floatBytes1;
                                    bytePlane[n+3] = *floatBytes0;
                                    n += 4;
                                }
                            }
                            bw.Write(bytePlane);
                            
                            // Copy the color data from input to output
                            if (format == "AMPLITUDE_AND_COLOR")
                            {
                                WaveFunction.ReadTextLine(br);
                                WaveFunction.ReadTextLine(br);
                                bw.Write(Encoding.ASCII.GetBytes("SCALARS colors unsigned_char 3" + nl));
                                bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));
                                bw.Write( br.ReadBytes(sx*sy*3) );
                            }
                        }
                    }
                }
            }
        }
        

        /// <summary>
        /// Smooths a 2D array with a gaussian kernel.
        /// </summary>
        public static float[][] Smooth(float[][] inArray, double factor, bool normalize)
        {
            float[] kernel = CreateGaussianKernel(factor);

            int sy  = inArray.Length;
            int sx = (sy < 1) ? 0 : inArray[0].Length;
            int sk = kernel.Length;
            int hsk = (sk-1)/2;

            float[][] outArray = TdseUtils.Misc.Allocate2DArray(sy,sx);

            // x-convolution
            for (int y = 0; y < sy; y++)
            {
                float[] inArrayY = inArray[y];
                float[] outArrayY = outArray[y];

                // Fast interior loop with no boundary checking
                for (int x=hsk; x<sx-hsk; x++)
                {
                    int xp = x - hsk;
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        pixVal += kernel[i] * inArrayY[xp + i];
                    }
                    outArrayY[x] = pixVal;
                }
                // Left edge
                for (int x=0; x<hsk; x++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = x - hsk + i;
                        if (n < 0) { n = -n; }
                        pixVal += kernel[i] * inArrayY[n];
                    }
                    outArrayY[x] = pixVal;
                }
                // Right edge 
                for (int x=sx-hsk; x<sx; x++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = x - hsk + i;
                        if (n >= sx) { n = 2*sx-2-n; }
                        pixVal += kernel[i] * inArrayY[n];
                    }
                    outArrayY[x] = pixVal;
                }
            }

            // y-convolution
            float[] tempArray = new float[sy];
            for (int x = 0; x < sx; x++)
            {
                // Fast inner loop with no boundary checking
                for (int y=hsk; y<sy-hsk; y++)
                {
                    int yp = y - hsk;
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        pixVal += kernel[i] * outArray[yp+i][x];
                    }
                    tempArray[y] = pixVal;
                }
                // top-edge pixels
                for (int y=0; y<hsk; y++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = y - hsk + i;
                        if (n < 0) { n = -n; }
                        pixVal += kernel[i] * outArray[n][x];
                    }
                    tempArray[y] = pixVal;
                }
                // bottom-edge pixels
                for (int y=sy-hsk; y<sy; y++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = y - hsk + i;
                        if (n >= sy) { n = 2*sy-2-n; }
                        pixVal += kernel[i] * outArray[n][x];
                    }
                    tempArray[y] = pixVal;
                }
                for (int y=0; y<sy; y++)
                {
                    outArray[y][x] = tempArray[y];
                }
            }
            
            // Maybe rescale the output to the same normalization as the input
            if (normalize)
            {
                // Compute the normalization factor
                float inNormSq = 0.0f;
                float outNormSq = 0.0f;

                for (int y=0; y<sy; y++)
                {
                    float[] inArrayY = inArray[y];
                    float[] outArrayY = outArray[y];
                    for (int x=0; x<sx; x++)
                    {
                        float inVal = inArrayY[x];
                        float outVal = outArrayY[x];
                        inNormSq += inVal*inVal;
                        outNormSq += outVal*outVal;
                    }
                }
                float normFactor = (float) Math.Sqrt(inNormSq/outNormSq);

                // Rescale the output
                for (int y=0; y<sy; y++)
                {
                    float[] outArrayY = outArray[y];
                    for (int x=0; x<sx; x++)
                    {
                        outArrayY[x] *= normFactor;
                    }
                }
            }

            return outArray; 
        }
                            

        /// <summary>
        ///  Creates a 1-dimensional Gaussian kernel of the form exp( -r^2/sigma^2 )
        /// </summary>
        private static float[] CreateGaussianKernel( double sigma )
        {
            // Invert exp(-r^2/sigma^2) = 0.1, to get r = -sigma*sqrt(ln(0.1)) = 1.52*sigma.
            int kernelRadius = (int) Math.Ceiling(1.52 * sigma); 
                
            // Calculate the kernel
            float[] kernel = new float[2*kernelRadius + 1];
            double expFactor = 1.0 / (sigma * sigma);
            for ( int n = 0; n <= kernelRadius; n++ )
            {
                float kernelVal = (float)Math.Exp( -expFactor * (n * n) );
                kernel[kernelRadius + n] = kernel[kernelRadius - n] = kernelVal;
            }

            // Normalize it
            float norm = 0.0f;
            for ( int i = 0; i < kernel.Length; i++ )
            {
                norm += kernel[i];
            }
            for ( int i = 0; i < kernel.Length; i++ )
            {
                kernel[i] /= norm;
            }

            return kernel;
        }
    }
}
