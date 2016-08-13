using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TdseSolver_3D1P
{
    /// <summary>
    /// This is the UI control for upsampling, cropping, and coloring wavefunctions.
    /// </summary>
    public partial class PostProcessingControl : UserControl
    {
        // Class data
        FolderBrowserDialog       m_folderBrowserDlg = null;
        TdseUtils.ColorBuilder    m_colorBuilder     = null;
        SolverControl             m_associatedSolver = null;
        TdseUtils.Proc            m_currentProc      = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        public PostProcessingControl()
        {
            InitializeComponent();

            m_folderBrowserDlg = new FolderBrowserDialog();
            m_colorBuilder = new TdseUtils.ColorBuilder();

            // Load last-used settings
            string lastDir = Properties.Settings.Default.LastPostProcFolder;
            InputDir_TextBox.Text = string.IsNullOrEmpty(lastDir) ? "C:\\" : lastDir;

            try
            {
                string cropString = Properties.Settings.Default.CropSettings;
                if (!string.IsNullOrEmpty(cropString))
                {
                    string[] comps = cropString.Split(',');
                    XCrop1_NUD.Value = int.Parse(comps[0]);
                    XCrop2_NUD.Value = int.Parse(comps[1]);
                    YCrop1_NUD.Value = int.Parse(comps[2]);
                    YCrop2_NUD.Value = int.Parse(comps[3]);
                    ZCrop1_NUD.Value = int.Parse(comps[4]);
                    ZCrop2_NUD.Value = int.Parse(comps[5]);
                }
            }
            catch { }


            try
            {
                UpsampleFactor_NUD.Value = (decimal) Properties.Settings.Default.UpsampleFactor;
            }
            catch { }


            try
            {
                SmoothingFactor_NUD.Value = (decimal) Properties.Settings.Default.SmoothingFactor;
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
            m_currentProc = new Upsampler( inputDir, (float)UpsampleFactor_NUD.Value );
            m_currentProc.ProgressEvent += CurrentProc_ProgressEvent;
            m_currentProc.CompletionEvent += CurrentProc_CompletionEvent;
            m_currentProc.RunInBackground();
        }


        /// <summary>
        /// Click handler for the Smooth button.
        /// </summary>
        private void Smooth_Btn_Click(object sender, EventArgs e)
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
            Properties.Settings.Default.SmoothingFactor = (float) SmoothingFactor_NUD.Value;
            Properties.Settings.Default.Save();

            // Update the UI
            Stop_Btn.Enabled = true;
            PauseResume_Btn.Enabled = true;
            Main_ProgressBar.Value = 0;
            EnableInputs(false);

            // Run the Upsampler in the background
            m_currentProc = new Smoother( inputDir, (float)SmoothingFactor_NUD.Value );
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
            int X1 = (int)XCrop1_NUD.Value;
            int X2 = (int)XCrop2_NUD.Value;
            int Y1 = (int)YCrop1_NUD.Value;
            int Y2 = (int)YCrop2_NUD.Value;
            int Z1 = (int)ZCrop1_NUD.Value;
            int Z2 = (int)ZCrop2_NUD.Value;
            Properties.Settings.Default.CropSettings = X1.ToString() + "," + X2.ToString() + ","  +
                Y1.ToString() + "," + Y2.ToString() + "," + Z1.ToString() + "," + Z2.ToString();
            Properties.Settings.Default.Save();

            // Update the UI
            Stop_Btn.Enabled = true;
            PauseResume_Btn.Enabled = true;
            Main_ProgressBar.Value = 0;
            EnableInputs(false);

            // Run the Upsampler in the background
            m_currentProc = new Cropper( inputDir, X1, X2, Y1, Y2, Z1, Z2 );
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
        void CurrentProc_ProgressEvent(TdseUtils.Proc sender)
        {
            // Update the progress bar
            int prog = -1;
            if (sender is Upsampler)
            {
                prog = ( (Upsampler)sender ).Progress;
            }
            if (sender is Smoother)
            {
                prog = ( (Smoother)sender ).Progress;
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
                MessageBox.Show("Abnormal termination.\n\n" + msg);
            }

            // Set my input dir equal to the last-used output dir
            if ( !e.Cancelled && (e.Error == null) )
            {
                string lastOutputDir = 
                    (sender is Upsampler) ? ((Upsampler)sender).LastOutputDir :
                    (sender is Smoother) ? ((Smoother)sender).LastOutputDir :
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
            InputDir_TextBox.Enabled = val;
            Upsample_Btn.Enabled = val;
            Smooth_Btn.Enabled = val;
            ReColor_Btn.Enabled = val;
            Crop_Btn.Enabled = val;
            XCrop1_NUD.Enabled = XCrop2_NUD.Enabled = val;
            YCrop1_NUD.Enabled = YCrop2_NUD.Enabled = val;
            ZCrop1_NUD.Enabled = ZCrop2_NUD.Enabled = val;
            UpsampleFactor_NUD.Enabled = val;
            SmoothingFactor_NUD.Enabled = val;
        }


    }
}
