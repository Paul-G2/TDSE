namespace TdseSolver_2D1P
{
    partial class NonModalMessageBox
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Msg_Label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Msg_Label
            // 
            this.Msg_Label.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Msg_Label.AutoEllipsis = true;
            this.Msg_Label.Location = new System.Drawing.Point(25, 28);
            this.Msg_Label.Name = "Msg_Label";
            this.Msg_Label.Size = new System.Drawing.Size(466, 72);
            this.Msg_Label.TabIndex = 0;
            this.Msg_Label.Text = "message goes here";
            this.Msg_Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NonModalMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 135);
            this.Controls.Add(this.Msg_Label);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NonModalMessageBox";
            this.ShowIcon = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label Msg_Label;
    }
}