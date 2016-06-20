using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This class creates wavefunction vtk files with specified color schemes. 
    /// </summary>
    class Colorer : TdseSolver.Proc
    {
        // Class data
        string           m_inputDir           = "";
        ColorCodeBuilder m_colorBuilder       = null;
        int              m_numFilesToProcess  = 0;
        int              m_currentFileIndex   = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Colorer(string inputDir, ColorCodeBuilder colorBuilder)
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
            string colorParamsFile = Path.Combine(outputDir, "ColorParams.txt");
            File.WriteAllText(colorParamsFile, m_colorBuilder.GetLastSavedCode().Replace("\n", "\r\n"));

            string paramsFile = Path.Combine(m_inputDir, "Params.txt");
            if (File.Exists(paramsFile))
            {
                File.Copy(paramsFile, Path.Combine(outputDir, Path.GetFileName(paramsFile)));
            }

            WaveFunction.ColorDelegate colorFunc = (re, im, maxAmpl) => { return m_colorBuilder.CalcColor(re,im,maxAmpl); };

            m_numFilesToProcess = vtkFiles.Length;
            int chunkSize = Environment.ProcessorCount;
            for (int iStart = 0; iStart < m_numFilesToProcess; iStart += chunkSize)
            {
                int iEnd = Math.Min(iStart+chunkSize, m_numFilesToProcess);
                Parallel.For(iStart, iEnd, i=> 
                {
                    // Re-color one file
                    string inputFile = vtkFiles[i];
                    WaveFunction wf = WaveFunction.ReadFromVtkFile(inputFile);

                    string outFile = Path.Combine(outputDir, Path.GetFileName(inputFile));
                    wf.SaveToVtkFile(outFile, WaveFunction.WfSaveFormat.AMPLITUDE_PHASE_AND_COLOR, colorFunc);
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
    }
}
