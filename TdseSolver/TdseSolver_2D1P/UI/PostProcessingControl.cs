using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TdseSolver_2D1P
{
    /// <summary>
    /// This is the UI control for upsampling, cropping, and coloring wavefunctions.
    /// </summary>
    public partial class PostProcessingControl : UserControl
    {
        // Class data
        FolderBrowserDialog m_folderBrowserDlg = null;
        ColorCodeBuilder    m_colorBuilder     = null;
        SolverControl       m_associatedSolver = null;
        TdseSolver.Proc     m_currentProc      = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        public PostProcessingControl()
        {
            InitializeComponent();

            m_folderBrowserDlg = new FolderBrowserDialog();
            m_colorBuilder = new ColorCodeBuilder();

            // Load last-used settings
            string lastDir = Properties.Settings.Default.LastPostProcFolder;
            InputDir_TextBox.Text = string.IsNullOrEmpty(lastDir) ? "C:\\" : lastDir;

            try
            {
                string cropString = Properties.Settings.Default.CropSettings;
                if (!string.IsNullOrEmpty(cropString))
                {
                    string[] comps = cropString.Split(',');
                    LeftCrop_NUD.Value   = int.Parse(comps[0]);
                    TopCrop_NUD.Value    = int.Parse(comps[1]);
                    RightCrop_NUD.Value  = int.Parse(comps[2]);
                    BottomCrop_NUD.Value = int.Parse(comps[3]);
                }
            }
            catch { }


            try
            {
                UpsampleFactor_NUD.Value = (decimal) Properties.Settings.Default.UpsampleFactor;
            }
            catch { }


        }


        /// <summary>
        /// Gets or sets the associated SolverControl.
        /// </summary>
        public SolverControl AssociatedSolver
        {
            get
            {
                return m_associatedSolver;
            }
            set
            {
                // Subscribe to events from the solver
                if (m_associatedSolver != null)
                {
                    m_associatedSolver.OnNormalCompletion -= OnSolverCompletion;
                }
                if (value != null)
                {
                    m_associatedSolver = value;
                    m_associatedSolver.OnNormalCompletion += OnSolverCompletion;
                }
            }
        }


        /// <summary>
        /// Handler for Completion events from the associated Solver
        /// </summary>
        void OnSolverCompletion(object sender, EventArgs e)
        {
            // Set my input dir equal to the Solver's last output dir
            InputDir_TextBox.Text = m_associatedSolver.LastOutputDir;
            Properties.Settings.Default.LastPostProcFolder = InputDir_TextBox.Text;
            Properties.Settings.Default.Save();
        }

        
        /// <summary>
        /// Handler for clicks on the SelectData button.
        /// </summary>
        private void SelectFolder_Btn_Click(object sender, EventArgs e)
        {
            // Launch the folder browser
            m_folderBrowserDlg.Description = "Select Input Folder";
            m_folderBrowserDlg.RootFolder = Environment.SpecialFolder.MyComputer;
            m_folderBrowserDlg.SelectedPath = InputDir_TextBox.Text;

            // Accept the selection
            DialogResult dlgResult = m_folderBrowserDlg.ShowDialog();
            if (dlgResult == DialogResult.OK || dlgResult == DialogResult.Yes)
            {
                InputDir_TextBox.Text = m_folderBrowserDlg.SelectedPath;
                Properties.Settings.Default.LastPostProcFolder = m_folderBrowserDlg.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }


        /// <summary>
        /// Click handler for the Pause/Resume button.
        /// </summary>
        private void PauseResume_Btn_Click(object sender, EventArgs e)
        {
            if (PauseResume_Btn.Text == "Pause")
            {
                PauseResume_Btn.Text = "Resume";
                m_currentProc.Pause();
            }
            else
            {
                PauseResume_Btn.Text = "Pause";
                m_currentProc.Resume();
            }
        }


        /// <summary>
        /// Click handler for the Stop button.
        /// </summary>
        private void Stop_Btn_Click(object sender, EventArgs e)
        {
            Stop_Btn.Enabled = false;
            PauseResume_Btn.Enabled = false;
                
            m_currentProc.Cancel();
        }
        
        
        /// <summary>
        /// Click handler for the Upsample button.
        /// </summary>
        private void Upsample_Btn_Click(object sender, EventArgs e)
        {
            // Check whether we actually have any files to process
            string inputDir = InputDir_TextBox.Text;
            if ( !Directory.Exists(inputDir) )
            {
                MessageBox.Show("The chosen input directory does not exist.");
                return;
            }

            string[] vtkFiles = Directory.GetFiles(inputDir, "*.vtk");
            if ( (vtkFiles==null) || (vtkFiles.Length == 0) )
            {
                MessageBox.Show("No vtk files found in the chosen input directory.");
                return;
            }

            // Save the settings
            Properties.Settings.Default.UpsampleFactor = (float) UpsampleFactor_NUD.Value;
            Properties.Settings.Default.Save();

            // Update the UI
            Stop_Btn.Enabled = true;
            PauseResume_Btn.Enabled = true;
            Main_ProgressBar.Value = 0;
            EnableInputs(false);

            // Run the Upsampler in the background
            m_currentProc = new Upsampler( inputDir, (double)UpsampleFactor_NUD.Value );
            m_currentProc.ProgressEvent += CurrentProc_ProgressEvent;
            m_currentProc.CompletionEvent += CurrentProc_CompletionEvent;
            m_currentProc.RunInBackground();
        }


        /// <summary>
        /// Click handler for the Crop button.
        /// </summary>
        private void Crop_Btn_Click(object sender, EventArgs e)
        {
            // Check whether we actually have any files to process
            string inputDir = InputDir_TextBox.Text;
            if ( !Directory.Exists(inputDir) )
            {
                MessageBox.Show("The chosen input directory does not exist.");
                return;
            }

            string[] vtkFiles = Directory.GetFiles(inputDir, "*.vtk");
            if ( (vtkFiles==null) || (vtkFiles.Length == 0) )
            {
                MessageBox.Show("No vtk files found in the chosen input directory.");
                return;
            }

            // Save the settings
            int L = (int)LeftCrop_NUD.Value;
            int T = (int)TopCrop_NUD.Value;
            int R = (int)RightCrop_NUD.Value;
            int B = (int)BottomCrop_NUD.Value;
            Properties.Settings.Default.CropSettings = L.ToString() + "," + T.ToString() + "," + R.ToString() + "," + B.ToString();
            Properties.Settings.Default.Save();

            // Update the UI
            Stop_Btn.Enabled = true;
            PauseResume_Btn.Enabled = true;
            Main_ProgressBar.Value = 0;
            EnableInputs(false);

            // Run the Upsampler in the background
            m_currentProc = new Cropper( inputDir, L, T, R, B );
            m_currentProc.ProgressEvent += CurrentProc_ProgressEvent;
            m_currentProc.CompletionEvent += CurrentProc_CompletionEvent;
            m_currentProc.RunInBackground();
        }

        
        /// <summary>
        /// Click handler for the ReColor button.
        /// </summary>
        private void ReColor_Btn_Click(object sender, EventArgs e)
        {
            // Check whether we actually have any files to process
            string inputDir = InputDir_TextBox.Text;
            if ( !Directory.Exists(inputDir) )
            {
                MessageBox.Show("The chosen input directory does not exist.");
                return;
            }

            string[] vtkFiles = Directory.GetFiles(inputDir, "*.vtk");
            if ( (vtkFiles==null) || (vtkFiles.Length == 0) )
            {
                MessageBox.Show("No vtk files found in the chosen input directory.");
                return;
            }


            // Run the Colorer in the background
            DialogResult dlgResult = m_colorBuilder.ShowDialog();
            if (dlgResult == DialogResult.OK || dlgResult == DialogResult.Yes)
            {
                // Update the UI
                Stop_Btn.Enabled = true;
                PauseResume_Btn.Enabled = true;
                Main_ProgressBar.Value = 0;
                EnableInputs(false);

                m_currentProc = new Colorer( inputDir, m_colorBuilder );
                m_currentProc.ProgressEvent += CurrentProc_ProgressEvent;
                m_currentProc.CompletionEvent += CurrentProc_CompletionEvent;
                m_currentProc.RunInBackground();
            }

        }


        /// <summary>
        /// Handler for progress events from the current proc.
        /// </summary>
        void CurrentProc_ProgressEvent(TdseSolver.Proc sender)
        {
            // Update the progress bar
            int prog = -1;
            if (sender is Upsampler)
            {
                prog = ( (Upsampler)sender ).Progress;
            }
            else if (sender is Colorer)
            {
                prog = ( (Colorer)sender ).Progress;
            }
            else if (sender is Cropper)
            {
                prog = ( (Cropper)sender ).Progress;
            }
            
            if ((prog >= 0) && (Main_ProgressBar.Value != prog))
            {
                Main_ProgressBar.Value = prog;                         // Workaround for slow ProgressBar updates
                Main_ProgressBar.Value = (prog > 0) ? prog - 1 : prog; //
                Main_ProgressBar.Value = prog;                         //
            }
        }
        
        
        /// <summary>
        /// Handler for completion events from the current proc.
        /// </summary>
        void CurrentProc_CompletionEvent(TdseSolver.Proc sender, RunWorkerCompletedEventArgs e)
        {
            PauseResume_Btn.Text = "Pause";
            PauseResume_Btn.Enabled = false;
            Stop_Btn.Enabled = false;
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

            // Set my input dir equal to the last-used output dir
            if ( !e.Cancelled && (e.Error == null) )
            {
                string lastOutputDir = (sender is Upsampler) ? ((Upsampler)sender).LastOutputDir :
                    (sender is Cropper) ? ((Cropper)sender).LastOutputDir : null;

                if (lastOutputDir != null) 
                {
                    InputDir_TextBox.Text = lastOutputDir;
                    Properties.Settings.Default.LastPostProcFolder = InputDir_TextBox.Text;
                    Properties.Settings.Default.Save();
                }
            }

            m_currentProc = null;
        }



        /// <summary>
        /// Enables or disables the parameter widgets. 
        /// </summary>
        void EnableInputs(bool val)
        {
            SelectFolder_Btn.Enabled = val;
            Upsample_Btn.Enabled = val;
            ReColor_Btn.Enabled = val;
            Crop_Btn.Enabled = val;
            LeftCrop_NUD.Enabled = RightCrop_NUD.Enabled = TopCrop_NUD.Enabled = BottomCrop_NUD.Enabled = val;
            UpsampleFactor_NUD.Enabled = val;
        }

    }
}
