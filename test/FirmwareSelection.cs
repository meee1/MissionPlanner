using MissionPlanner.ArduPilot;
using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DeviceInfo = MissionPlanner.ArduPilot.DeviceInfo;

namespace MissionPlanner.test
{
    /// <summary>
    /// Firmware filter/picker. Ported from a Xamarin.Forms ContentPage to a native WinForms Form so
    /// it runs everywhere the app does (desktop directly, mobile via the Mono WinForms-on-Skia host)
    /// without any Xamarin.Forms dependency. Shown with ShowDialog(); the chosen url is FinalResult
    /// (null if the user closed without picking).
    /// </summary>
    public class FirmwareSelection : Form
    {
        private Label lbltype, lblversiontype, lblplatform, lblversion, lblformat, lblusbid, lblboardid, lblbootloaderid, lblfirmware;
        private ComboBox mavtype, versiontype, platform, version, format, USBID, board_id, bootloader_str, Result;
        private Button Button;

        public FirmwareSelection(List<APFirmware.FirmwareInfo> fwitems, DeviceInfo? item = null)
        {
            InitializeComponent();

            DevInfo = item;
            FWList = fwitems;

            OnSelectedIndexChanged(null, null);

            if (DevInfo.HasValue)
            {
                platform.SelectedItem = DevInfo.Value.board?.Replace("-BL", "");

                mavtype.Visible = false;
                lbltype.Visible = false;

                USBID.Visible = false;
                lblusbid.Visible = false;

                bootloader_str.Visible = false;
                lblbootloaderid.Visible = false;

                board_id.Visible = false;
                lblboardid.Visible = false;

                if (versiontype.Items.Count == 2)
                {
                    versiontype.Visible = false;
                    lblversiontype.Visible = false;
                }

                {
                    format.Visible = false;
                    lblformat.Visible = false;
                }

                if (version.Items.Count == 2)
                {
                    version.Visible = false;
                    lblversion.Visible = false;
                }
            }

            try { MissionPlanner.Controls.ThemeManager.ApplyThemeTo(this); } catch { }
        }

        public DeviceInfo? DevInfo { get; set; } = null;

        public IEnumerable<APFirmware.FirmwareInfo> FWList { get; set; }

        public string FinalResult { get; set; }

        private void InitializeComponent()
        {
            Text = "Firmware Selection";
            ClientSize = new Size(560, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            Controls.Add(panel);

            Label MakeLabel(string text)
            {
                var l = new Label { Text = text, AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
                panel.Controls.Add(l);
                return l;
            }

            ComboBox MakeCombo()
            {
                var c = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 520,
                    Margin = new Padding(3, 0, 3, 6)
                };
                c.SelectedIndexChanged += OnSelectedIndexChanged;
                panel.Controls.Add(c);
                return c;
            }

            var hdr = new Label { Text = "More than one choice exists. Please filter down to the desired selection.", AutoSize = true, Margin = new Padding(3, 0, 3, 6) };
            panel.Controls.Add(hdr);

            lbltype = MakeLabel("Type"); mavtype = MakeCombo();
            lblversiontype = MakeLabel("Version Type"); versiontype = MakeCombo();
            lblplatform = MakeLabel("Platform"); platform = MakeCombo();
            lblversion = MakeLabel("Version"); version = MakeCombo();
            lblformat = MakeLabel("Format"); format = MakeCombo();
            lblusbid = MakeLabel("USB ID"); USBID = MakeCombo();
            lblboardid = MakeLabel("Board ID"); board_id = MakeCombo();
            lblbootloaderid = MakeLabel("Bootloader ID"); bootloader_str = MakeCombo();

            lblfirmware = MakeLabel("Firmwares - Please pick a file to download and upload (.apj)");
            Result = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 520, Margin = new Padding(3, 0, 3, 6) };
            Result.SelectedIndexChanged += Result_OnSelectedIndexChanged;
            panel.Controls.Add(Result);

            Button = new Button { Text = "Upload Firmware", Height = 30, Width = 520, Margin = new Padding(3, 6, 3, 6) };
            Button.Click += Button_OnClicked;
            panel.Controls.Add(Button);
        }

        private bool _updating;

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            // WinForms raises SelectedIndexChanged while we repopulate the combos (Items.Clear /
            // SelectedItem = null); guard against reentrant recursion.
            if (_updating)
                return;
            _updating = true;
            try
            {
                UpdateFilters();
            }
            finally
            {
                _updating = false;
            }
        }

        private void UpdateFilters()
        {
            var FWList = this.FWList;

            if (board_id.SelectedItem != null && (string)board_id.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.BoardId.ToString() == (string)board_id.SelectedItem);
            }

            if (mavtype.SelectedItem != null && (string)mavtype.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.MavType == (string)mavtype.SelectedItem);
            }

            if (versiontype.SelectedItem != null && (string)versiontype.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.MavFirmwareVersionType == (string)versiontype.SelectedItem);
            }

            if (format.SelectedItem != null && (string)format.SelectedItem != "Ignore")
            {
                //FWList = FWList.Where(a => a.Format == (string) format.SelectedItem);
            }

            if (platform.SelectedItem != null && (string)platform.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.Platform == (string)platform.SelectedItem);
            }

            if (version.SelectedItem != null && (string)version.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.MavFirmwareVersion.ToString() == (string)version.SelectedItem);
            }

            if (USBID.SelectedItem != null && (string)USBID.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.Usbid.Any(b => b.Contains((string)USBID.SelectedItem)));
            }

            if (bootloader_str.SelectedItem != null && (string)bootloader_str.SelectedItem != "Ignore")
            {
                FWList = FWList.Where(a => a.BootloaderStr.Any(b => b.Contains((string)bootloader_str.SelectedItem)));
            }

            Result.Items.Clear();
            if (FWList.Count() < 100)
            {
                Result.Items.AddRange(FWList.Select(a => (object)a.Url.AbsoluteUri).ToArray());
                if (FWList.Count() == 1)
                    Result.SelectedIndex = 0;
            }
            else
            {
                Result.Items.Add("To many options - apply more filters - " + FWList.Count());
                Result.SelectedIndex = 0;
            }

            if (FWList.Count() == 0)
            {
                Result.Items.Add("No options to show");
                Result.SelectedIndex = 0;
                return;
            }

            PopulatePicker(lblboardid, board_id,
                FWList.Select(a => a.BoardId.ToString()).Distinct().OrderBy(a => int.Parse(a)));

            PopulatePicker(lbltype, mavtype, FWList.Select(a => a.MavType.ToString()).Distinct().OrderBy(a => a));

            PopulatePicker(lblversiontype, versiontype,
                FWList.Select(a => a.MavFirmwareVersionType.ToString()).Distinct().OrderBy(a => a));

            PopulatePicker(lblformat, format, FWList.Select(a => a.Format.ToString()).Distinct().OrderBy(a => a));

            PopulatePicker(lblplatform, platform, FWList.Select(a => a.Platform.ToString()).Distinct().OrderBy(a => a));

            PopulatePicker(lblversion, version,
                FWList.Select(a => a.MavFirmwareVersion.ToString()).Distinct().OrderBy(a => a));

            PopulatePicker(lblusbid, USBID,
                FWList.Where(a => a.Usbid?.Length > 0).SelectMany(a => a.Usbid, (info, s) => s).Distinct()
                    .OrderBy(a => a));

            PopulatePicker(lblbootloaderid, bootloader_str,
                FWList.Where(a => a.BootloaderStr?.Length > 0)
                    .SelectMany(info => info.BootloaderStr, (info, s) => s)
                    .Distinct().OrderBy(a => a));
        }

        private void PopulatePicker(Label label, ComboBox picker, IEnumerable<string> list)
        {
            try
            {
                if (!picker.Visible)
                {
                    picker.SelectedItem = null;
                    return;
                }

                if (picker.SelectedItem == null)
                {
                    var pick = list.ToList();
                    pick.Add("Ignore");
                    picker.Items.Clear();
                    picker.Items.AddRange(pick.Cast<object>().ToArray());
                }

                if (picker.SelectedItem != null && (string)picker.SelectedItem == "Ignore")
                    picker.SelectedItem = null;
            }
            catch
            {
            }
        }

        private void Result_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            if (Result.SelectedItem == null)
            {
                return;
            }

            FinalResult = Result.SelectedItem.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
