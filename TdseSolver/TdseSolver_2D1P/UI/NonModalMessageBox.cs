using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TdseSolver_2D1P
{
    /// <summary>
    /// This is a small utility class for displaying messages in a non-modal dialog.
    /// </summary>
    public partial class NonModalMessageBox : Form
    {
        public NonModalMessageBox(string message, string caption)
        {
            InitializeComponent();

            this.Text = caption;
            this.Msg_Label.Text = message;
        }
    }
}
