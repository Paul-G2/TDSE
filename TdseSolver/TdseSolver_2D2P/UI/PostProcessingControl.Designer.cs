namespace TdseSolver_2D2P
{
    partial class PostProcessingControl
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
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.Stop_Btn = new System.Windows.Forms.Button();
            this.PauseResume_Btn = new System.Windows.Forms.Button();
            this.Main_ProgressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Crop_Btn = new System.Windows.Forms.Button();
            this.XCrop1_NUD = new System.Windows.Forms.NumericUpDown();
            this.YCrop2_NUD = new System.Windows.Forms.NumericUpDown();
            this.YCrop1_NUD = new System.Windows.Forms.NumericUpDown();
            this.XCrop2_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.InputDir_TextBox = new System.Windows.Forms.TextBox();
            this.SelectFolder_Btn = new System.Windows.Forms.Button();
            this.groupBox10.SuspendLayout();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XCrop1_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YCrop2_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.YCrop1_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.XCrop2_NUD)).BeginInit();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox10
            // 
            this.groupBox10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox10.Controls.Add(this.Stop_Btn);
            this.groupBox10.Controls.Add(this.PauseResume_Btn);
            this.groupBox10.Controls.Add(this.Main_ProgressBar);
            this.groupBox10.Location = new System.Drawing.Point(0, 424);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(528, 110);
            this.groupBox10.TabIndex = 16;
            this.groupBox10.TabStop = false;
            // 
            // Stop_Btn
            // 
            this.Stop_Btn.Enabled = false;
            this.Stop_Btn.Location = new System.Drawing.Point(112, 23);
            this.Stop_Btn.Name = "Stop_Btn";
            this.Stop_Btn.Size = new System.Drawing.Size(105, 36);
            this.Stop_Btn.TabIndex = 0;
            this.Stop_Btn.Text = "Stop";
            this.Stop_Btn.UseVisualStyleBackColor = true;
            this.Stop_Btn.Click += new System.EventHandler(this.Stop_Btn_Click);
            // 
            // PauseResume_Btn
            // 
            this.PauseResume_Btn.Enabled = false;
            this.PauseResume_Btn.Location = new System.Drawing.Point(304, 23);
            this.PauseResume_Btn.Name = "PauseResume_Btn";
            this.PauseResume_Btn.Size = new System.Drawing.Size(97, 36);
            this.PauseResume_Btn.TabIndex = 0;
            this.PauseResume_Btn.Text = "Pause";
            this.PauseResume_Btn.UseVisualStyleBackColor = true;
            this.PauseResume_Btn.Click += new System.EventHandler(this.PauseResume_Btn_Click);
            // 
            // Main_ProgressBar
            // 
            this.Main_ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Main_ProgressBar.Location = new System.Drawing.Point(14, 73);
            this.Main_ProgressBar.Name = "Main_ProgressBar";
            this.Main_ProgressBar.Size = new System.Drawing.Size(498, 23);
            this.Main_ProgressBar.TabIndex = 9;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.label2);
            this.groupBox8.Controls.Add(this.label1);
            this.groupBox8.Controls.Add(this.Crop_Btn);
            this.groupBox8.Controls.Add(this.XCrop1_NUD);
            this.groupBox8.Controls.Add(this.YCrop2_NUD);
            this.groupBox8.Controls.Add(this.YCrop1_NUD);
            this.groupBox8.Controls.Add(this.XCrop2_NUD);
            this.groupBox8.Location = new System.Drawing.Point(0, 124);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(528, 200);
            this.groupBox8.TabIndex = 14;
            this.groupBox8.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(293, 147);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(214, 147);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "X";
            // 
            // Crop_Btn
            // 
            this.Crop_Btn.Location = new System.Drawing.Point(188, 40);
            this.Crop_Btn.Name = "Crop_Btn";
            this.Crop_Btn.Size = new System.Drawing.Size(151, 38);
            this.Crop_Btn.TabIndex = 0;
            this.Crop_Btn.Text = "Crop";
            this.Crop_Btn.UseVisualStyleBackColor = true;
            this.Crop_Btn.Click += new System.EventHandler(this.Crop_Btn_Click);
            // 
            // XCrop1_NUD
            // 
            this.XCrop1_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.XCrop1_NUD.Location = new System.Drawing.Point(198, 91);
            this.XCrop1_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.XCrop1_NUD.Name = "XCrop1_NUD";
            this.XCrop1_NUD.Size = new System.Drawing.Size(47, 20);
            this.XCrop1_NUD.TabIndex = 1;
            this.XCrop1_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.XCrop1_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // YCrop2_NUD
            // 
            this.YCrop2_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.YCrop2_NUD.Location = new System.Drawing.Point(280, 120);
            this.YCrop2_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.YCrop2_NUD.Name = "YCrop2_NUD";
            this.YCrop2_NUD.Size = new System.Drawing.Size(46, 20);
            this.YCrop2_NUD.TabIndex = 1;
            this.YCrop2_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.YCrop2_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // YCrop1_NUD
            // 
            this.YCrop1_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.YCrop1_NUD.Location = new System.Drawing.Point(280, 91);
            this.YCrop1_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.YCrop1_NUD.Name = "YCrop1_NUD";
            this.YCrop1_NUD.Size = new System.Drawing.Size(47, 20);
            this.YCrop1_NUD.TabIndex = 1;
            this.YCrop1_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.YCrop1_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // XCrop2_NUD
            // 
            this.XCrop2_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.XCrop2_NUD.Location = new System.Drawing.Point(198, 120);
            this.XCrop2_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.XCrop2_NUD.Name = "XCrop2_NUD";
            this.XCrop2_NUD.Size = new System.Drawing.Size(47, 20);
            this.XCrop2_NUD.TabIndex = 1;
            this.XCrop2_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.XCrop2_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.InputDir_TextBox);
            this.groupBox7.Controls.Add(this.SelectFolder_Btn);
            this.groupBox7.Location = new System.Drawing.Point(0, 0);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(528, 100);
            this.groupBox7.TabIndex = 13;
            this.groupBox7.TabStop = false;
            // 
            // InputDir_TextBox
            // 
            this.InputDir_TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputDir_TextBox.Location = new System.Drawing.Point(144, 42);
            this.InputDir_TextBox.Name = "InputDir_TextBox";
            this.InputDir_TextBox.Size = new System.Drawing.Size(365, 20);
            this.InputDir_TextBox.TabIndex = 1;
            // 
            // SelectFolder_Btn
            // 
            this.SelectFolder_Btn.Location = new System.Drawing.Point(19, 34);
            this.SelectFolder_Btn.Name = "SelectFolder_Btn";
            this.SelectFolder_Btn.Size = new System.Drawing.Size(110, 38);
            this.SelectFolder_Btn.TabIndex = 0;
            this.SelectFolder_Btn.Text = "Select Folder";
            this.SelectFolder_Btn.UseVisualStyleBackColor = true;
            this.SelectFolder_Btn.Click += new System.EventHandler(this.SelectFolder_Btn_Click);
            // 
            // PostProcessingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.groupBox8);
            this.Controls.Add(this.groupBox7);
            this.Name = "PostProcessingControl";
            this.Size = new System.Drawing.Size(530, 533);
            this.groupBox10.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.XCrop1_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YCrop2_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.YCrop1_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.XCrop2_NUD)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button Stop_Btn;
        private System.Windows.Forms.Button PauseResume_Btn;
        private System.Windows.Forms.ProgressBar Main_ProgressBar;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.TextBox InputDir_TextBox;
        private System.Windows.Forms.Button SelectFolder_Btn;
        private System.Windows.Forms.Button Crop_Btn;
        private System.Windows.Forms.NumericUpDown XCrop1_NUD;
        private System.Windows.Forms.NumericUpDown XCrop2_NUD;
        private System.Windows.Forms.NumericUpDown YCrop2_NUD;
        private System.Windows.Forms.NumericUpDown YCrop1_NUD;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
