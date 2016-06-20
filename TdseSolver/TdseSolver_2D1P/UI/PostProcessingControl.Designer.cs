namespace TdseSolver_2D1P
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
            this.ReColor_Btn = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.UpsampleFactor_NUD = new System.Windows.Forms.NumericUpDown();
            this.Upsample_Btn = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.InputDir_TextBox = new System.Windows.Forms.TextBox();
            this.SelectFolder_Btn = new System.Windows.Forms.Button();
            this.Crop_Btn = new System.Windows.Forms.Button();
            this.RightCrop_NUD = new System.Windows.Forms.NumericUpDown();
            this.LeftCrop_NUD = new System.Windows.Forms.NumericUpDown();
            this.TopCrop_NUD = new System.Windows.Forms.NumericUpDown();
            this.BottomCrop_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox10.SuspendLayout();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpsampleFactor_NUD)).BeginInit();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RightCrop_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftCrop_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopCrop_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomCrop_NUD)).BeginInit();
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
            // ReColor_Btn
            // 
            this.ReColor_Btn.Location = new System.Drawing.Point(399, 30);
            this.ReColor_Btn.Name = "ReColor_Btn";
            this.ReColor_Btn.Size = new System.Drawing.Size(110, 38);
            this.ReColor_Btn.TabIndex = 0;
            this.ReColor_Btn.Text = "Re-Color ...";
            this.ReColor_Btn.UseVisualStyleBackColor = true;
            this.ReColor_Btn.Click += new System.EventHandler(this.ReColor_Btn_Click);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.Crop_Btn);
            this.groupBox8.Controls.Add(this.ReColor_Btn);
            this.groupBox8.Controls.Add(this.label12);
            this.groupBox8.Controls.Add(this.LeftCrop_NUD);
            this.groupBox8.Controls.Add(this.BottomCrop_NUD);
            this.groupBox8.Controls.Add(this.TopCrop_NUD);
            this.groupBox8.Controls.Add(this.RightCrop_NUD);
            this.groupBox8.Controls.Add(this.UpsampleFactor_NUD);
            this.groupBox8.Controls.Add(this.Upsample_Btn);
            this.groupBox8.Location = new System.Drawing.Point(0, 124);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(528, 200);
            this.groupBox8.TabIndex = 14;
            this.groupBox8.TabStop = false;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 81);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(43, 13);
            this.label12.TabIndex = 2;
            this.label12.Text = "Factor :";
            // 
            // UpsampleFactor_NUD
            // 
            this.UpsampleFactor_NUD.DecimalPlaces = 1;
            this.UpsampleFactor_NUD.Location = new System.Drawing.Point(60, 79);
            this.UpsampleFactor_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.UpsampleFactor_NUD.Name = "UpsampleFactor_NUD";
            this.UpsampleFactor_NUD.Size = new System.Drawing.Size(66, 20);
            this.UpsampleFactor_NUD.TabIndex = 1;
            this.UpsampleFactor_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.UpsampleFactor_NUD.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // Upsample_Btn
            // 
            this.Upsample_Btn.Location = new System.Drawing.Point(19, 30);
            this.Upsample_Btn.Name = "Upsample_Btn";
            this.Upsample_Btn.Size = new System.Drawing.Size(110, 38);
            this.Upsample_Btn.TabIndex = 0;
            this.Upsample_Btn.Text = "Upsample";
            this.Upsample_Btn.UseVisualStyleBackColor = true;
            this.Upsample_Btn.Click += new System.EventHandler(this.Upsample_Btn_Click);
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
            this.InputDir_TextBox.Enabled = false;
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
            // Crop_Btn
            // 
            this.Crop_Btn.Location = new System.Drawing.Point(209, 30);
            this.Crop_Btn.Name = "Crop_Btn";
            this.Crop_Btn.Size = new System.Drawing.Size(114, 38);
            this.Crop_Btn.TabIndex = 0;
            this.Crop_Btn.Text = "Crop";
            this.Crop_Btn.UseVisualStyleBackColor = true;
            this.Crop_Btn.Click += new System.EventHandler(this.Crop_Btn_Click);
            // 
            // RightCrop_NUD
            // 
            this.RightCrop_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.RightCrop_NUD.Location = new System.Drawing.Point(277, 118);
            this.RightCrop_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.RightCrop_NUD.Name = "RightCrop_NUD";
            this.RightCrop_NUD.Size = new System.Drawing.Size(54, 20);
            this.RightCrop_NUD.TabIndex = 1;
            this.RightCrop_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.RightCrop_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // LeftCrop_NUD
            // 
            this.LeftCrop_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.LeftCrop_NUD.Location = new System.Drawing.Point(201, 118);
            this.LeftCrop_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.LeftCrop_NUD.Name = "LeftCrop_NUD";
            this.LeftCrop_NUD.Size = new System.Drawing.Size(54, 20);
            this.LeftCrop_NUD.TabIndex = 1;
            this.LeftCrop_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LeftCrop_NUD.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.LeftCrop_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // TopCrop_NUD
            // 
            this.TopCrop_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.TopCrop_NUD.Location = new System.Drawing.Point(241, 86);
            this.TopCrop_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.TopCrop_NUD.Name = "TopCrop_NUD";
            this.TopCrop_NUD.Size = new System.Drawing.Size(54, 20);
            this.TopCrop_NUD.TabIndex = 1;
            this.TopCrop_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TopCrop_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // BottomCrop_NUD
            // 
            this.BottomCrop_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.BottomCrop_NUD.Location = new System.Drawing.Point(241, 152);
            this.BottomCrop_NUD.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.BottomCrop_NUD.Name = "BottomCrop_NUD";
            this.BottomCrop_NUD.Size = new System.Drawing.Size(54, 20);
            this.BottomCrop_NUD.TabIndex = 1;
            this.BottomCrop_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.BottomCrop_NUD.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
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
            ((System.ComponentModel.ISupportInitialize)(this.UpsampleFactor_NUD)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RightCrop_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftCrop_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopCrop_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomCrop_NUD)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Button Stop_Btn;
        private System.Windows.Forms.Button PauseResume_Btn;
        private System.Windows.Forms.ProgressBar Main_ProgressBar;
        private System.Windows.Forms.Button ReColor_Btn;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown UpsampleFactor_NUD;
        private System.Windows.Forms.Button Upsample_Btn;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.TextBox InputDir_TextBox;
        private System.Windows.Forms.Button SelectFolder_Btn;
        private System.Windows.Forms.Button Crop_Btn;
        private System.Windows.Forms.NumericUpDown LeftCrop_NUD;
        private System.Windows.Forms.NumericUpDown RightCrop_NUD;
        private System.Windows.Forms.NumericUpDown BottomCrop_NUD;
        private System.Windows.Forms.NumericUpDown TopCrop_NUD;
    }
}
