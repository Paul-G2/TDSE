using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TdseSolver_2D2P
{
    /// <summary>
    /// This is the UI control for upsampling, cropping, and coloring wavefunctions.
    /// </summary>
    public partial class PostProcessingControl : UserControl
    {
        // Class data
        FolderBrowserDialog       m_folderBrowserDlg = null;
        SolverControl             m_associatedSolver = null;
        TdseUtils.Proc            m_currentProc      = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        public PostProcessingControl()
        {
            InitializeComponent();

            m_folderBrowserDlg = new FolderBrowserDialog();

            // Load last-used settings
            string lastDir = Properties.Settings.Default.LastPostProcFolder;
            InputDir_TextBox.Text = string.IsNullOrEmpty(lastDir) ? "C:\\" : lastDir;

            try
            {
                string cropString = Properties.Settings.Default.CropSettings;
                if (!string.IsNullOrEmpty(cropString))
                {
                    string[] comps = cropString.Split(',');
                    XCrop1_NUD.Value   = int.Parse(comps[0]);
                    YCrop1_NUD.Value    = int.Parse(comps[1]);
                    XCrop2_NUD.Value  = int.Parse(comps[2]);
                    YCrop2_NUD.Value = int.Parse(comps[3]);
                }
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
            int XminCrop = (int)XCrop1_NUD.Value;
            int YminCrop = (int)YCrop1_NUD.Value;
            int XmaxCrop = (int)XCrop2_NUD.Value;
            int YmaxCrop = (int)YCrop2_NUD.Value;
            Properties.Settings.Default.CropSettings = XminCrop.ToString() + "," + YminCrop.ToString() + "," + XmaxCrop.ToString() + "," + YmaxCrop.ToString();
            Properties.Settings.Default.Save();

            // Update the UI
            Stop_Btn.Enabled = true;
            PauseResume_Btn.Enabled = true;
            Main_ProgressBar.Value = 0;
            EnableInputs(false);

            // Run the Upsampler in the background
            m_currentProc = new Cropper( inputDir, XminCrop, XmaxCrop, YminCrop, YmaxCrop );
            m_currentProc.ProgressEvent += CurrentProc_ProgressEvent;
            m_currentProc.CompletionEvent += CurrentProc_CompletionEvent;
            m_currentProc.RunInBackground();
        }

        
        /// <summary>
        /// Handler for progress events from the current proc.
        /// </summary>
        void CurrentProc_ProgressEvent(TdseUtils.Proc sender)
        {
            // Update the progress bar
            int prog = -1;
            if (sender is Cropper)
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
        void CurrentProc_CompletionEvent(TdseUtils.Proc sender, RunWorkerCompletedEventArgs e)
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
                if ( (msg != null) && msg.ToLower().Contains("unsupported wavefunction format") )
                {
                    msg = "The wavefunction file is invalid, or does not support the attempted operation.";
                }
                MessageBox.Show("Abnormal termination.\n\n" + msg);
            }

            // Set my input dir equal to the last-used output dir
            if ( !e.Cancelled && (e.Error == null) )
            {
                string lastOutputDir = (sender is Cropper) ? ((Cropper)sender).LastOutputDir : null;

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
            InputDir_TextBox.Enabled = val;
            Crop_Btn.Enabled = val;
            XCrop1_NUD.Enabled = XCrop2_NUD.Enabled = YCrop1_NUD.Enabled = YCrop2_NUD.Enabled = val;
        }

    }
}
