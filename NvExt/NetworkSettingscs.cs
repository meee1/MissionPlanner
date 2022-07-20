using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using MissionPlanner.Utilities;

namespace MissionPlanner.GCSViews
{
    public partial class NetworkSettingscs : Form
    {
        public string network_url;
        public IPAddress url_ip;
        public int url_port;
        public IPAddress vid_fwd_url_ip;
        public int vid_fwd_url_port;
        public bool enable_video_forwarding = false;

        public NetworkSettingscs()
        {
            network_url = null;
            InitializeComponent();
            
            // this.BackColor = Color.FromArgb(38,39,41);
            if (Settings.Instance["video_ip"] != null)
                StreamIPAddressControl.Text = Settings.Instance["video_ip"];
            if (Settings.Instance["video_port"] != null)
                StreamPortTextBox.Text = Settings.Instance["video_port"];

            if (Settings.Instance["video_fwd_enable"] != null)
            {
                if (Settings.Instance["video_fwd_enable"].ToLower().Equals("true"))
                {
                    checkBoxEnableVideoForwarding.Checked = true;
                    enable_video_forwarding = true;
                }
                else
                {
                    checkBoxEnableVideoForwarding.Checked = false;
                    enable_video_forwarding = false;
                }
            }                
            if (Settings.Instance["video_fwd_ip"] != null)
                VideoForwardIpAddressControl.Text = Settings.Instance["video_fwd_ip"];
            if (Settings.Instance["video_fwd_port"] != null)
                textBoxVideoForwardPort.Text = Settings.Instance["video_fwd_port"];

            StreamIPAddressControl.BackColor = Color.FromArgb(67, 68, 69);
            VideoForwardIpAddressControl.BackColor = Color.FromArgb(67, 68, 69);
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /****************************************************************************************************************************
        *                                                      OkButton_Click()
        *
        * Description : Event Handler for the OK Button, parses the values of the boxes 
        *               And construct a url from the IP & Port
        *
        * Arguments   : object sender
        *               EventArgs e
        *
        * Returns     : none
        *
        ****************************************************************************************************************************/
        private void OkButton_Click(object sender, EventArgs e)
        {
            IPAddress ipAddress;
            IPAddress ipAddressVidFwd;
            string ip_addr = StreamIPAddressControl.ToString();
            int port;
            string vid_fwd_ip_addr = VideoForwardIpAddressControl.ToString();
            int vid_fwd_port;

            if (StreamPortTextBox.TextLength > 0)
            {
                port = Convert.ToInt32(StreamPortTextBox.Text);
                /* attempt to parse the input ip address & port */
                if ((IPAddress.TryParse(ip_addr, out ipAddress) == true) && port > 1024 && port < 65536)
                {
                    if (ip_addr != "0.0.0.0")
                    {
                        // update the network URL
                        network_url = @"rtp://" + ip_addr + ":" + port;
                        url_ip = ipAddress;
                        url_port = port;
                        Settings.Instance["video_ip"] = ip_addr;
                        Settings.Instance["video_port"] = port.ToString();                        

                        if(enable_video_forwarding)
                        {
                            if (textBoxVideoForwardPort.TextLength > 0)
                            {
                                vid_fwd_port = Convert.ToInt32(textBoxVideoForwardPort.Text);
                                /* attempt to parse the input ip address & port */
                                if ((IPAddress.TryParse(vid_fwd_ip_addr, out ipAddressVidFwd) == true) && vid_fwd_port > 1024 && vid_fwd_port < 65536)
                                {
                                    if (vid_fwd_ip_addr != "0.0.0.0")
                                    {
                                        vid_fwd_url_ip = ipAddressVidFwd;
                                        vid_fwd_url_port = vid_fwd_port;
                                        Settings.Instance["video_fwd_ip"] = vid_fwd_ip_addr;
                                        Settings.Instance["video_fwd_port"] = vid_fwd_port.ToString();
                                        this.Close();
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //vid_fwd_url_ip = ;
                            vid_fwd_url_port = 0;
                            this.Close();
                            return;
                        }
                    }
                }
            }

            // show error message
            MessageBox.Show("Improper IP/Port Entered !", "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

        }

        /****************************************************************************************************************************
        *                                                      StreamPortTextBox_KeyPress()
        *
        * Description : Stream Port Textbox KeyPress handler, Used to allow only a 5 digit number up to 65536
        *
        * Arguments   : object sender
        *               EventArgs e
        *
        * Returns     : none
        *
        ****************************************************************************************************************************/
        private void StreamPortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "[^\b]"))
                return;
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "\\d+") || StreamPortTextBox.TextLength > 4)
            {
                if (StreamPortTextBox.TextLength == StreamPortTextBox.SelectionLength && StreamPortTextBox.SelectionLength > 0)
                    return;
                e.Handled = true;
            }
            if (StreamPortTextBox.TextLength > 0)
                if (Convert.ToInt32(StreamPortTextBox.Text) > 65536)
                    StreamPortTextBox.Text = "65536";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Improper IP/Port Entered !", "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
        }

        private void checkBoxEnableVideoForwarding_CheckedChanged(object sender, EventArgs e)
        {
            enable_video_forwarding = checkBoxEnableVideoForwarding.Checked;

            Settings.Instance["video_fwd_enable"] = enable_video_forwarding.ToString().ToLower();

            textBoxVideoForwardPort.Enabled = enable_video_forwarding;
            VideoForwardIpAddressControl.Enabled = enable_video_forwarding;
            labelVideoForwardingIP.Enabled = enable_video_forwarding;
            labelVideoForwardingPort.Enabled = enable_video_forwarding;                       
        }
    }
}
