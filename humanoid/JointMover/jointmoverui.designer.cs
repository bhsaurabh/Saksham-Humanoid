namespace ProMRDS.Simulation.JointMover
{
    partial class SimulatedBipedMoverUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.walk = new System.Windows.Forms.Button();
            this.front_stand = new System.Windows.Forms.Button();
            this.Mirror = new System.Windows.Forms.Button();
            this.Reset = new System.Windows.Forms.Button();
            this.Stop = new System.Windows.Forms.Button();
            this.Start = new System.Windows.Forms.Button();
            this._entityGroup = new System.Windows.Forms.GroupBox();
            this._entityNameComboBox = new System.Windows.Forms.ComboBox();
            this.SerialPortControllerThread = new System.ComponentModel.BackgroundWorker();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this._entityGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.walk);
            this.splitContainer1.Panel1.Controls.Add(this.front_stand);
            this.splitContainer1.Panel1.Controls.Add(this.Mirror);
            this.splitContainer1.Panel1.Controls.Add(this.Reset);
            this.splitContainer1.Panel1.Controls.Add(this.Stop);
            this.splitContainer1.Panel1.Controls.Add(this.Start);
            this.splitContainer1.Panel1.Controls.Add(this._entityGroup);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Size = new System.Drawing.Size(587, 762);
            this.splitContainer1.SplitterDistance = 146;
            this.splitContainer1.TabIndex = 2;
            // 
            // walk
            // 
            this.walk.Location = new System.Drawing.Point(15, 218);
            this.walk.Name = "walk";
            this.walk.Size = new System.Drawing.Size(115, 24);
            this.walk.TabIndex = 7;
            this.walk.Text = "Walk";
            this.walk.UseVisualStyleBackColor = true;
            this.walk.Click += new System.EventHandler(this.walk_Click);
            // 
            // front_stand
            // 
            this.front_stand.Location = new System.Drawing.Point(15, 189);
            this.front_stand.Name = "front_stand";
            this.front_stand.Size = new System.Drawing.Size(115, 23);
            this.front_stand.TabIndex = 3;
            this.front_stand.Text = "Front Stand";
            this.front_stand.UseVisualStyleBackColor = true;
            this.front_stand.Click += new System.EventHandler(this.front_stand_Click);
            // 
            // Mirror
            // 
            this.Mirror.Location = new System.Drawing.Point(15, 160);
            this.Mirror.Name = "Mirror";
            this.Mirror.Size = new System.Drawing.Size(115, 23);
            this.Mirror.TabIndex = 3;
            this.Mirror.Text = "Mirror";
            this.Mirror.UseVisualStyleBackColor = true;
            this.Mirror.Click += new System.EventHandler(this.Mirror_Click);
            // 
            // Reset
            // 
            this.Reset.Enabled = false;
            this.Reset.Location = new System.Drawing.Point(15, 131);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(115, 23);
            this.Reset.TabIndex = 6;
            this.Reset.Text = "Reset";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // Stop
            // 
            this.Stop.Enabled = false;
            this.Stop.Location = new System.Drawing.Point(15, 102);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(115, 23);
            this.Stop.TabIndex = 5;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(15, 73);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(115, 23);
            this.Start.TabIndex = 0;
            this.Start.Text = "StartComPort";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // _entityGroup
            // 
            this._entityGroup.Controls.Add(this._entityNameComboBox);
            this._entityGroup.Location = new System.Drawing.Point(12, 12);
            this._entityGroup.Name = "_entityGroup";
            this._entityGroup.Size = new System.Drawing.Size(124, 55);
            this._entityGroup.TabIndex = 4;
            this._entityGroup.TabStop = false;
            this._entityGroup.Text = "Entity Name";
            // 
            // _entityNameComboBox
            // 
            this._entityNameComboBox.FormattingEnabled = true;
            this._entityNameComboBox.Location = new System.Drawing.Point(3, 19);
            this._entityNameComboBox.Name = "_entityNameComboBox";
            this._entityNameComboBox.Size = new System.Drawing.Size(115, 21);
            this._entityNameComboBox.TabIndex = 6;
            this._entityNameComboBox.Text = "Select an Entity";
            this._entityNameComboBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this._entityNameComboBox_MouseClick);
            this._entityNameComboBox.SelectionChangeCommitted += new System.EventHandler(this._entityNameComboBox_SelectionChangeCommitted);
            this._entityNameComboBox.SelectedIndexChanged += new System.EventHandler(this._entityNameComboBox_SelectedIndexChanged);
            this._entityNameComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._entityNameComboBox_KeyPress);
            this._entityNameComboBox.SelectedValueChanged += new System.EventHandler(this._entityNameComboBox_SelectedValueChanged);
            // 
            // SerialPortControllerThread
            // 
            this.SerialPortControllerThread.WorkerReportsProgress = true;
            this.SerialPortControllerThread.DoWork += new System.ComponentModel.DoWorkEventHandler(this.SerialPortControllerThread_DoWork);
            // 
            // SimulatedBipedMoverUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(587, 762);
            this.Controls.Add(this.splitContainer1);
            this.Location = new System.Drawing.Point(603, 0);
            this.Name = "SimulatedBipedMoverUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Joint Mover";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this._entityGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox _entityGroup;
        private System.Windows.Forms.ComboBox _entityNameComboBox;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button Mirror;
        private System.Windows.Forms.Button front_stand;
        private System.Windows.Forms.Button walk;
        private System.ComponentModel.BackgroundWorker SerialPortControllerThread;

    }
}