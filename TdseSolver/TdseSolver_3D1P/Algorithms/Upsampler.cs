using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_3D1P
{
    /// <summary>
    /// This class provides methods for upsampling a wavefunction.
    /// </summary>
    class Upsampler : TdseUtils.Proc
    {
        // Class data
        string m_inputDir           = "";
        string m_outputDir          = "";
        double m_upsampFactor       = 1.0;
        int    m_numFilesToProcess  = 0;
        int    m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Upsampler(string inputDir, double upsampFactor)
        {
            m_inputDir = inputDir;
            m_upsampFactor = upsampFactor;
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
            string dir = Path.Combine( baseDir, "Upsampled_" + index.ToString("D4") );
            while ( Directory.Exists(dir) )
            {
                index++;
                dir = Path.Combine( baseDir, "Upsampled_" + index.ToString("D4") );
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
                            latticeSpacing = (float)( Single.Parse(comps[3]) /m_upsampFactor );
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
                    int sxu = (int) Math.Max(1, sx*m_upsampFactor);
                    int syu = (int) Math.Max(1, sy*m_upsampFactor);
                    int szu = (int) Math.Max(1, sz*m_upsampFactor);
                    int sxu2 = 2 * sxu;
                    float[][] workSpace = TdseUtils.Misc.Allocate2DArray(sy, 2*sx);
                    float[][] outPlane  = TdseUtils.Misc.Allocate2DArray(syu, sxu2);

                    // Read the initial few xy planes
                    float[][][] iXYPlanes = new float[sz][][];
                    for (int zIn=0; zIn<4; zIn++)
                    {
                        iXYPlanes[zIn] = TdseUtils.Misc.Allocate2DArray(syu, sxu2);
                        GetNextXYPlane(br, iXYPlanes[zIn], workSpace, m_upsampFactor);
                    }

                    // Precalculate the z-weights and indices
                    int[] zInm2, zInm1, zInp1, zInp2;
                    float[] wzm2, wzm1, wzp1, wzp2;
                    GetUpsampleArrays(sz, m_upsampFactor, out zInm2, out zInm1, out zInp1, out zInp2, out wzm2, out wzm1, out wzp1, out wzp2);


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
                            bw.Write(Encoding.ASCII.GetBytes("DIMENSIONS " + sxu + " " + syu + " " + szu + nl));
                            bw.Write(Encoding.ASCII.GetBytes("ORIGIN 0 0 0" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SPACING 1 1 1" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("POINT_DATA " +  sxu*syu*szu + nl));
                            bw.Write(Encoding.ASCII.GetBytes("SCALARS wf float 2" + nl));
                            bw.Write(Encoding.ASCII.GetBytes("LOOKUP_TABLE default" + nl));


                            // Loop over the output z-planes
                            for (int zOut = 0; zOut < szu; zOut++)
                            {
                                // Cache z-weights and offsets
                                float Wzm2 = wzm2[zOut];
                                float Wzm1 = wzm1[zOut];
                                float Wzp1 = wzp1[zOut];
                                float Wzp2 = wzp2[zOut];

                                float[][] pixValsZInm2 = iXYPlanes[ zInm2[zOut] ];
                                float[][] pixValsZInm1 = iXYPlanes[ zInm1[zOut] ];
                                float[][] pixValsZInp1 = iXYPlanes[ zInp1[zOut] ];
                                float[][] pixValsZInp2 = iXYPlanes[ zInp2[zOut] ];
                                if ( pixValsZInp2 == null )
                                {
                                    // Load (and upsample) the next xy plane
                                    float[][] temp = iXYPlanes[ zInp2[zOut] - 4 ];
                                    iXYPlanes[ zInp2[zOut] - 4 ] = null;
                                    GetNextXYPlane(br, temp, workSpace, m_upsampFactor);
                                    pixValsZInp2 = iXYPlanes[ zInp2[zOut] ] = temp;
                                }

                                for (int yOut = 0; yOut < syu; yOut++)
                                {
                                    float[] pixValsZInm2Y = pixValsZInm2[yOut];
                                    float[] pixValsZInm1Y = pixValsZInm1[yOut];
                                    float[] pixValsZInp1Y = pixValsZInp1[yOut];
                                    float[] pixValsZInp2Y = pixValsZInp2[yOut];
                                    float[] outPlaneY = outPlane[yOut];

                                    for (int xOut = 0; xOut < sxu2; xOut++)
                                    {
                                        outPlaneY[xOut] = Wzm2*pixValsZInm2Y[xOut] + Wzm1*pixValsZInm1Y[xOut] + Wzp1*pixValsZInp1Y[xOut] + Wzp2*pixValsZInp2Y[xOut];
                                    }
                                }
                                WriteXYPlane(bw, outPlane);
                            }
                        }
                    }
                }
            }
        }



 
                            
        /// <summary>
        /// Reads a plane of (re,im) values, and upsamples it.
        /// </summary>
        private static void GetNextXYPlane(BinaryReader br, float[][] outPlane, float[][] workSpace, double upsampFactor)
        {
            int sy  = workSpace.Length;
            int sx2 = workSpace[0].Length;

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
                    float[] workSpaceY = workSpace[y];

                    for (int x = 0; x < sx2; x++)
                    {
                        *fv3 = inPlane[n];   // Reverse the byte order
                        *fv2 = inPlane[n+1];
                        *fv1 = inPlane[n+2];
                        *fv0 = inPlane[n+3];
                        workSpaceY[x] = fVal;
                        n += 4;
                    }
                }
            }

            // Upsample
            UpsampleXYPlane(workSpace, outPlane, upsampFactor);
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
        /// Upsamples a 2D array of (re,im) pairs.
        /// </summary>
        private static void UpsampleXYPlane(float[][] inPlane, float[][] outPlane, double upsampFactor)
        {
            int syi  = inPlane.Length;
            int sxi  = inPlane[0].Length/2;
            int sxi2 = 2*sxi;

            int syu  = (int) Math.Max(1, syi*upsampFactor);
            int sxu  = (int) Math.Max(1, sxi*upsampFactor);
            int sxu2 = 2*sxu;

            if ( (syu != outPlane.Length) || (sxu2 != outPlane[0].Length) )
            {
                throw new ArgumentException("Array size mismatch, in Upsampler.UpsampleXYPlane");
            }

            // Precalculate the weights and indices
            int[] xInm2, xInm1, xInp1, xInp2, yInm2, yInm1, yInp1, yInp2;
            float[] wxm2, wxm1, wxp1, wxp2, wym2, wym1, wyp1, wyp2;
            GetUpsampleArrays(sxi, upsampFactor, out xInm2, out xInm1, out xInp1, out xInp2, out wxm2, out wxm1, out wxp1, out wxp2);
            GetUpsampleArrays(syi, upsampFactor, out yInm2, out yInm1, out yInp1, out yInp2, out wym2, out wym1, out wyp1, out wyp2);

            for (int yOut = 0; yOut < syu; yOut++)
            {
                // Cache y-weights and offsets
                float Wym2 = wym2[yOut];
                float Wym1 = wym1[yOut];
                float Wyp1 = wyp1[yOut];
                float Wyp2 = wyp2[yOut];

                float[] inPlaneYInm2 = inPlane[ yInm2[yOut] ];
                float[] inPlaneYInm1 = inPlane[ yInm1[yOut] ];
                float[] inPlaneYInp1 = inPlane[ yInp1[yOut] ];
                float[] inPlaneYInp2 = inPlane[ yInp2[yOut] ];
                float[] outPlaneYOut = outPlane[yOut];

                for (int xOut = 0; xOut < sxu; xOut++)
                {
                    float Wxm2 = wxm2[xOut];
                    float Wxm1 = wxm1[xOut];
                    float Wxp1 = wxp1[xOut];
                    float Wxp2 = wxp2[xOut];
                    int XInm2 = 2*xInm2[xOut];
                    int XInm1 = 2*xInm1[xOut];
                    int XInp1 = 2*xInp1[xOut];
                    int XInp2 = 2*xInp2[xOut];

                    // Biicubic formula
                    float pixValR = 
                        Wym2 * (Wxm2 * inPlaneYInm2[XInm2] + Wxm1 * inPlaneYInm2[XInm1] + Wxp1 * inPlaneYInm2[XInp1] + Wxp2 * inPlaneYInm2[XInp2]) +
                        Wym1 * (Wxm2 * inPlaneYInm1[XInm2] + Wxm1 * inPlaneYInm1[XInm1] + Wxp1 * inPlaneYInm1[XInp1] + Wxp2 * inPlaneYInm1[XInp2]) +
                        Wyp1 * (Wxm2 * inPlaneYInp1[XInm2] + Wxm1 * inPlaneYInp1[XInm1] + Wxp1 * inPlaneYInp1[XInp1] + Wxp2 * inPlaneYInp1[XInp2]) + 
                        Wyp2 * (Wxm2 * inPlaneYInp2[XInm2] + Wxm1 * inPlaneYInp2[XInm1] + Wxp1 * inPlaneYInp2[XInp1] + Wxp2 * inPlaneYInp2[XInp2]);
                    outPlaneYOut[2*xOut] = pixValR;

                    XInm2++; XInm1++; XInp1++; XInp2++;
                    float pixValI =
                        Wym2 * (Wxm2 * inPlaneYInm2[XInm2] + Wxm1 * inPlaneYInm2[XInm1] + Wxp1 * inPlaneYInm2[XInp1] + Wxp2 * inPlaneYInm2[XInp2]) +
                        Wym1 * (Wxm2 * inPlaneYInm1[XInm2] + Wxm1 * inPlaneYInm1[XInm1] + Wxp1 * inPlaneYInm1[XInp1] + Wxp2 * inPlaneYInm1[XInp2]) +
                        Wyp1 * (Wxm2 * inPlaneYInp1[XInm2] + Wxm1 * inPlaneYInp1[XInm1] + Wxp1 * inPlaneYInp1[XInp1] + Wxp2 * inPlaneYInp1[XInp2]) + 
                        Wyp2 * (Wxm2 * inPlaneYInp2[XInm2] + Wxm1 * inPlaneYInp2[XInm1] + Wxp1 * inPlaneYInp2[XInp1] + Wxp2 * inPlaneYInp2[XInp2]);
                    outPlaneYOut[2*xOut + 1] = pixValI;   
                }
            }
        }

        /// <summary>
        /// Creates the arrays of indices and weights needed for upsampling with cubic interpolation. 
        /// </summary>
        private static void GetUpsampleArrays(int inSize, double upsampFactor, 
            out int[] qm2,   out int[] qm1,   out int[] qp1,   out int[] qp2, 
            out float[] wm2, out float[] wm1, out float[] wp1, out float[] wp2)
        {
            // Allocate arrays for indices and weights 
            int outSize = (int)Math.Max(1, inSize * upsampFactor);
            qm2 = new int[outSize];
            qm1 = new int[outSize];
            qp1 = new int[outSize];
            qp2 = new int[outSize];
            wm2 = new float[outSize];
            wm1 = new float[outSize];
            wp1 = new float[outSize];
            wp2 = new float[outSize];

            double B = 0.0; // (B,C) determines the interpolation scheme: (1,0) gives cubic B-splines. (0, 0.5) gives Catmull-Rom splines, etc. 
            double C = 0.5;
            double k1 =  2.0 - 1.5*B - C;
            double k2 = -3.0 + 2*B + C;
            double k3 =  1.0 - B/3.0;
            double k4 = -B/6.0 - C;
            double k5 =  B + 5*C;
            double k6 = -2*B - 8*C;
            double k7 = (4*B)/3 + 4*C;

            double factorInv = 1.0 / upsampFactor;
            for (int qOut = 0; qOut < outSize; qOut++)
            {
                // Calc indices of bracketing pixels  
                double qInf = factorInv * (qOut + 0.5);
                int Qm1 = (qInf >= 0.5) ? (int)(qInf - 0.5) : -1;
                qm1[qOut] = Math.Max(Qm1, 0);
                qm2[qOut] = Math.Max(Qm1 - 1, 0);
                qp1[qOut] = Math.Min(Qm1 + 1, inSize-1);
                qp2[qOut] = Math.Min(Qm1 + 2, inSize-1);

                // Calc weights
                double sm1 = qInf - (Qm1 + 0.5);
                wm1[qOut] = (float) ( sm1*sm1*(sm1*k1 + k2) + k3 );

                double sp1 = 1.0 - sm1;
                wp1[qOut] = (float) ( sp1*sp1*(sp1*k1 + k2) + k3 );

                double sm2 = sm1 + 1.0;
                wm2[qOut] = (float) ( sm2*(sm2*sm2*k4 + sm2*k5 + k6) + k7 );

                double sp2 = sp1 + 1.0;
                wp2[qOut] = (float) ( sp2*(sp2*sp2*k4 + sp2*k5 + k6) + k7 );
            }
        }
    }
}
