using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class provides methods for cropping a wavefunction to a smaller grid.
    /// </summary>
    class Cropper : TdseUtils.Proc
    {
        // Class data
        string m_inputDir           = "";
        string m_outputDir          = "";
        int    m_leftCrop           = 0;
        int    m_rightCrop          = 0;
        int    m_topCrop            = 0;
        int    m_bottomCrop         = 0;
        int    m_numFilesToProcess  = 0;
        int    m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Cropper(string inputDir, int leftCrop, int topCrop, int rightCrop, int bottomCrop)
        {
            m_inputDir   = inputDir;
            m_leftCrop   = leftCrop;
            m_rightCrop  = rightCrop;
            m_topCrop    = topCrop;
            m_bottomCrop = bottomCrop;
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
                    wf = Crop( wf, m_leftCrop, m_topCrop, m_rightCrop, m_bottomCrop );

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
        /// Crops a wavefunction.
        /// </summary>
        public static WaveFunction Crop(WaveFunction wf, int leftCrop, int topCrop, int rightCrop, int bottomCrop)
        {
            float[][] modRealPart = Crop(wf.RealPart, leftCrop, topCrop, rightCrop, bottomCrop);
            float[][] modImagPart = Crop(wf.ImagPart, leftCrop, topCrop, rightCrop, bottomCrop);
  
            return new WaveFunction(modRealPart, modImagPart, wf.LatticeSpacing);
        }



        /// <summary>
        /// Crops a 2D array.
        /// </summary>
        public static float[][] Crop(float[][] inArray, int leftCrop, int topCrop, int rightCrop, int bottomCrop)
        {
            int inWidth  = inArray.Length;
            int inHeight = (inWidth < 1) ? 0 : inArray[0].Length;

            if ( (leftCrop + rightCrop >= inWidth) || (topCrop + bottomCrop >= inHeight) )
            {
                throw new ArgumentException("Invalid crop parameters, in Cropper.Crop");
            }

            int outWidth =  inWidth - leftCrop - rightCrop;
            int outHeight = inHeight - topCrop - bottomCrop;

            float[][] outArray = new float[outWidth][];
            for (int i = 0; i < outWidth; i++) { outArray[i] = new float[outHeight]; }


            // Loop over the pixels
            for (int xIn = leftCrop; xIn < inWidth - rightCrop; xIn++)
            {
                float[] inArrayX = inArray[xIn];
                float[] outArrayX = outArray[xIn-leftCrop];

                for (int yIn = topCrop; yIn < inHeight - bottomCrop; yIn++)
                {
                    outArrayX[yIn-topCrop] = inArrayX[yIn];
                }
            }
            return outArray; 
        }
                            


    }
}
