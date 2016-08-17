namespace TdseSolver_3D2P
{
    partial class VCodeBuilder
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Code_TextBox = new System.Windows.Forms.RichTextBox();
            this.Compile_Btn = new System.Windows.Forms.Button();
            this.Accept_Btn = new System.Windows.Forms.Button();
            this.Cancel_Btn = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.snippetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sphereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Code_TextBox
            // 
            this.Code_TextBox.AcceptsTab = true;
            this.Code_TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Code_TextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Code_TextBox.Location = new System.Drawing.Point(23, 49);
            this.Code_TextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Code_TextBox.Name = "Code_TextBox";
            this.Code_TextBox.Size = new System.Drawing.Size(901, 577);
            this.Code_TextBox.TabIndex = 0;
            this.Code_TextBox.Text = "";
            // 
            // Compile_Btn
            // 
            this.Compile_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Compile_Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Compile_Btn.Location = new System.Drawing.Point(23, 649);
            this.Compile_Btn.Name = "Compile_Btn";
            this.Compile_Btn.Size = new System.Drawing.Size(95, 39);
            this.Compile_Btn.TabIndex = 1;
            this.Compile_Btn.Text = "Compile";
            this.Compile_Btn.UseVisualStyleBackColor = true;
            this.Compile_Btn.Click += new System.EventHandler(this.Compile_Btn_Click);
            // 
            // Accept_Btn
            // 
            this.Accept_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Accept_Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Accept_Btn.Location = new System.Drawing.Point(693, 649);
            this.Accept_Btn.Name = "Accept_Btn";
            this.Accept_Btn.Size = new System.Drawing.Size(95, 39);
            this.Accept_Btn.TabIndex = 1;
            this.Accept_Btn.Text = "Accept";
            this.Accept_Btn.UseVisualStyleBackColor = true;
            this.Accept_Btn.Click += new System.EventHandler(this.Accept_Btn_Click);
            // 
            // Cancel_Btn
            // 
            this.Cancel_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel_Btn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Cancel_Btn.Location = new System.Drawing.Point(829, 649);
            this.Cancel_Btn.Name = "Cancel_Btn";
            this.Cancel_Btn.Size = new System.Drawing.Size(95, 39);
            this.Cancel_Btn.TabIndex = 1;
            this.Cancel_Btn.Text = "Cancel";
            this.Cancel_Btn.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.snippetsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(954, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // snippetsToolStripMenuItem
            // 
            this.snippetsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultToolStripMenuItem,
            this.sphereToolStripMenuItem});
            this.snippetsToolStripMenuItem.Name = "snippetsToolStripMenuItem";
            this.snippetsToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.snippetsToolStripMenuItem.Text = "Snippets";
            // 
            // defaultToolStripMenuItem
            // 
            this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
            this.defaultToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.defaultToolStripMenuItem.Text = "Default";
            this.defaultToolStripMenuItem.Click += new System.EventHandler(this.DefaultMenuItem_Click);
            // 
            // sphereToolStripMenuItem
            // 
            this.sphereToolStripMenuItem.Name = "sphereToolStripMenuItem";
            this.sphereToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.sphereToolStripMenuItem.Text = "Sphere";
            this.sphereToolStripMenuItem.Click += new System.EventHandler(this.SphereMenuItem_Click);
            // 
            // VCodeBuilder
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.Cancel_Btn;
            this.ClientSize = new System.Drawing.Size(954, 712);
            this.Controls.Add(this.Cancel_Btn);
            this.Controls.Add(this.Accept_Btn);
            this.Controls.Add(this.Compile_Btn);
            this.Controls.Add(this.Code_TextBox);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "VCodeBuilder";
            this.Text = "V Builder";
            this.Shown += new System.EventHandler(this.VBuilder_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox Code_TextBox;
        private System.Windows.Forms.Button Compile_Btn;
        private System.Windows.Forms.Button Accept_Btn;
        private System.Windows.Forms.Button Cancel_Btn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem snippetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sphereToolStripMenuItem;
    }
}
