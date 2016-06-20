using System;
using System.CodeDom.Compiler;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This is the UI control for entering custom code that is used to color the wavefuctions.
    /// </summary>
    public partial class ColorCodeBuilder : Form
    {
        // Class data
        MethodInfo m_colorCalcMethodInfo = null;


        /// <summary>
        /// Constructor
        /// </summary>
        public ColorCodeBuilder()
        {
            InitializeComponent();
            LoadLastSavedCode();
        }


        /// <summary>
        /// Computes the color to use for a given (complex) wavefunction value.
        /// </summary>
        public Color CalcColor(float re, float im, float maxAmplitude)
        {
            return (Color) m_colorCalcMethodInfo.Invoke( null, new object[]{re, im, maxAmplitude} );
        }


        /// <summary>
        /// Gets the colo-code that was last compiled and saved.
        /// </summary>
        public string GetLastSavedCode()
        {
            return Properties.Settings.Default.ColorCode;
        }


        /// <summary>
        /// Handler for form-shown events.
        /// </summary>
        private void ColorBuilder_Shown(object sender, EventArgs e)
        {
            LoadLastSavedCode();
        }


        /// <summary>
        /// Handler for click events on the Compile button.
        /// </summary>
        private void Compile_Btn_Click(object sender, EventArgs e)
        {
            // Try to compile the current code
            string errorMessages;
            Assembly assembly = CompileCode( AddBoilerplateCode(Code_TextBox.Text), out errorMessages );

            // Check for compilation errors
            if ( assembly == null )
            {
                string msg = string.IsNullOrEmpty(errorMessages) ? "Unknown error." : errorMessages;
                new NonModalMessageBox(msg, "Compiler Errors").Show(this);
            }
            else
            {
                MessageBox.Show(this, "Code successfully compiled.", "Compiler Result");
            }
        }


        /// <summary>
        /// Handler for click events on the Accept button.
        /// </summary>
        private void Accept_Btn_Click(object sender, EventArgs e)
        {
            // Try to compile the current code
            string errorMessages;
            Assembly assembly = CompileCode( AddBoilerplateCode(Code_TextBox.Text), out errorMessages );

            // Check for compilation errors
            if ( assembly == null )
            {
                string msg = string.IsNullOrEmpty(errorMessages) ? "Unknown error." : errorMessages;
                new NonModalMessageBox(msg, "Compiler Errors").Show(this);
                return;
            }
            else
            {
                // Accept the current code and close this dialog
                Properties.Settings.Default.ColorCode = Code_TextBox.Text;
                Properties.Settings.Default.Save();
                m_colorCalcMethodInfo = assembly.GetTypes()[0].GetMethod("GetColor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                DialogResult = DialogResult.OK;
                this.Close();
            }

        }


        /// <summary>
        /// Loads the last-saved code.
        /// </summary>
        private void LoadLastSavedCode()
        {
            // Populate the code area with the last-saved code
            Code_TextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.ColorCode) ? DefaultSnippet : Properties.Settings.Default.ColorCode;

            // Try to compile the code and extract the V method
            string errorMessages;
            Assembly assembly = CompileCode(AddBoilerplateCode(Code_TextBox.Text), out errorMessages);
            if (assembly == null)
            {
                Code_TextBox.Text = DefaultSnippet;
                Properties.Settings.Default.ColorCode = DefaultSnippet;
                Properties.Settings.Default.Save();
                assembly = CompileCode(AddBoilerplateCode(Code_TextBox.Text), out errorMessages);
            }
            m_colorCalcMethodInfo = assembly.GetTypes()[0].GetMethod("GetColor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        }

        
        /// <summary>
        /// Tries to compile a given code string.
        /// </summary>
        private Assembly CompileCode(string code, out string errorMessages)
        {
            // Try to compile
            CompilerParameters parms = new CompilerParameters();
            parms.GenerateInMemory = true;
            parms.TreatWarningsAsErrors = false;
            parms.ReferencedAssemblies.Add("System.Drawing.dll");
            CompilerResults cr = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parms, code);

            // Check for errors
            errorMessages = "";
            if (cr.Errors.Count > 0)
            {
                for (int i=0; i<cr.Errors.Count; i++)
                {
                    CompilerError err = cr.Errors[i];
                    if (i > 0) { errorMessages += Environment.NewLine + Environment.NewLine; }
                    errorMessages += err.ErrorNumber + ":   " + err.ErrorText + "  (line " + (err.Line-7).ToString() + ", column " + err.Column.ToString() + ")";
                }
            }

            return (cr.Errors.HasErrors) ? null : cr.CompiledAssembly;
        }


        /// <summary>
        /// Handler for the Default Snippet menu item
        /// </summary>
        private void DefaultMenuItem_Click(object sender, EventArgs e)
        {
            Code_TextBox.Text = DefaultSnippet;
        }


        /// <summary>
        /// Handler for the Phase Wheel Snippet menu item
        /// </summary>
        private void PhaseWheelMenuItem_Click(object sender, EventArgs e)
        {
             Code_TextBox.Text = PhaseWheelSnippet;
        }


        /// <summary>
        /// Handler for the White Stripes Snippet menu item
        /// </summary>
        private void WhiteStripesMenuItem_Click(object sender, EventArgs e)
        {
             Code_TextBox.Text = WhiteStripesSnippet;
        }

        
        /// <summary>
        /// Adds nalespace and class wrapper around the V-function code.
        /// </summary>
        private string AddBoilerplateCode(string coreCode)
        {
            string result = 
                "using System;                                                           \n" +
                "using System.Drawing;                                                   \n" +
                "using System.Runtime.InteropServices;                                   \n" +
                "                                                                        \n" +
                "                                                                        \n" +
                "namespace TdseSolver_2D1P                                               \n" +
                "{                                                                       \n" +
                "    public static class ColorCalculator                                 \n" +
                "    {                                                                   \n" +
                "        [DllImport(\"shlwapi.dll\")]                                    \n" +
                "        private static extern int ColorHLSToRGB(int H, int L, int S);   \n" +
                "                                                                        \n" +
                "        /// <summary>                                                   \n" +
                "        /// Converts a HSL color value to an RGB color value.           \n" +
                "        /// H,S,L should all be in the range 0-240.                     \n" +
                "        /// </summary>                                                  \n" +
                "        public static Color HSLToRGB(int H, int S, int L)               \n" +
                "        {                                                               \n" +
                "           return ColorTranslator.FromWin32( ColorHLSToRGB(H,L,S) );    \n" +
                "        }                                                               \n";           

            result += coreCode + "} }";

            return result;
        }


        /// <summary>
        /// Gets the default code snippet (which just returns a constant color).
        /// </summary>
        private static string DefaultSnippet
        {
            get
            {
                return
                    "                                                                                       \n" +
                    "/////////////////////////////////////////////////////////////////////////////////////  \n" +
                    "//                                                                                     \n" +
                    "// This method calculates the color to use for a given (complex) wavefunction value.   \n" +
                    "//                                                                                     \n" +
                    "/////////////////////////////////////////////////////////////////////////////////////  \n" +
                    "                                                                                       \n" +
                    "public static Color GetColor(float re, float im, float maxAmplitude)                   \n" +
                    "{                                                                                      \n" +
                    "    return Color.Blue;                                                                 \n" +
                    "}                                                                                      \n";
            }
        }


        /// <summary>
        /// Gets a code snippet that returns a color based on the phase of the wavefunction.
        /// </summary>
        private static string PhaseWheelSnippet
        {
            get
            {
                return 
                    "\n" +
                    "///////////////////////////////////////////////////////////////////////////////////// \n" +
                    "//                                                                                    \n" +
                    "// This method calculates the color to use for a given (complex) wavefunction value.  \n" +
                    "//                                                                                    \n" +
                    "///////////////////////////////////////////////////////////////////////////////////// \n" +
                    "                                                                                      \n" +
                    "public static Color GetColor(float re, float im, float maxAmplitude)                  \n" +
                    "{                                                                                     \n" +
                    "    float ampl = (float) Math.Sqrt(re*re + im*im);                                    \n" +
                    "    double phase = Math.Atan2(im,re);                                                 \n" +
                    "                                                                                      \n" +
                    "    int hue = Convert.ToInt32( ((phase/Math.PI + 1)/2) *240 );                        \n" +
                    "    int sat = 240;                                                                    \n" +
                    "    int lum = Convert.ToInt32( (ampl/maxAmplitude)*120 );                             \n" +
                    "                                                                                      \n" +         
                    "    return HSLToRGB(hue, sat, lum);                                                   \n" +
                    "}";
            }
        }


        /// <summary>
        /// Gets a code snippet that returns a color based on the phase of the wavefunction.
        /// </summary>
        private static string WhiteStripesSnippet
        {
            get
            {
                return 
                    "\n" +
                    "///////////////////////////////////////////////////////////////////////////////////// \n" +
                    "//                                                                                    \n" +
                    "// This method calculates the color to use for a given (complex) wavefunction value.  \n" +
                    "//                                                                                    \n" +
                    "///////////////////////////////////////////////////////////////////////////////////// \n" +
                    "                                                                                      \n" +
                    "public static Color GetColor(float re, float im, float maxAmplitude)                  \n" +
                    "{                                                                                     \n" +
                    "    float ampl = (float) Math.Sqrt(re*re + im*im);                                    \n" +
                    "    double phase = Math.Atan2(im,re);                                                 \n" +
                    "                                                                                      \n" +
                    "    int hue = 20;                                                                     \n" +
                    "    int sat = 240;                                                                    \n" +
                    "    double cos = Math.Cos(phase/2);                                                   \n" +
                    "    int lum = Convert.ToInt32( 120 + 120*(ampl/maxAmplitude)*Math.Pow(cos,6) );       \n" +
                    "                                                                                      \n" +         
                    "    return HSLToRGB(hue, sat, lum);                                                   \n" +
                    "}";
            }
        }

    }

   
}
