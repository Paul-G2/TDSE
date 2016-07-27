using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This is the UI control for entering the parameters that define a single run of the 
    /// 2-dimensional, 1-particle, time-dependent Schrodinger equation solver. (2D1P TDSE).
    /// </summary>
    public partial class SolverControl : UserControl
    {
        // Class data
        Evolver        m_evolver        = null;
        VCodeBuilder   m_vBuilder       = null;
        RunParams      m_params         = null;
        int            m_lastSavedFrame = -1;
        string         m_outputDir      = "";

        // Event that we fire when the solver completes successfully
        public event EventHandler OnNormalCompletion = null;

        

        // Constructor
        public SolverControl()
        {
            InitializeComponent();
            m_vBuilder = new VCodeBuilder(); // Also loads the last-used code snippet

            // Load the last-used params
            if ( !string.IsNullOrEmpty(Properties.Settings.Default.LastRunParams) )
            {
                try 
                {
                    UpdateUiFromParams(RunParams.FromString(Properties.Settings.Default.LastRunParams));
                }
                catch {}
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
        /// Handles click events from the Run/Stop button.
        /// </summary>
        private void RunStop_Btn_Click(object sender, EventArgs e)
        {
            // Sanity check the inputs
            if (DampingBorderWidth_NUD.Value > Math.Min(GridSizeX_NUD.Value, GridSizeY_NUD.Value))
            {
                MessageBox.Show("Damping border must be less than half the minimum grid dimension.");
                return;
            }

            if (RunStop_Btn.Text == "Stop")
            {
                RunStop_Btn.Enabled = false;
                PauseResume_Btn.Enabled = false;
                
                m_evolver.Cancel();
            }
            else
            {
                RunStop_Btn.Text = "Stop";
                PauseResume_Btn.Enabled = true;
                Main_ProgressBar.Value = 0;
                EnableInputs(false);

                m_params = GetParamsFromUi();
                Properties.Settings.Default.LastRunParams = m_params.ToString();
                Properties.Settings.Default.Save();
                CreateAnimationFrames();
            }
        }


        /// <summary>
        /// Handles click events from the Pause/Resume button.
        /// </summary>
        private void PauseResume_Btn_Click(object sender, EventArgs e)
        {
            if (PauseResume_Btn.Text == "Pause")
            {
                PauseResume_Btn.Text = "Resume";
                m_evolver.Pause();
            }
            else
            {
                PauseResume_Btn.Text = "Pause";
                m_evolver.Resume();
            }
        }


        /// <summary>
        /// Handles click events from the Potential Energy button.
        /// </summary>
        private void V_Btn_Click(object sender, EventArgs e)
        {
            // Launch the V-Builder dialog
            m_vBuilder.ShowDialog();
        }


        /// <summary>
        /// Reads the UI parameters into a RunParams object.
        /// </summary>
        private RunParams GetParamsFromUi()
        {
            RunParams parms = new RunParams();

            parms.GridSizeX = Convert.ToInt32( GridSizeX_NUD.Value );
            parms.GridSizeY = Convert.ToInt32( GridSizeY_NUD.Value );
            parms.LatticeSpacing = (float) LatticeSpacing_NUD.Value;
            parms.ParticleMass = (float) Mass_NUD.Value;

            parms.InitialWavePacketSize     = new PointF( (float)InitialPacketSizeX_NUD.Value,   (float)InitialPacketSizeY_NUD.Value   );
            parms.InitialWavePacketCenter   = new PointF( (float)InitialPacketCenterX_NUD.Value, (float)InitialPacketCenterY_NUD.Value );
            parms.InitialWavePacketMomentum = new PointF( (float)InitialMomentumX_NUD.Value,     (float)InitialMomentumY_NUD.Value     );
            parms.DampingBorderWidth        = (int) DampingBorderWidth_NUD.Value;
            parms.DampingFactor             = (float) DampingFactor_NUD.Value;

            parms.TimeStep        = (float) TimeStep_NUD.Value;
            parms.TotalTime       = (float) TotalTime_NUD.Value;
            parms.NumFramesToSave = (int)   NumFrames_NUD.Value;

            parms.MultiThread = MultiThread_CheckBox.Checked;

            parms.VCode = RunParams.FromString(Properties.Settings.Default.LastRunParams).VCode;

            return parms;
        }


        /// <summary>
        /// Sets the UI values from a RunParams object.
        /// </summary>
        private RunParams UpdateUiFromParams(RunParams parms)
        {
            GridSizeX_NUD.Value = parms.GridSizeX;
            GridSizeY_NUD.Value = parms.GridSizeY;
            LatticeSpacing_NUD.Value = (decimal) parms.LatticeSpacing;
            Mass_NUD.Value = (decimal) parms.ParticleMass;

            InitialPacketSizeX_NUD.Value   = (decimal) parms.InitialWavePacketSize.X;
            InitialPacketSizeY_NUD.Value   = (decimal) parms.InitialWavePacketSize.Y;
            InitialPacketCenterX_NUD.Value = (decimal) parms.InitialWavePacketCenter.X;
            InitialPacketCenterY_NUD.Value = (decimal) parms.InitialWavePacketCenter.Y;
            InitialMomentumX_NUD.Value     = (decimal) parms.InitialWavePacketMomentum.X;
            InitialMomentumY_NUD.Value     = (decimal) parms.InitialWavePacketMomentum.Y;

            DampingBorderWidth_NUD.Value = parms.DampingBorderWidth;
            DampingFactor_NUD.Value = (decimal) parms.DampingFactor;

            TimeStep_NUD.Value  = (decimal) parms.TimeStep;
            TotalTime_NUD.Value = (decimal) parms.TotalTime;
            NumFrames_NUD.Value = (decimal) parms.NumFramesToSave;

            MultiThread_CheckBox.Checked = parms.MultiThread;

            m_vBuilder.SetCode(parms.VCode);

            return parms;
        }


        /// <summary>
        /// Evolves the wavefunction and saves keyframes to disk.
        /// </summary>
        private void CreateAnimationFrames()
        {
            // Get a fresh output directory
            m_outputDir = CreateOutputDir();

            // Write the run parameters to a file
            string paramsFile = Path.Combine(m_outputDir, "Params.txt");
            File.WriteAllText(paramsFile, m_params.ToString().Replace("\n", "\r\n"));


            // Create the initial wavefunction
            WaveFunction wf = WaveFunctionUtils.CreateGaussianWavePacket(
                m_params.GridSizeX, m_params.GridSizeY, m_params.LatticeSpacing, m_params.ParticleMass,
                m_params.InitialWavePacketCenter, 
                m_params.InitialWavePacketSize,
                m_params.InitialWavePacketMomentum,
                m_params.MultiThread
            );

            // Save the initial wf as Frame 0 
            wf.SaveToVtkFile( Path.Combine(m_outputDir, "Frame_0000.vtk"), m_params.SaveFormat );
            m_lastSavedFrame = 0;


            // Create an Evolver and run it in the background
            Evolver.VDelegate V = (x,y,t, m,sx,sy) => { return m_vBuilder.V(x,y,t, m,sx,sy); };
            m_evolver = new Evolver(wf, m_params.TotalTime, m_params.TimeStep, V, false, m_params.ParticleMass, 1, 
                m_params.DampingBorderWidth, m_params.DampingFactor, m_params.MultiThread);
            m_evolver.ProgressEvent += Evolver_ProgressEvent;
            m_evolver.CompletionEvent += Evolver_CompletionEvent;
            m_evolver.RunInBackground();
        }



        /// <summary>
        /// Handles Progress events from the Evolver.
        /// </summary>
        void Evolver_ProgressEvent(TdseUtils.Proc sender)
        {
            Evolver evolver = (Evolver) sender;

            // If the current frame is a keyframe, then save it.
            double frameInterval = (m_params.NumFramesToSave <= 1) ? 1 : m_params.TotalTime/(m_params.NumFramesToSave-1);
            if ( evolver.CurrentTimeStepIndex == (int)Math.Round( (m_lastSavedFrame+1)*frameInterval/m_params.TimeStep ) )
            {
                if (m_lastSavedFrame + 1 < m_params.NumFramesToSave)
                {
                    string outFile = Path.Combine(m_outputDir, "Frame_" + (m_lastSavedFrame + 1).ToString("D4") + ".vtk");
                    evolver.Wf.SaveToVtkFile(outFile, m_params.SaveFormat);
                }
                m_lastSavedFrame++;
            }

            // Update the progress bar
            int val = (100*evolver.CurrentTimeStepIndex)/(evolver.TotalNumTimeSteps);
            if (Main_ProgressBar.Value != val)
            {
                Main_ProgressBar.Value = val;                       // Workaround for slow ProgressBar updates
                Main_ProgressBar.Value = (val > 0) ? val - 1 : val; //
                Main_ProgressBar.Value = val;                       //
            }
        }


        /// <summary>
        /// Handles Completion events from the Evolver.
        /// </summary>
        void Evolver_CompletionEvent(TdseUtils.Proc sender, RunWorkerCompletedEventArgs e)
        {
            // Update the UI
            RunStop_Btn.Text = "Run";
            PauseResume_Btn.Text = "Pause";
            PauseResume_Btn.Enabled = false;
            RunStop_Btn.Enabled = true;
            EnableInputs(true);

            if (e.Cancelled)
            {
                Main_ProgressBar.Value = 0;
            }

            // Report any errors
            if (e.Error != null)
            {
                string msg;
                try 
                {
                    msg = e.Error.InnerException.Message + " \n" + e.Error.InnerException.StackTrace;
                } 
                catch 
                {
                    msg = e.Error.Message;
                }
                MessageBox.Show("Abnormal termination.\n\n" + msg);
            }

            // Maybe notify
            if ( !e.Cancelled && (e.Error == null) )
            {
                if (OnNormalCompletion != null) { OnNormalCompletion(this, EventArgs.Empty); }
            }
        }


        /// <summary>
        /// Handler for drag-enter events
        /// </summary>
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }

        /// <summary>
        /// Handler for drag-drop events
        /// </summary>
        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if ( e.Data.GetDataPresent( DataFormats.FileDrop ) )
            {
                string[] files = (string[])e.Data.GetData( DataFormats.FileDrop );
                if ( files.Length > 0 )
                {
                    try
                    {
                        string fileContents = File.ReadAllText(files[0]);
                        RunParams parms = RunParams.FromString(fileContents);
                        UpdateUiFromParams(parms);
                    }
                    catch
                    {
                        MessageBox.Show ("Invalid parameter file");
                    }
                }
            }
        }


        /// <summary>
        /// Creates a directory to hold the results.
        /// </summary>
        static string CreateOutputDir()
        {
            // Get the output drive
            string outputDrive = Properties.Settings.Default.OutputDrive;
            if (string.IsNullOrEmpty(outputDrive)) { outputDrive = "Z"; }
            outputDrive = outputDrive.Substring(0,1);

            DriveInfo[] driveInfo = DriveInfo.GetDrives();
            if (driveInfo.Where(d => d.IsReady && d.Name.StartsWith(outputDrive)).Count() < 1) 
            { 
                outputDrive = "Z"; 
                if (driveInfo.Where(d => d.IsReady && d.Name.StartsWith(outputDrive)).Count() < 1) 
                { 
                    outputDrive = "D"; 
                    if (driveInfo.Where(d => d.IsReady && d.Name.StartsWith(outputDrive)).Count() < 1)
                    {
                        outputDrive = "C";
                        if (driveInfo.Where(d => d.IsReady && d.Name.StartsWith(outputDrive)).Count() < 1)
                        {
                            outputDrive = driveInfo[0].Name.Substring(0,1);
                        }
                    }
                }
            }

            // Save the selected drive
            Properties.Settings.Default.OutputDrive = outputDrive;
            Properties.Settings.Default.Save();

            // Create a fresh direcory
            int index = 0;
            string dir = outputDrive + ":\\" + "WfAnimations2D" + "\\" + index.ToString("D4"); 
            while ( Directory.Exists(dir) )
            {
                index++;
                dir = outputDrive + ":\\" + "WfAnimations2D" + "\\" + index.ToString("D4"); 
            }
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create output directory " + dir + "\n" + ex.Message);
                throw (ex);
            }

            return dir;
        }


        /// <summary>
        /// Enables or disables the parameter widgets. 
        /// </summary>
        void EnableInputs(bool val)
        {
            GridSizeX_NUD.Enabled = val;
            GridSizeY_NUD.Enabled = val;
            LatticeSpacing_NUD.Enabled = val;
            Mass_NUD.Enabled = val;
            InitialPacketSizeX_NUD.Enabled = val;
            InitialPacketSizeY_NUD.Enabled = val;
            InitialPacketCenterX_NUD.Enabled = val;
            InitialPacketCenterY_NUD.Enabled = val;
            InitialMomentumX_NUD.Enabled = val;
            InitialMomentumY_NUD.Enabled = val;
            DampingBorderWidth_NUD.Enabled = val;
            DampingFactor_NUD.Enabled = val;
            TimeStep_NUD.Enabled = val;
            TotalTime_NUD.Enabled = val;
            NumFrames_NUD.Enabled = val;
            V_Btn.Enabled = val;
            MultiThread_CheckBox.Enabled = val;
            this.AllowDrop = val;
        }


    }
}
