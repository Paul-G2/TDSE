using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Windows.Forms;


namespace TdseSolver_2D2P_NS
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
        /// Handler for form-shown events.
        /// </summary>
        private void VBuilder_Shown(object sender, EventArgs e)
        {
            LoadLastSavedCode();
        }

        
        /// <summary>
        /// Loads the last-saved code.
        /// </summary>
        private void LoadLastSavedCode()
        {
            string errorMessages = SetCode( RunParams.FromString(Properties.Settings.Default.LastRunParams).VCode );

            if ( !string.IsNullOrEmpty(errorMessages) )
            {
                SetCode( DefaultSnippet );
            }
        }
        
        

        /// <summary>
        /// Sets and saves the V-code.
        /// </summary>
        public string SetCode(string code)
        {
            // Try to compile the given code
            string errorMessages;
            Assembly assembly = CompileCode( AddBoilerplateCode(code), out errorMessages );

            // Check for compilation errors
            if ( assembly == null )
            {
                return string.IsNullOrEmpty(errorMessages) ? "Unknown compilation error." : errorMessages;
            }
            else
            {
                // Accept the given code
                Code_TextBox.Text = code;
                m_vCalcMethodInfo = assembly.GetTypes()[0].GetMethod("V", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                RunParams parms = RunParams.FromString(Properties.Settings.Default.LastRunParams);
                parms.VCode = code;
                Properties.Settings.Default.LastRunParams = parms.ToString();
                Properties.Settings.Default.Save();
                return "";
            }
        }
        
        
        /// <summary>
        /// Computes the potential energy at a given location and time, using the function defined in the current code snippet.
        /// </summary>
        public float V(float x, float y, float domainSizeX, float domainSizeY)
        {
            return (float) m_vCalcMethodInfo.Invoke( null, new object[]{x, y, domainSizeX, domainSizeY} );
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
            string errorMessages = SetCode(Code_TextBox.Text);

            if ( string.IsNullOrEmpty(errorMessages) )
            {
                this.Close();
            }
            else
            {
                new TdseUtils.NonModalMessageBox(errorMessages, "Compiler Errors").Show(this);
                return;
            }
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
            if ( cr.Errors.HasErrors || (cr.CompiledAssembly == null) )
            {
                return null;
            }


            // Check that the V method had been defined
            MethodInfo vMethodInfo = cr.CompiledAssembly.GetTypes()[0].GetMethod("V", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (vMethodInfo == null)
            {
                errorMessages = "Expected V method not found.";
                return null;
            }
            else
            {
                return cr.CompiledAssembly;
            }
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
        /// Adds nalespace and class wrapper around the V-function code.
        /// </summary>
        private string AddBoilerplateCode(string coreCode)
        {
            string result = 
                "using System;                        \n" +
                "                                     \n" +
                "                                     \n" +
                "namespace TdseSolver_2D2P_NS         \n" +
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
                    "                                                                                   \n" +
                    "/////////////////////////////////////////////////////////////////////////////      \n" +
                    "//                                                                                 \n" +
                    "// This method calculates the potential energy at a given r1 - r2.                 \n" +
                    "//                                                                                 \n" +
                    "/////////////////////////////////////////////////////////////////////////////      \n" +
                    "                                                                                   \n" +
                    "public static float V(float x12, float y12, float domainSizeX, float domainSizeY)  \n" +
                    "{                                                                                  \n" +
                    "    return 0.0f;                                                                   \n" +
                    "}                                                                                  \n";
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
                    "                                                                                   \n" +
                    "/////////////////////////////////////////////////////////////////////////////      \n" +
                    "//                                                                                 \n" +
                    "// This method calculates the potential energy at a given r1 - r2.                 \n" +
                    "//                                                                                 \n" +
                    "/////////////////////////////////////////////////////////////////////////////      \n" +
                    "                                                                                   \n" +
                    "public static float V(float x12, float y12, float domainSizeX, float domainSizeY)  \n" +
                    "{                                                                                  \n" +
                    "    double Rsq = 10*10;                                                            \n" +
                    "                                                                                   \n" +
                    "    double distSq = (x12*x12 + y12*y12);                                           \n" +
                    "                                                                                   \n" +
                    "    return (distSq > Rsq) ? 0.0f : 1.0f;                                           \n" +
                    "}                                                                                  \n";
            }
        }

    }






   
}
