namespace TdseSolver_2D1P
{
    partial class SolverControl
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
            this.V_Btn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LatticeSpacing_NUD = new System.Windows.Forms.NumericUpDown();
            this.GridSizeY_NUD = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.GridSizeX_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.InitialPacketSizeY_NUD = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.InitialPacketSizeX_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.InitialPacketCenterY_NUD = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.InitialPacketCenterX_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.Mass_NUD = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.InitialMomentumY_NUD = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.InitialMomentumX_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.NumFrames_NUD = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.TotalTime_NUD = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.TimeStep_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.RunStop_Btn = new System.Windows.Forms.Button();
            this.PauseResume_Btn = new System.Windows.Forms.Button();
            this.Main_ProgressBar = new System.Windows.Forms.ProgressBar();
            this.MultiThread_CheckBox = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.DampingBorderWidth_NUD = new System.Windows.Forms.NumericUpDown();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.DampingFactor_NUD = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LatticeSpacing_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridSizeY_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridSizeX_NUD)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketSizeY_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketSizeX_NUD)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketCenterY_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketCenterX_NUD)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Mass_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialMomentumY_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialMomentumX_NUD)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumFrames_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TotalTime_NUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeStep_NUD)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DampingBorderWidth_NUD)).BeginInit();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DampingFactor_NUD)).BeginInit();
            this.SuspendLayout();
            // 
            // V_Btn
            // 
            this.V_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.V_Btn.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.V_Btn.Location = new System.Drawing.Point(386, 317);
            this.V_Btn.Name = "V_Btn";
            this.V_Btn.Size = new System.Drawing.Size(121, 40);
            this.V_Btn.TabIndex = 22;
            this.V_Btn.Text = "Potential Energy ...";
            this.V_Btn.UseVisualStyleBackColor = true;
            this.V_Btn.Click += new System.EventHandler(this.V_Btn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.LatticeSpacing_NUD);
            this.groupBox1.Controls.Add(this.GridSizeY_NUD);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.GridSizeX_NUD);
            this.groupBox1.Location = new System.Drawing.Point(0, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(312, 67);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Grid size";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(192, 29);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(49, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "Spacing:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(100, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y :";
            // 
            // LatticeSpacing_NUD
            // 
            this.LatticeSpacing_NUD.DecimalPlaces = 1;
            this.LatticeSpacing_NUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.LatticeSpacing_NUD.Location = new System.Drawing.Point(243, 26);
            this.LatticeSpacing_NUD.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.LatticeSpacing_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.LatticeSpacing_NUD.Name = "LatticeSpacing_NUD";
            this.LatticeSpacing_NUD.Size = new System.Drawing.Size(56, 20);
            this.LatticeSpacing_NUD.TabIndex = 3;
            this.LatticeSpacing_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LatticeSpacing_NUD.Value = new decimal(new int[] {
            20,
            0,
            0,
            65536});
            // 
            // GridSizeY_NUD
            // 
            this.GridSizeY_NUD.Location = new System.Drawing.Point(121, 26);
            this.GridSizeY_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.GridSizeY_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.GridSizeY_NUD.Name = "GridSizeY_NUD";
            this.GridSizeY_NUD.Size = new System.Drawing.Size(56, 20);
            this.GridSizeY_NUD.TabIndex = 3;
            this.GridSizeY_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.GridSizeY_NUD.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "X :";
            // 
            // GridSizeX_NUD
            // 
            this.GridSizeX_NUD.Location = new System.Drawing.Point(29, 26);
            this.GridSizeX_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.GridSizeX_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.GridSizeX_NUD.Name = "GridSizeX_NUD";
            this.GridSizeX_NUD.Size = new System.Drawing.Size(56, 20);
            this.GridSizeX_NUD.TabIndex = 1;
            this.GridSizeX_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.GridSizeX_NUD.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.InitialPacketSizeY_NUD);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.InitialPacketSizeX_NUD);
            this.groupBox2.Location = new System.Drawing.Point(322, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(202, 67);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Initial wavepacket size";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Y :";
            // 
            // InitialPacketSizeY_NUD
            // 
            this.InitialPacketSizeY_NUD.DecimalPlaces = 1;
            this.InitialPacketSizeY_NUD.Location = new System.Drawing.Point(132, 26);
            this.InitialPacketSizeY_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.InitialPacketSizeY_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.InitialPacketSizeY_NUD.Name = "InitialPacketSizeY_NUD";
            this.InitialPacketSizeY_NUD.Size = new System.Drawing.Size(60, 20);
            this.InitialPacketSizeY_NUD.TabIndex = 3;
            this.InitialPacketSizeY_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialPacketSizeY_NUD.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "X :";
            // 
            // InitialPacketSizeX_NUD
            // 
            this.InitialPacketSizeX_NUD.DecimalPlaces = 1;
            this.InitialPacketSizeX_NUD.Location = new System.Drawing.Point(29, 26);
            this.InitialPacketSizeX_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.InitialPacketSizeX_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.InitialPacketSizeX_NUD.Name = "InitialPacketSizeX_NUD";
            this.InitialPacketSizeX_NUD.Size = new System.Drawing.Size(60, 20);
            this.InitialPacketSizeX_NUD.TabIndex = 1;
            this.InitialPacketSizeX_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialPacketSizeX_NUD.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.InitialPacketCenterY_NUD);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.InitialPacketCenterX_NUD);
            this.groupBox3.Location = new System.Drawing.Point(322, 92);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(201, 67);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Initial wavepacket location";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(110, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Y :";
            // 
            // InitialPacketCenterY_NUD
            // 
            this.InitialPacketCenterY_NUD.DecimalPlaces = 1;
            this.InitialPacketCenterY_NUD.Location = new System.Drawing.Point(132, 27);
            this.InitialPacketCenterY_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.InitialPacketCenterY_NUD.Name = "InitialPacketCenterY_NUD";
            this.InitialPacketCenterY_NUD.Size = new System.Drawing.Size(60, 20);
            this.InitialPacketCenterY_NUD.TabIndex = 3;
            this.InitialPacketCenterY_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialPacketCenterY_NUD.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "X :";
            // 
            // InitialPacketCenterX_NUD
            // 
            this.InitialPacketCenterX_NUD.DecimalPlaces = 1;
            this.InitialPacketCenterX_NUD.Location = new System.Drawing.Point(29, 27);
            this.InitialPacketCenterX_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.InitialPacketCenterX_NUD.Name = "InitialPacketCenterX_NUD";
            this.InitialPacketCenterX_NUD.Size = new System.Drawing.Size(60, 20);
            this.InitialPacketCenterX_NUD.TabIndex = 1;
            this.InitialPacketCenterX_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialPacketCenterX_NUD.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.Mass_NUD);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.InitialMomentumY_NUD);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.InitialMomentumX_NUD);
            this.groupBox4.Location = new System.Drawing.Point(4, 92);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(308, 67);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Initial momentum";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(203, 30);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(35, 13);
            this.label15.TabIndex = 6;
            this.label15.Text = "Mass:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Mass_NUD
            // 
            this.Mass_NUD.DecimalPlaces = 1;
            this.Mass_NUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.Mass_NUD.Location = new System.Drawing.Point(240, 27);
            this.Mass_NUD.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.Mass_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.Mass_NUD.Name = "Mass_NUD";
            this.Mass_NUD.Size = new System.Drawing.Size(56, 20);
            this.Mass_NUD.TabIndex = 5;
            this.Mass_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Mass_NUD.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(96, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Y :";
            // 
            // InitialMomentumY_NUD
            // 
            this.InitialMomentumY_NUD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InitialMomentumY_NUD.DecimalPlaces = 3;
            this.InitialMomentumY_NUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.InitialMomentumY_NUD.Location = new System.Drawing.Point(117, 26);
            this.InitialMomentumY_NUD.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.InitialMomentumY_NUD.Name = "InitialMomentumY_NUD";
            this.InitialMomentumY_NUD.Size = new System.Drawing.Size(65, 20);
            this.InitialMomentumY_NUD.TabIndex = 3;
            this.InitialMomentumY_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialMomentumY_NUD.Value = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 29);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "X :";
            // 
            // InitialMomentumX_NUD
            // 
            this.InitialMomentumX_NUD.DecimalPlaces = 3;
            this.InitialMomentumX_NUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.InitialMomentumX_NUD.Location = new System.Drawing.Point(25, 26);
            this.InitialMomentumX_NUD.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            65536});
            this.InitialMomentumX_NUD.Name = "InitialMomentumX_NUD";
            this.InitialMomentumX_NUD.Size = new System.Drawing.Size(65, 20);
            this.InitialMomentumX_NUD.TabIndex = 1;
            this.InitialMomentumX_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.InitialMomentumX_NUD.Value = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.NumFrames_NUD);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.TotalTime_NUD);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.TimeStep_NUD);
            this.groupBox5.Location = new System.Drawing.Point(0, 183);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(523, 85);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            // 
            // NumFrames_NUD
            // 
            this.NumFrames_NUD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NumFrames_NUD.Location = new System.Drawing.Point(409, 43);
            this.NumFrames_NUD.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.NumFrames_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumFrames_NUD.Name = "NumFrames_NUD";
            this.NumFrames_NUD.Size = new System.Drawing.Size(72, 20);
            this.NumFrames_NUD.TabIndex = 6;
            this.NumFrames_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NumFrames_NUD.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(400, 23);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(93, 13);
            this.label11.TabIndex = 5;
            this.label11.Text = "Number of Frames";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(232, 23);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Total Time";
            // 
            // TotalTime_NUD
            // 
            this.TotalTime_NUD.DecimalPlaces = 1;
            this.TotalTime_NUD.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.TotalTime_NUD.Location = new System.Drawing.Point(217, 43);
            this.TotalTime_NUD.Maximum = new decimal(new int[] {
            900000,
            0,
            0,
            0});
            this.TotalTime_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.TotalTime_NUD.Name = "TotalTime_NUD";
            this.TotalTime_NUD.Size = new System.Drawing.Size(83, 20);
            this.TotalTime_NUD.TabIndex = 4;
            this.TotalTime_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TotalTime_NUD.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(47, 23);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "Time Step";
            // 
            // TimeStep_NUD
            // 
            this.TimeStep_NUD.DecimalPlaces = 3;
            this.TimeStep_NUD.Increment = new decimal(new int[] {
            5,
            0,
            0,
            196608});
            this.TimeStep_NUD.Location = new System.Drawing.Point(37, 43);
            this.TimeStep_NUD.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.TimeStep_NUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.TimeStep_NUD.Name = "TimeStep_NUD";
            this.TimeStep_NUD.Size = new System.Drawing.Size(83, 20);
            this.TimeStep_NUD.TabIndex = 2;
            this.TimeStep_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TimeStep_NUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.RunStop_Btn);
            this.groupBox6.Controls.Add(this.PauseResume_Btn);
            this.groupBox6.Controls.Add(this.Main_ProgressBar);
            this.groupBox6.Controls.Add(this.MultiThread_CheckBox);
            this.groupBox6.Location = new System.Drawing.Point(0, 402);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(523, 131);
            this.groupBox6.TabIndex = 23;
            this.groupBox6.TabStop = false;
            // 
            // RunStop_Btn
            // 
            this.RunStop_Btn.Location = new System.Drawing.Point(112, 21);
            this.RunStop_Btn.Name = "RunStop_Btn";
            this.RunStop_Btn.Size = new System.Drawing.Size(105, 36);
            this.RunStop_Btn.TabIndex = 0;
            this.RunStop_Btn.Text = "Run";
            this.RunStop_Btn.UseVisualStyleBackColor = true;
            this.RunStop_Btn.Click += new System.EventHandler(this.RunStop_Btn_Click);
            // 
            // PauseResume_Btn
            // 
            this.PauseResume_Btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PauseResume_Btn.Enabled = false;
            this.PauseResume_Btn.Location = new System.Drawing.Point(304, 21);
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
            this.Main_ProgressBar.Location = new System.Drawing.Point(14, 94);
            this.Main_ProgressBar.Name = "Main_ProgressBar";
            this.Main_ProgressBar.Size = new System.Drawing.Size(493, 23);
            this.Main_ProgressBar.TabIndex = 9;
            // 
            // MultiThread_CheckBox
            // 
            this.MultiThread_CheckBox.AutoSize = true;
            this.MultiThread_CheckBox.Checked = true;
            this.MultiThread_CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MultiThread_CheckBox.Location = new System.Drawing.Point(128, 62);
            this.MultiThread_CheckBox.Name = "MultiThread_CheckBox";
            this.MultiThread_CheckBox.Size = new System.Drawing.Size(82, 17);
            this.MultiThread_CheckBox.TabIndex = 10;
            this.MultiThread_CheckBox.Text = "MultiThread";
            this.MultiThread_CheckBox.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 37);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(72, 13);
            this.label12.TabIndex = 25;
            this.label12.Text = "Border width :";
            // 
            // DampingBorderWidth_NUD
            // 
            this.DampingBorderWidth_NUD.Location = new System.Drawing.Point(90, 34);
            this.DampingBorderWidth_NUD.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.DampingBorderWidth_NUD.Name = "DampingBorderWidth_NUD";
            this.DampingBorderWidth_NUD.Size = new System.Drawing.Size(56, 20);
            this.DampingBorderWidth_NUD.TabIndex = 24;
            this.DampingBorderWidth_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.DampingFactor_NUD);
            this.groupBox7.Controls.Add(this.DampingBorderWidth_NUD);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Location = new System.Drawing.Point(0, 293);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(300, 78);
            this.groupBox7.TabIndex = 26;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Damping";
            // 
            // DampingFactor_NUD
            // 
            this.DampingFactor_NUD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DampingFactor_NUD.DecimalPlaces = 2;
            this.DampingFactor_NUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.DampingFactor_NUD.Location = new System.Drawing.Point(226, 34);
            this.DampingFactor_NUD.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DampingFactor_NUD.Name = "DampingFactor_NUD";
            this.DampingFactor_NUD.Size = new System.Drawing.Size(59, 20);
            this.DampingFactor_NUD.TabIndex = 26;
            this.DampingFactor_NUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DampingFactor_NUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(181, 38);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(43, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "Factor :";
            // 
            // SolverControl
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.V_Btn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox5);
            this.Name = "SolverControl";
            this.Size = new System.Drawing.Size(524, 533);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LatticeSpacing_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridSizeY_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GridSizeX_NUD)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketSizeY_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketSizeX_NUD)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketCenterY_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialPacketCenterX_NUD)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Mass_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialMomentumY_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InitialMomentumX_NUD)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumFrames_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TotalTime_NUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TimeStep_NUD)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DampingBorderWidth_NUD)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DampingFactor_NUD)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button V_Btn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown GridSizeY_NUD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown GridSizeX_NUD;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown InitialPacketSizeY_NUD;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown InitialPacketSizeX_NUD;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown InitialPacketCenterY_NUD;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown InitialPacketCenterX_NUD;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown InitialMomentumY_NUD;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown InitialMomentumX_NUD;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.NumericUpDown NumFrames_NUD;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown TotalTime_NUD;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown TimeStep_NUD;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button RunStop_Btn;
        private System.Windows.Forms.Button PauseResume_Btn;
        private System.Windows.Forms.ProgressBar Main_ProgressBar;
        private System.Windows.Forms.CheckBox MultiThread_CheckBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown DampingBorderWidth_NUD;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.NumericUpDown DampingFactor_NUD;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown LatticeSpacing_NUD;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown Mass_NUD;
    }
}
