using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_2D2P
{
    /// <summary>
    /// This class provides methods for cropping a wavefunction to a smaller grid.
    /// </summary>
    class Cropper : TdseUtils.Proc
    {
        // Class data
        string m_inputDir           = "";
        string m_outputDir          = "";
        int    m_xminCrop           = 0;
        int    m_xmaxCrop           = 0;
        int    m_yminCrop           = 0;
        int    m_ymaxCrop           = 0;
        int    m_numFilesToProcess  = 0;
        int    m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Cropper(string inputDir, int xminCrop, int xmaxCrop, int yminCrop, int ymaxCrop)
        {
            m_inputDir = inputDir;
            m_xminCrop = xminCrop;
            m_xmaxCrop = xmaxCrop;
            m_yminCrop = yminCrop;
            m_ymaxCrop = ymaxCrop;
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
                    // Crop one file, and save the result
                    string inputFile = vtkFiles[i];
                    ProbabilityDensity[] probs = ProbabilityDensity.ReadFromVtkFile(inputFile);
                    probs[0] = Crop( probs[0], m_xminCrop, m_xmaxCrop, m_yminCrop, m_ymaxCrop );
                    probs[1] = Crop( probs[1], m_xminCrop, m_xmaxCrop, m_yminCrop, m_ymaxCrop );

                    string outFile = Path.Combine(m_outputDir, Path.GetFileName(inputFile));
                    ProbabilityDensity.SaveToVtkFile(probs, outFile);
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
        public static ProbabilityDensity Crop(ProbabilityDensity inputDensity, int xminCrop, int xmaxCrop, int yminCrop, int ymaxCrop)
        {
            float[][] modData = Crop(inputDensity.Data, xminCrop, xmaxCrop, yminCrop, ymaxCrop);
  
            return new ProbabilityDensity(modData, inputDensity.LatticeSpacing);
        }



        /// <summary>
        /// Crops a 2D array.
        /// </summary>
        public static float[][] Crop(float[][] inArray, int xminCrop, int xmaxCrop, int yminCrop, int ymaxCrop)
        {
            int syi  = inArray.Length;
            int sxi = (syi < 1) ? 0 : inArray[0].Length;

            if ( (xminCrop + xmaxCrop >= sxi) || (yminCrop + ymaxCrop >= syi) )
            {
                throw new ArgumentException("Invalid crop parameters, in Cropper.Crop");
            }

            int sxo = sxi - xminCrop - xmaxCrop;
            int syo = syi - yminCrop - ymaxCrop;
            float[][] outArray = TdseUtils.Misc.Allocate2DArray(syo, sxo);


            // Loop over the pixels
            for (int yi = yminCrop; yi < syi - ymaxCrop; yi++)
            {
                float[] inArrayY = inArray[yi];
                float[] outArrayY = outArray[yi-yminCrop];

                for (int xi = xminCrop; xi < sxi - xmaxCrop; xi++)
                {
                    outArrayY[xi-xminCrop] = inArrayY[xi];
                }
            }
            return outArray; 
        }
                            

    }
}
