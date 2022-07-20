namespace MissionPlanner.GCSViews
{
    partial class NetworkSettingscs
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetworkSettingscs));
            this.CancelButton = new System.Windows.Forms.Button();
            this.OkButton = new System.Windows.Forms.Button();
            this.StreamPortLabel = new System.Windows.Forms.Label();
            this.StreamPortTextBox = new System.Windows.Forms.TextBox();
            this.StreamIPLabel = new System.Windows.Forms.Label();
            this.StreamIPAddressControl = new IPAddressControlLib.IPAddressControl();
            this.VideoForwardIpAddressControl = new IPAddressControlLib.IPAddressControl();
            this.labelVideoForwardingPort = new System.Windows.Forms.Label();
            this.textBoxVideoForwardPort = new System.Windows.Forms.TextBox();
            this.labelVideoForwardingIP = new System.Windows.Forms.Label();
            this.checkBoxEnableVideoForwarding = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // CancelButton
            // 
            resources.ApplyResources(this.CancelButton, "CancelButton");
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OkButton
            // 
            resources.ApplyResources(this.OkButton, "OkButton");
            this.OkButton.Name = "OkButton";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // StreamPortLabel
            // 
            resources.ApplyResources(this.StreamPortLabel, "StreamPortLabel");
            this.StreamPortLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.StreamPortLabel.Name = "StreamPortLabel";
            // 
            // StreamPortTextBox
            // 
            this.StreamPortTextBox.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.StreamPortTextBox, "StreamPortTextBox");
            this.StreamPortTextBox.Name = "StreamPortTextBox";
            // 
            // StreamIPLabel
            // 
            resources.ApplyResources(this.StreamIPLabel, "StreamIPLabel");
            this.StreamIPLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.StreamIPLabel.Name = "StreamIPLabel";
            // 
            // StreamIPAddressControl
            // 
            this.StreamIPAddressControl.AllowInternalTab = false;
            this.StreamIPAddressControl.AutoHeight = true;
            this.StreamIPAddressControl.BackColor = System.Drawing.Color.White;
            this.StreamIPAddressControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.StreamIPAddressControl.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.StreamIPAddressControl, "StreamIPAddressControl");
            this.StreamIPAddressControl.Name = "StreamIPAddressControl";
            this.StreamIPAddressControl.ReadOnly = false;
            // 
            // VideoForwardIpAddressControl
            // 
            this.VideoForwardIpAddressControl.AllowInternalTab = false;
            this.VideoForwardIpAddressControl.AutoHeight = true;
            this.VideoForwardIpAddressControl.BackColor = System.Drawing.Color.White;
            this.VideoForwardIpAddressControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.VideoForwardIpAddressControl.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.VideoForwardIpAddressControl, "VideoForwardIpAddressControl");
            this.VideoForwardIpAddressControl.Name = "VideoForwardIpAddressControl";
            this.VideoForwardIpAddressControl.ReadOnly = false;
            // 
            // labelVideoForwardingPort
            // 
            resources.ApplyResources(this.labelVideoForwardingPort, "labelVideoForwardingPort");
            this.labelVideoForwardingPort.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelVideoForwardingPort.Name = "labelVideoForwardingPort";
            // 
            // textBoxVideoForwardPort
            // 
            this.textBoxVideoForwardPort.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.textBoxVideoForwardPort, "textBoxVideoForwardPort");
            this.textBoxVideoForwardPort.Name = "textBoxVideoForwardPort";
            // 
            // labelVideoForwardingIP
            // 
            resources.ApplyResources(this.labelVideoForwardingIP, "labelVideoForwardingIP");
            this.labelVideoForwardingIP.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelVideoForwardingIP.Name = "labelVideoForwardingIP";
            // 
            // checkBoxEnableVideoForwarding
            // 
            resources.ApplyResources(this.checkBoxEnableVideoForwarding, "checkBoxEnableVideoForwarding");
            this.checkBoxEnableVideoForwarding.Name = "checkBoxEnableVideoForwarding";
            this.checkBoxEnableVideoForwarding.UseVisualStyleBackColor = true;
            this.checkBoxEnableVideoForwarding.CheckedChanged += new System.EventHandler(this.checkBoxEnableVideoForwarding_CheckedChanged);
            // 
            // NetworkSettingscs
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.checkBoxEnableVideoForwarding);
            this.Controls.Add(this.VideoForwardIpAddressControl);
            this.Controls.Add(this.labelVideoForwardingPort);
            this.Controls.Add(this.textBoxVideoForwardPort);
            this.Controls.Add(this.labelVideoForwardingIP);
            this.Controls.Add(this.StreamIPAddressControl);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.StreamPortLabel);
            this.Controls.Add(this.StreamPortTextBox);
            this.Controls.Add(this.StreamIPLabel);
            this.Name = "NetworkSettingscs";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Label StreamPortLabel;
        private System.Windows.Forms.TextBox StreamPortTextBox;
        private System.Windows.Forms.Label StreamIPLabel;
        private IPAddressControlLib.IPAddressControl StreamIPAddressControl;
        private IPAddressControlLib.IPAddressControl VideoForwardIpAddressControl;
        private System.Windows.Forms.Label labelVideoForwardingPort;
        private System.Windows.Forms.TextBox textBoxVideoForwardPort;
        private System.Windows.Forms.Label labelVideoForwardingIP;
        private System.Windows.Forms.CheckBox checkBoxEnableVideoForwarding;
    }
}