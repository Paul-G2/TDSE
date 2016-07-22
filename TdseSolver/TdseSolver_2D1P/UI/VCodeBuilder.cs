using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Windows.Forms;


namespace TdseSolver_2D1P
{
    /// <summary>
    /// This is the UI for entering custom code that computes the potential energy term in the Schrodinger equation.
    /// </summary>
    public partial class VCodeBuilder : Form
    {
        // Class data
        MethodInfo m_vCalcMethodInfo = null;


        /// <summary>
        /// Constructor
        /// </summary>
        public VCodeBuilder()
        {
            InitializeComponent();
            LoadLastSavedCode();
        }


        /// <summary>
        /// Computes the potential energy at a given location and time, using the function defined in the current code snippet.
        /// </summary>
        public float V(float x, float y, float t, float mass, float domainSizeX, float domainSizeY)
        {
            return (float) m_vCalcMethodInfo.Invoke( null, new object[]{x, y, t, mass, domainSizeX, domainSizeY} );
        }


        /// <summary>
        /// Gets the V-code that was last compiled and saved.
        /// </summary>
        public string GetLastSavedCode()
        {
            return Properties.Settings.Default.VCode;
        }


        /// <summary>
        /// Handler for form-shown events.
        /// </summary>
        private void VBuilder_Shown(object sender, EventArgs e)
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
                new TdseUtils.NonModalMessageBox(msg, "Compiler Errors").Show(this);
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
                new TdseUtils.NonModalMessageBox(msg, "Compiler Errors").Show(this);
                return;
            }
            else
            {
                // Accept the current code and close this dialog
                Properties.Settings.Default.VCode = Code_TextBox.Text;
                Properties.Settings.Default.Save();
                m_vCalcMethodInfo = assembly.GetTypes()[0].GetMethod("V", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                this.Close();
            }

        }


        /// <summary>
        /// Loads the last-saved code.
        /// </summary>
        private void LoadLastSavedCode()
        {
            // Populate the code area with the last-saved code
            Code_TextBox.Text = string.IsNullOrEmpty(Properties.Settings.Default.VCode) ? DefaultSnippet : Properties.Settings.Default.VCode;

            // Try to compile the code and extract the V method
            string errorMessages;
            Assembly assembly = CompileCode(AddBoilerplateCode(Code_TextBox.Text), out errorMessages);
            if (assembly == null)
            {
                Code_TextBox.Text = DefaultSnippet;
                Properties.Settings.Default.VCode = DefaultSnippet;
                Properties.Settings.Default.Save();
                assembly = CompileCode(AddBoilerplateCode(Code_TextBox.Text), out errorMessages);
            }
            m_vCalcMethodInfo = assembly.GetTypes()[0].GetMethod("V", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
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
        /// Handler for the Default snippet menu item
        /// </summary>
        private void DefaultMenuItem_Click(object sender, EventArgs e)
        {
            Code_TextBox.Text = DefaultSnippet;
        }


        /// <summary>
        /// Handler for the Cylinder snippet menu item
        /// </summary>
        private void CylinderMenuItem_Click(object sender, EventArgs e)
        {
             Code_TextBox.Text = CylinderSnippet;
        }


        /// <summary>
        /// Handler for the DoubleSlit snippet menu item
        /// </summary>
        private void DoubleSlitMenuItem_Click(object sender, EventArgs e)
        {
             Code_TextBox.Text = DoubleSlitSnippet;
        }

        
        /// <summary>
        /// Adds nalespace and class wrapper around the V-function code.
        /// </summary>
        private string AddBoilerplateCode(string coreCode)
        {
            string result = 
                "using System;                        \n" +
                "                                     \n" +
                "                                     \n" +
                "namespace TdseSolver_2D1P            \n" +
                "{                                    \n" +
                "    public static class VCalc        \n" +
                "    {                                \n";

            result += coreCode + "}}";

            return result;
        }


        /// <summary>
        /// Gets the default code snippet (which implements a function that just returns zero).
        /// </summary>
        private static string DefaultSnippet
        {
            get
            {
                return
                    "                                                                                                    \n" +
                    "/////////////////////////////////////////////////////////////////////////////                       \n" +
                    "//                                                                                                  \n" +
                    "// This method calculates the potential energy at a given location and time.                        \n" +
                    "//                                                                                                  \n" +
                    "/////////////////////////////////////////////////////////////////////////////                       \n" +
                    "                                                                                                    \n" +
                    "public static float V(float x, float y, float t, float mass, float domainSizeX, float domainSizeY)  \n" +
                    "{                                                                                                   \n" +
                    "    return 0.0f;                                                                                    \n" +
                    "}                                                                                                   \n";
            }
        }


        /// <summary>
        /// Gets a code snippet that implements a cylindrical potential barrier.
        /// </summary>
        private static string CylinderSnippet
        {
            get
            {
                return 
                    "                                                                                                    \n" +
                    "/////////////////////////////////////////////////////////////////////////////                       \n" +
                    "//                                                                                                  \n" +
                    "// This method calculates the potential energy at a given location and time.                        \n" +
                    "//                                                                                                  \n" +
                    "/////////////////////////////////////////////////////////////////////////////                       \n" +
                    "                                                                                                    \n" +
                    "public static float V(float x, float y, float t, float mass, float domainSizeX, float domainSizeY)  \n" +
                    "{                                                                                                   \n" +
                    "    double R = domainSizeX/25;                                                                      \n" +
                    "                                                                                                    \n" +
                    "    double dx = x - domainSizeX/2;                                                                  \n" +
                    "    double dy = y - domainSizeY/2;                                                                  \n" +
                    "    double dist = Math.Sqrt(dx*dx + dy*dy);                                                         \n" +
                    "                                                                                                    \n" +
                    "    return (dist > R) ? 0.0f : 2.0f;                                                                \n" +
                    "}                                                                                                   \n";
            }
        }


        /// <summary>
        /// Gets a code snippet that implements a double-slit potential barrier.
        /// </summary>
        private static string DoubleSlitSnippet
        {
            get
            {
                return 
                    "                                                                                                    \n" +
                    "/////////////////////////////////////////////////////////////////////////////                       \n" +
                    "//                                                                                                  \n" +
                    "// This method calculates the potential energy at a given location and time.                        \n" +
                    "//                                                                                                  \n" +
                    "/////////////////////////////////////////////////////////////////////////////                       \n" +
                    "                                                                                                    \n" +
                    "public static float V(float x, float y, float t, float mass, float domainSizeX, float domainSizeY)  \n" +
                    "{                                                                                                   \n" +
                    "    double WallOffset     = domainSizeX;                                                            \n" +
                    "    double WallThickness  = domainSizeX/50;                                                         \n" +
                    "    double HoleWidth      = domainSizeX/50;                                                         \n" +
                    "    double HoleSeparation = domainSizeX/10;                                                         \n" +
                    "                                                                                                    \n" +
                    "    float result = 0.0f;                                                                            \n" +
                    "    if ( Math.Abs(x + y - WallOffset) < WallThickness/Math.Sqrt(2.0) )                              \n" +
                    "    {                                                                                               \n" +
                    "        double d = Math.Abs(x-y);                                                                   \n" +
                    "        if ( (d > HoleSeparation + HoleWidth/2) || (d < HoleSeparation-HoleWidth/2) )               \n" +
                    "        {                                                                                           \n" +
                    "            result = 2.0f;                                                                          \n" +
                    "        }                                                                                           \n" +
                    "    }                                                                                               \n" +
                    "                                                                                                    \n" +
                    "    return result;                                                                                  \n" +
                    "}                                                                                                   \n";
            }
        }

    }






   
}
