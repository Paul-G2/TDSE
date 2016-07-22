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
                    // Upsample one file, and save the result
                    string inputFile = vtkFiles[i];
                    WaveFunction wf = WaveFunction.ReadFromVtkFile(inputFile);
                    wf = Upsample( wf, m_upsampFactor );

                    string outFile = Path.Combine(m_outputDir, Path.GetFileName(inputFile));
                    wf.SaveToVtkFile(outFile, WaveFunction.WfSaveFormat.REAL_AND_IMAG);
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
        /// Resamples a wavefunction to a higher resolution.
        /// </summary>
        public static WaveFunction Upsample(WaveFunction inputWf, double factor)
        {
            float[][] modRealPart = UpsampleBicubic(inputWf.RealPart, factor);
            float[][] modImagPart = UpsampleBicubic(inputWf.ImagPart, factor);
  
            WaveFunction outputWf = new WaveFunction( modRealPart, modImagPart, (float)(inputWf.LatticeSpacing/factor) );

            // Match the normalization of the input wavefunction
            float normFactor = (float) Math.Sqrt( inputWf.NormSq()/outputWf.NormSq() );
            outputWf.ScaleBy( normFactor );

            return outputWf;
        }



        /// <summary>
        /// Upsamples an array by a given factor, using bicubic interpolation.
        /// </summary>
        public static float[][] UpsampleBicubic(float[][] inArray, double factor)
        {
            if (factor < 1.0)
            {
                throw new ArgumentException("Upsampling factor must be greater than or equal to 1, in UpsampleBicubic");
            }

            int inWidth  = inArray.Length;
            int inHeight = (inWidth < 1) ? 0 : inArray[0].Length;
            int outWidth =  (int)Math.Max(1, inWidth  * factor);
            int outHeight = (int)Math.Max(1, inHeight * factor);

            float[][] outArray = new float[outWidth][];
            for (int i = 0; i < outWidth; i++) { outArray[i] = new float[outHeight]; }

            // Precalculate the weights and indices
            int[] xInm2, xInm1, xInp1, xInp2, yInm2, yInm1, yInp1, yInp2;
            float[] wxm2, wxm1, wxp1, wxp2, wym2, wym1, wyp1, wyp2;
            GetUpsampleArrays_BiCubic(inWidth,  factor, out xInm2, out xInm1, out xInp1, out xInp2, out wxm2, out wxm1, out wxp1, out wxp2);
            GetUpsampleArrays_BiCubic(inHeight, factor, out yInm2, out yInm1, out yInp1, out yInp2, out wym2, out wym1, out wyp1, out wyp2);

            // Loop over the output pixels
            for (int yOut = 0; yOut < outHeight; yOut++)
            {
                // Cache y-weights and offsets
                float Wym2 = wym2[yOut];
                float Wym1 = wym1[yOut];
                float Wyp1 = wyp1[yOut];
                float Wyp2 = wyp2[yOut];

                int YInm2 = yInm2[yOut];
                int YInm1 = yInm1[yOut];
                int YInp1 = yInp1[yOut];
                int YInp2 = yInp2[yOut];

                for (int xOut = 0; xOut < outWidth; xOut++)
                {
                    float Wxm2 = wxm2[xOut];
                    float Wxm1 = wxm1[xOut];
                    float Wxp1 = wxp1[xOut];
                    float Wxp2 = wxp2[xOut];
                    int XInm2 = xInm2[xOut];
                    int XInm1 = xInm1[xOut];
                    int XInp1 = xInp1[xOut];
                    int XInp2 = xInp2[xOut];

                    // Bicubic formula
                    float pixVal =
                        Wym2 * (Wxm2 * inArray[XInm2][YInm2]   +   Wxm1 * inArray[XInm1][YInm2]   +   Wxp1 * inArray[XInp1][YInm2]   +   Wxp2 * inArray[XInp2][YInm2]) +
                        Wym1 * (Wxm2 * inArray[XInm2][YInm1]   +   Wxm1 * inArray[XInm1][YInm1]   +   Wxp1 * inArray[XInp1][YInm1]   +   Wxp2 * inArray[XInp2][YInm1]) +
                        Wyp1 * (Wxm2 * inArray[XInm2][YInp1]   +   Wxm1 * inArray[XInm1][YInp1]   +   Wxp1 * inArray[XInp1][YInp1]   +   Wxp2 * inArray[XInp2][YInp1]) +
                        Wyp2 * (Wxm2 * inArray[XInm2][YInp2]   +   Wxm1 * inArray[XInm1][YInp2]   +   Wxp1 * inArray[XInp1][YInp2]   +   Wxp2 * inArray[XInp2][YInp2]);

                    outArray[xOut][yOut] = pixVal;
                }
            }
            return outArray; 
        }
                            


        /// <summary>
        /// Creates the arrays of indices and weights needed for upsampling with bicubic interpolation. 
        /// </summary>
        private static void GetUpsampleArrays_BiCubic(int inSize, double upsampFactor, 
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
