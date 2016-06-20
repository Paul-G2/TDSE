using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This is the application's main form.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Tell the PostProcesser who his sibling is
            PostProcessingControl.AssociatedSolver = SolverControl;
        }

    }
}
