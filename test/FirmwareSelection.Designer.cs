using MissionPlanner.ArduPilot;
using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MissionPlanner.test
{
    /// <summary>
    /// Firmware filter/picker. Ported from a Xamarin.Forms ContentPage to a native WinForms Form so
    /// it runs everywhere the app does (desktop directly, mobile via the Mono WinForms-on-Skia host)
    /// without any Xamarin.Forms dependency. Shown with ShowDialog(); the chosen url is FinalResult
    /// (null if the user closed without picking).
    /// </summary>
    public partial class FirmwareSelection : Form
    {
        private Label lbltype, lblversiontype, lblplatform, lblversion, lblformat, lblusbid, lblboardid, lblbootloaderid, lblfirmware;
        private ComboBox mavtype, versiontype, platform, version, format, USBID, board_id, bootloader_str, Result;
        private Button Button;
        private Label hdr;
        private FlowLayoutPanel panel;

        private void InitializeComponent()
        {
            this.panel = new System.Windows.Forms.FlowLayoutPanel();
            this.hdr = new System.Windows.Forms.Label();
            this.lbltype = new System.Windows.Forms.Label();
            this.mavtype = new System.Windows.Forms.ComboBox();
            this.lblversiontype = new System.Windows.Forms.Label();
            this.versiontype = new System.Windows.Forms.ComboBox();
            this.lblplatform = new System.Windows.Forms.Label();
            this.platform = new System.Windows.Forms.ComboBox();
            this.lblversion = new System.Windows.Forms.Label();
            this.version = new System.Windows.Forms.ComboBox();
            this.lblformat = new System.Windows.Forms.Label();
            this.format = new System.Windows.Forms.ComboBox();
            this.lblusbid = new System.Windows.Forms.Label();
            this.USBID = new System.Windows.Forms.ComboBox();
            this.lblboardid = new System.Windows.Forms.Label();
            this.board_id = new System.Windows.Forms.ComboBox();
            this.lblbootloaderid = new System.Windows.Forms.Label();
            this.bootloader_str = new System.Windows.Forms.ComboBox();
            this.lblfirmware = new System.Windows.Forms.Label();
            this.Result = new System.Windows.Forms.ComboBox();
            this.Button = new System.Windows.Forms.Button();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.AutoScroll = true;
            this.panel.Controls.Add(this.hdr);
            this.panel.Controls.Add(this.lbltype);
            this.panel.Controls.Add(this.mavtype);
            this.panel.Controls.Add(this.lblversiontype);
            this.panel.Controls.Add(this.versiontype);
            this.panel.Controls.Add(this.lblplatform);
            this.panel.Controls.Add(this.platform);
            this.panel.Controls.Add(this.lblversion);
            this.panel.Controls.Add(this.version);
            this.panel.Controls.Add(this.lblformat);
            this.panel.Controls.Add(this.format);
            this.panel.Controls.Add(this.lblusbid);
            this.panel.Controls.Add(this.USBID);
            this.panel.Controls.Add(this.lblboardid);
            this.panel.Controls.Add(this.board_id);
            this.panel.Controls.Add(this.lblbootloaderid);
            this.panel.Controls.Add(this.bootloader_str);
            this.panel.Controls.Add(this.lblfirmware);
            this.panel.Controls.Add(this.Result);
            this.panel.Controls.Add(this.Button);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Padding = new System.Windows.Forms.Padding(10);
            this.panel.Size = new System.Drawing.Size(551, 499);
            this.panel.TabIndex = 0;
            this.panel.WrapContents = false;
            // 
            // hdr
            // 
            this.hdr.AutoSize = true;
            this.hdr.Location = new System.Drawing.Point(13, 10);
            this.hdr.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.hdr.Name = "hdr";
            this.hdr.Size = new System.Drawing.Size(344, 13);
            this.hdr.TabIndex = 0;
            this.hdr.Text = "More than one choice exists. Please filter down to the desired selection.";
            // 
            // lbltype
            // 
            this.lbltype.AutoSize = true;
            this.lbltype.Location = new System.Drawing.Point(13, 35);
            this.lbltype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lbltype.Name = "lbltype";
            this.lbltype.Size = new System.Drawing.Size(31, 13);
            this.lbltype.TabIndex = 1;
            this.lbltype.Text = "Type";
            // 
            // mavtype
            // 
            this.mavtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mavtype.Location = new System.Drawing.Point(13, 48);
            this.mavtype.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.mavtype.Name = "mavtype";
            this.mavtype.Size = new System.Drawing.Size(520, 21);
            this.mavtype.TabIndex = 2;
            this.mavtype.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblversiontype
            // 
            this.lblversiontype.AutoSize = true;
            this.lblversiontype.Location = new System.Drawing.Point(13, 81);
            this.lblversiontype.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblversiontype.Name = "lblversiontype";
            this.lblversiontype.Size = new System.Drawing.Size(69, 13);
            this.lblversiontype.TabIndex = 3;
            this.lblversiontype.Text = "Version Type";
            // 
            // versiontype
            // 
            this.versiontype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.versiontype.Location = new System.Drawing.Point(13, 94);
            this.versiontype.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.versiontype.Name = "versiontype";
            this.versiontype.Size = new System.Drawing.Size(520, 21);
            this.versiontype.TabIndex = 4;
            this.versiontype.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblplatform
            // 
            this.lblplatform.AutoSize = true;
            this.lblplatform.Location = new System.Drawing.Point(13, 127);
            this.lblplatform.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblplatform.Name = "lblplatform";
            this.lblplatform.Size = new System.Drawing.Size(45, 13);
            this.lblplatform.TabIndex = 5;
            this.lblplatform.Text = "Platform";
            // 
            // platform
            // 
            this.platform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.platform.Location = new System.Drawing.Point(13, 140);
            this.platform.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.platform.Name = "platform";
            this.platform.Size = new System.Drawing.Size(520, 21);
            this.platform.TabIndex = 6;
            this.platform.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblversion
            // 
            this.lblversion.AutoSize = true;
            this.lblversion.Location = new System.Drawing.Point(13, 173);
            this.lblversion.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblversion.Name = "lblversion";
            this.lblversion.Size = new System.Drawing.Size(42, 13);
            this.lblversion.TabIndex = 7;
            this.lblversion.Text = "Version";
            // 
            // version
            // 
            this.version.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.version.Location = new System.Drawing.Point(13, 186);
            this.version.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.version.Name = "version";
            this.version.Size = new System.Drawing.Size(520, 21);
            this.version.TabIndex = 8;
            this.version.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblformat
            // 
            this.lblformat.AutoSize = true;
            this.lblformat.Location = new System.Drawing.Point(13, 219);
            this.lblformat.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblformat.Name = "lblformat";
            this.lblformat.Size = new System.Drawing.Size(39, 13);
            this.lblformat.TabIndex = 9;
            this.lblformat.Text = "Format";
            // 
            // format
            // 
            this.format.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.format.Location = new System.Drawing.Point(13, 232);
            this.format.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.format.Name = "format";
            this.format.Size = new System.Drawing.Size(520, 21);
            this.format.TabIndex = 10;
            this.format.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblusbid
            // 
            this.lblusbid.AutoSize = true;
            this.lblusbid.Location = new System.Drawing.Point(13, 265);
            this.lblusbid.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblusbid.Name = "lblusbid";
            this.lblusbid.Size = new System.Drawing.Size(43, 13);
            this.lblusbid.TabIndex = 11;
            this.lblusbid.Text = "USB ID";
            // 
            // USBID
            // 
            this.USBID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.USBID.Location = new System.Drawing.Point(13, 278);
            this.USBID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.USBID.Name = "USBID";
            this.USBID.Size = new System.Drawing.Size(520, 21);
            this.USBID.TabIndex = 12;
            this.USBID.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblboardid
            // 
            this.lblboardid.AutoSize = true;
            this.lblboardid.Location = new System.Drawing.Point(13, 311);
            this.lblboardid.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblboardid.Name = "lblboardid";
            this.lblboardid.Size = new System.Drawing.Size(49, 13);
            this.lblboardid.TabIndex = 13;
            this.lblboardid.Text = "Board ID";
            // 
            // board_id
            // 
            this.board_id.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.board_id.Location = new System.Drawing.Point(13, 324);
            this.board_id.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.board_id.Name = "board_id";
            this.board_id.Size = new System.Drawing.Size(520, 21);
            this.board_id.TabIndex = 14;
            this.board_id.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblbootloaderid
            // 
            this.lblbootloaderid.AutoSize = true;
            this.lblbootloaderid.Location = new System.Drawing.Point(13, 357);
            this.lblbootloaderid.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblbootloaderid.Name = "lblbootloaderid";
            this.lblbootloaderid.Size = new System.Drawing.Size(72, 13);
            this.lblbootloaderid.TabIndex = 15;
            this.lblbootloaderid.Text = "Bootloader ID";
            // 
            // bootloader_str
            // 
            this.bootloader_str.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bootloader_str.Location = new System.Drawing.Point(13, 370);
            this.bootloader_str.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.bootloader_str.Name = "bootloader_str";
            this.bootloader_str.Size = new System.Drawing.Size(520, 21);
            this.bootloader_str.TabIndex = 16;
            this.bootloader_str.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // lblfirmware
            // 
            this.lblfirmware.AutoSize = true;
            this.lblfirmware.Location = new System.Drawing.Point(13, 403);
            this.lblfirmware.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblfirmware.Name = "lblfirmware";
            this.lblfirmware.Size = new System.Drawing.Size(286, 13);
            this.lblfirmware.TabIndex = 17;
            this.lblfirmware.Text = "Firmwares - Please pick a file to download and upload (.apj)";
            // 
            // Result
            // 
            this.Result.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Result.Location = new System.Drawing.Point(13, 416);
            this.Result.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.Result.Name = "Result";
            this.Result.Size = new System.Drawing.Size(520, 21);
            this.Result.TabIndex = 18;
            this.Result.SelectedIndexChanged += new System.EventHandler(this.Result_OnSelectedIndexChanged);
            // 
            // Button
            // 
            this.Button.Location = new System.Drawing.Point(13, 449);
            this.Button.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.Button.Name = "Button";
            this.Button.Size = new System.Drawing.Size(520, 30);
            this.Button.TabIndex = 19;
            this.Button.Text = "Upload Firmware";
            this.Button.Click += new System.EventHandler(this.Button_OnClicked);
            // 
            // FirmwareSelection
            // 
            this.ClientSize = new System.Drawing.Size(551, 499);
            this.Controls.Add(this.panel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FirmwareSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Firmware Selection";
            this.Load += new System.EventHandler(this.FirmwareSelection_Load);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);

        }

    }
}