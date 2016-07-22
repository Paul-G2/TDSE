using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class provides methods for smoothing a wavefunction.
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
            // Get the list of files to process
            string[] vtkFiles = Directory.GetFiles(m_inputDir, "*.vtk");
            if ( (vtkFiles==null) || (vtkFiles.Length == 0) ) { return; }

            // Initialize the output directory
            m_outputDir = CreateOutputDir(m_inputDir);
            string paramsFile = Path.Combine(m_inputDir, "Params.txt");
            if (File.Exists(paramsFile))
            {
                File.Copy(paramsFile, Path.Combine(m_outputDir, Path.GetFileName(paramsFile)));
            }

            // Process the files
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
                int sx = -1,  sy = -1,  sz = -1;
                string format = "";
                float latticeSpacing = 0.0f;
                string nl = Environment.NewLine;

                float[] kernel = CreateGaussianKernel(m_smoothingFactor);
                int sk = kernel.Length;
                int hsk = (sk-1)/2;
                
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
                    if (string.IsNullOrEmpty(format) || (sx < 0) || (sy < 0) || (sz < 0))
                    {
                        throw new ArgumentException("Invalid Wavefunction file, in Colorer.ProcessFile.");
                    }
                    if (format != "REAL_AND_IMAG")
                    {
                        throw new ArgumentException("Unsupported Wavefunction format, in Smoother.ProcessFile. " + "(" + format + ")");
                    }

                    // Allocate some storage
                    float[][][] slab    = TdseUtils.Misc.Allocate3DArray(sk, sy, 2*sx);
                    float[][] outPlane  = TdseUtils.Misc.Allocate2DArray(sy, 2*sx);
                    float[][] workSpace = TdseUtils.Misc.Allocate2DArray(sy, 2*sx);


                    // Create and open the output file
                    using (FileStream fileStream = File.Create(outFile))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fileStream))
                        {
                            // Write the output header
                            bw.Write(Encoding.ASCII.GetBytes("# vtk DataFile Version 3.0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("Wavefunction3D " + "REAL_AND_IMAG" + " " + "spacing: " + latticeSpacing.ToString() + nl));
                            bw.Write(Encoding.ASCII.GetBytes("BINARY" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DATASET STRUCTURED_POINTS" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + sx + " " + sy + " " + sz + nl));
                            bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sx*sy*sz + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SCALARS wf float 2" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));

                            // Read the initial few planes
                            for (int i=hsk; i<sk; i++)
                            {
                                GetNextXYPlane(br, kernel, workSpace, slab[i]);
                            }
                            for (int i=0; i<hsk; i++)
                            {
                                TdseUtils.Misc.Copy2DArray(slab[sk-1-i], slab[i]); // Mirror boundary conditions on z
                            }
                            // Smooth along z, and write-out the result
                            SmoothAlongZ(slab, kernel, outPlane);
                            WriteXYPlane(bw, outPlane);


                            // Loop over remaining planes
                            for (int z=1; z<sz; z++)
                            {
                                // Cycle the z-planes, and read-in a new one
                                float[][] temp = slab[0];
                                for (int i=0; i<sk-1; i++)
                                {
                                    slab[i] = slab[i+1];
                                }
                                slab[sk-1] = temp;
                                if (z < sz - hsk)
                                {
                                    GetNextXYPlane(br, kernel, workSpace, slab[sk-1]);
                                }
                                else
                                {
                                    TdseUtils.Misc.Copy2DArray(slab[2*(sz-1-z)], slab[sk-1]); // Mirror boundary conditions on z
                                }

                                // Smooth along z, and write-out the result
                                SmoothAlongZ(slab, kernel, outPlane);
                                WriteXYPlane(bw, outPlane);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Reads a plane of (re,im) values, and smooths it.
        /// </summary>
        private static void GetNextXYPlane(BinaryReader br, float[] kernel, float[][] workSpace, float[][] outPlane)
        {
            int sy  = outPlane.Length;
            int sx2 = outPlane[0].Length;

            unsafe
            {
                float fVal = 0.0f;
                byte* fv0 = (byte*)(&fVal);
                byte* fv1 = fv0 + 1;
                byte* fv2 = fv0 + 2;
                byte* fv3 = fv0 + 3;

                byte[] inPlane = br.ReadBytes(sx2*sy*4);

                int n = 0;
                for (int y = 0; y < sy; y++)
                {
                    float[] outPlaneY = outPlane[y];

                    for (int x = 0; x < sx2; x++)
                    {
                        *fv3 = inPlane[n];   // Reverse the byte order
                        *fv2 = inPlane[n+1];
                        *fv1 = inPlane[n+2];
                        *fv0 = inPlane[n+3];
                        outPlaneY[x] = fVal;
                        n += 4;
                    }
                }
            }

            // Apply smoothing
            SmoothXYPlane(outPlane, workSpace, kernel);
        }



        /// <summary>
        /// Writes a plane of values.
        /// </summary>
        private static void WriteXYPlane(BinaryWriter bw, float[][] plane)
        {
            int sy = plane.Length;
            int sx2 = plane[0].Length;

            unsafe
            {
                float fVal = 0.0f;
                byte* fv0 = (byte*)(&fVal);
                byte* fv1 = fv0 + 1;
                byte* fv2 = fv0 + 2;
                byte* fv3 = fv0 + 3;

                byte[] outBytes = new byte[sx2*sy*4];

                int n = 0;
                for (int y = 0; y < sy; y++)
                {
                    float[] planeY = plane[y];

                    for (int x = 0; x < sx2; x++)
                    {
                        fVal = planeY[x];
                        outBytes[n]   = *fv3;
                        outBytes[n+1] = *fv2;
                        outBytes[n+2] = *fv1;
                        outBytes[n+3] = *fv0;
                        n += 4;
                    }
                }
                bw.Write(outBytes);
            }
        }
        
        
        /// <summary>
        /// Smooths a 2D array of (re,im) pairs, with a gaussian kernel.
        /// </summary>
        private static void SmoothXYPlane(float[][] array, float[][] workSpace, float[] kernel)
        {
            int sy = array.Length;
            int sx2 = array[0].Length;

            if ( (sy != workSpace.Length) || (sx2 != workSpace[0].Length) )
            {
                throw new ArgumentException("Array size mismatch, in Smoother.SmoothXYPlane");
            }

            int sk = kernel.Length;
            int hsk = (sk-1)/2;
            int hsk2 = (sk-1);


            // x-convolution
            for (int y = 0; y < sy; y++)
            {
                float[] arrayY  = array[y];
                float[] workSpaceY = workSpace[y];

                // Fast inner loop with no boundary checking
                for (int x=hsk2; x<sx2-hsk2; x++)
                {
                    int xp = x - hsk2;
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        pixVal += kernel[i] * arrayY[xp + 2*i];
                    }
                    workSpaceY[x] = pixVal;
                }
                // Front-edge pixels
                for (int x=0; x<hsk2; x++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = x - hsk2 + 2*i;
                        if (n < 0) { n = -n; }
                        pixVal += kernel[i] * arrayY[n];
                    }
                    workSpaceY[x] = pixVal;
                }
                // Back-Edge pixels
                for (int x=sx2-hsk2; x<sx2; x++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = x - hsk2 + 2*i;
                        if (n >= sx2) { n = 2*sx2-2-n; }
                        pixVal += kernel[i] * arrayY[n];
                    }
                    workSpaceY[x] = pixVal;
                }
            }
            

            // y-convolution
            for (int x = 0; x < sx2; x++)
            {
                // Fast inner loop with no boundary checking
                for (int y=hsk; y<sy-hsk; y++)
                {
                    int yp = y - hsk;
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        pixVal += kernel[i] * workSpace[yp+i][x];
                    }
                    array[y][x] = pixVal;
                }
                // Front-edge pixels
                for (int y=0; y<hsk; y++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = y - hsk + i;
                        if (n < 0) { n = -n; }
                        pixVal += kernel[i] * workSpace[n][x];
                    }
                    array[y][x] = pixVal;
                }
                // Back-Edge pixels
                for (int y=sy-hsk; y<sy; y++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        int n = y - hsk + i;
                        if (n >= sy) { n = 2*sy-2-n; }
                        pixVal += kernel[i] * workSpace[n][x];
                    }
                    array[y][x] = pixVal;
                }
            }
        }
                           


        /// <summary>
        /// Smooths a slab along the z-direction.
        /// </summary>
        private static void SmoothAlongZ(float[][][] inSlab, float[] kernel, float[][] outPlane)
        {
            int sz = inSlab.Length;
            int sy = inSlab[0].Length;
            int sx = inSlab[0][0].Length;
            int sk = kernel.Length;
            int hsk = (sk-1)/2;

            if ((sy != outPlane.Length) || (sx != outPlane[0].Length) || (sz != sk) )
            {
                throw new ArgumentException("Array size mismatch, in Smoother.SmoothAlongZ");
            }

            // z-convolution
            for (int y = 0; y < sy; y++)
            {
                for (int x = 0; x < sx; x++)
                {
                    float pixVal = 0.0f;
                    for (int i=0; i<sk; i++)
                    {
                        pixVal += kernel[i] * inSlab[i][y][x];
                    }
                    outPlane[y][x] = pixVal;
                }
            }
            
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
