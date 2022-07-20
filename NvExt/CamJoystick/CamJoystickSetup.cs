using System;
using System.Drawing;
using System.Windows.Forms;
using MissionPlanner.Controls;
using MissionPlanner.Utilities;
using SharpDX.DirectInput;

namespace MissionPlanner.CamJoystick
{
    public partial class CamJoystickSetup : Form
    {
        public CamJoystickSetup()
        {
            InitializeComponent();
            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);
        }

        private void Joystick_Load(object sender, EventArgs e)
        {
            CMB_CH0.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH1.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH2.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH3.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH4.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH5.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH6.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH7.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH8.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH9.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH10.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH11.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH12.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH13.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH14.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH15.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH16.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH17.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH18.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH19.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH20.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH21.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH22.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH23.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH24.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH25.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH26.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));
            CMB_CH27.DataSource = (Enum.GetValues(typeof(CamJoystick.joystickaxis)));

            var tempjoystick = new CamJoystick();
            CMB_CH0.Text = tempjoystick.getChannel(0).axis.ToString();
            CMB_CH1.Text = tempjoystick.getChannel(1).axis.ToString();
            CMB_CH2.Text = tempjoystick.getChannel(2).axis.ToString();
            CMB_CH3.Text = tempjoystick.getChannel(3).axis.ToString();
            CMB_CH4.Text = tempjoystick.getChannel(4).axis.ToString();
            CMB_CH5.Text = tempjoystick.getChannel(5).axis.ToString();
            CMB_CH6.Text = tempjoystick.getChannel(6).axis.ToString();
            CMB_CH7.Text = tempjoystick.getChannel(7).axis.ToString();
            CMB_CH8.Text = tempjoystick.getChannel(8).axis.ToString();
            CMB_CH9.Text = tempjoystick.getChannel(9).axis.ToString();
            CMB_CH10.Text = tempjoystick.getChannel(10).axis.ToString();
            CMB_CH11.Text = tempjoystick.getChannel(11).axis.ToString();
            CMB_CH12.Text = tempjoystick.getChannel(12).axis.ToString();
            CMB_CH13.Text = tempjoystick.getChannel(13).axis.ToString();
            CMB_CH14.Text = tempjoystick.getChannel(14).axis.ToString();
            CMB_CH15.Text = tempjoystick.getChannel(15).axis.ToString();
            CMB_CH16.Text = tempjoystick.getChannel(16).axis.ToString();
            CMB_CH17.Text = tempjoystick.getChannel(17).axis.ToString();
            CMB_CH18.Text = tempjoystick.getChannel(18).axis.ToString();
            CMB_CH19.Text = tempjoystick.getChannel(19).axis.ToString();
            CMB_CH20.Text = tempjoystick.getChannel(20).axis.ToString();
            CMB_CH21.Text = tempjoystick.getChannel(21).axis.ToString();
            CMB_CH22.Text = tempjoystick.getChannel(22).axis.ToString();
            CMB_CH23.Text = tempjoystick.getChannel(23).axis.ToString();
            CMB_CH24.Text = tempjoystick.getChannel(24).axis.ToString();
            CMB_CH25.Text = tempjoystick.getChannel(25).axis.ToString();
            CMB_CH26.Text = tempjoystick.getChannel(26).axis.ToString();
            CMB_CH27.Text = tempjoystick.getChannel(27).axis.ToString();

            RevCH0.Checked = (tempjoystick.getChannel(0).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH1.Checked = (tempjoystick.getChannel(1).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH2.Checked = (tempjoystick.getChannel(2).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH3.Checked = (tempjoystick.getChannel(3).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH4.Checked = (tempjoystick.getChannel(4).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH5.Checked = (tempjoystick.getChannel(5).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH6.Checked = (tempjoystick.getChannel(6).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH7.Checked = (tempjoystick.getChannel(7).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH8.Checked = (tempjoystick.getChannel(8).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH9.Checked = (tempjoystick.getChannel(9).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH10.Checked = (tempjoystick.getChannel(10).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH11.Checked = (tempjoystick.getChannel(11).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH12.Checked = (tempjoystick.getChannel(12).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH13.Checked = (tempjoystick.getChannel(13).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH14.Checked = (tempjoystick.getChannel(14).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH15.Checked = (tempjoystick.getChannel(15).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH16.Checked = (tempjoystick.getChannel(16).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH17.Checked = (tempjoystick.getChannel(17).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH18.Checked = (tempjoystick.getChannel(18).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH19.Checked = (tempjoystick.getChannel(19).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH20.Checked = (tempjoystick.getChannel(20).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH21.Checked = (tempjoystick.getChannel(21).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH22.Checked = (tempjoystick.getChannel(22).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH23.Checked = (tempjoystick.getChannel(23).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH24.Checked = (tempjoystick.getChannel(24).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH25.Checked = (tempjoystick.getChannel(25).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH26.Checked = (tempjoystick.getChannel(26).reverse.ToString().ToLower() == "false") ? false : true;
            RevCH27.Checked = (tempjoystick.getChannel(27).reverse.ToString().ToLower() == "false") ? false : true;

            if (Settings.Instance.ContainsKey("cam_joystick_name"))
            {
                if (Settings.Instance["cam_joystick_name"].ToString() != "")
                    CMB_joysticks.Text = Settings.Instance["cam_joystick_name"].ToString();
            }

            if (Settings.Instance.ContainsKey("joystickDZ"))
            {  
                joystickDzTextBox.Text = Settings.Instance["joystickDZ"].ToString();
            }

            if (Settings.Instance.ContainsKey("joystickGain"))
            {
                joystickGainTextBox.Text = Settings.Instance["joystickGain"].ToString();
            }
        }

        private void BUT_Default_Click(object sender, EventArgs e)
        {

            if (MainV2.Camjoystick == null)
                return;

            MainV2.Camjoystick.setAxis(0, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "HatLeftRight"));
            MainV2.Camjoystick.setAxis(1, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "HatUpDown"));
            MainV2.Camjoystick.setAxis(2, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn1"));
            MainV2.Camjoystick.setAxis(3, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn2"));
            MainV2.Camjoystick.setAxis(4, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn3"));
            MainV2.Camjoystick.setAxis(5, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn4"));
            MainV2.Camjoystick.setAxis(6, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn5"));
            MainV2.Camjoystick.setAxis(7, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn6"));
            MainV2.Camjoystick.setAxis(8, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn7"));
            MainV2.Camjoystick.setAxis(9, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn8"));
            MainV2.Camjoystick.setAxis(10, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn9"));
            MainV2.Camjoystick.setAxis(11, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn10"));
            MainV2.Camjoystick.setAxis(12, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn11"));
            MainV2.Camjoystick.setAxis(13, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn12"));
            MainV2.Camjoystick.setAxis(14, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn13"));
            MainV2.Camjoystick.setAxis(15, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn14"));
            MainV2.Camjoystick.setAxis(16, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn15"));
            MainV2.Camjoystick.setAxis(17, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn16"));
            MainV2.Camjoystick.setAxis(18, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn17"));
            MainV2.Camjoystick.setAxis(19, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn18"));
            MainV2.Camjoystick.setAxis(20, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "btn19"));
            MainV2.Camjoystick.setAxis(21, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));
            MainV2.Camjoystick.setAxis(22, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));
            MainV2.Camjoystick.setAxis(23, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));
            MainV2.Camjoystick.setAxis(24, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));
            MainV2.Camjoystick.setAxis(25, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));
            MainV2.Camjoystick.setAxis(26, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));
            MainV2.Camjoystick.setAxis(27, (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), "None"));

            CMB_CH0.Text = "HatLeftRight";
            CMB_CH1.Text = "HatUpDown";
            CMB_CH2.Text = "btn1";
            CMB_CH3.Text = "btn2";
            CMB_CH4.Text = "btn3";
            CMB_CH5.Text = "btn4";
            CMB_CH6.Text = "btn5";
            CMB_CH7.Text = "btn6";
            CMB_CH8.Text = "btn7";
            CMB_CH9.Text = "btn8";
            CMB_CH10.Text = "btn9";
            CMB_CH11.Text = "btn10";
            CMB_CH12.Text = "btn11";
            CMB_CH13.Text = "btn12";
            CMB_CH14.Text = "btn13";
            CMB_CH15.Text = "btn14";
            CMB_CH16.Text = "btn15";
            CMB_CH17.Text = "btn16";
            CMB_CH18.Text = "btn17";
            CMB_CH19.Text = "btn18";
            CMB_CH20.Text = "btn19";
            CMB_CH21.Text = "None";
            CMB_CH22.Text = "None";
            CMB_CH23.Text = "None";
            CMB_CH24.Text = "None";
            CMB_CH25.Text = "None";
            CMB_CH26.Text = "None";
            CMB_CH27.Text = "None";

        }

        private void BUT_Update_Click(object sender, EventArgs e)
        {
            if (CMB_joysticks.Text != "")
            {
                try
                {
                    MainV2.Camjoystick.enabled = false;
                    MainV2.Camjoystick.UnAcquireJoyStick();
                    MainV2.Camjoystick = null;
                }
                catch
                {
                }

                CamJoystick joy = new CamJoystick();

                if (!joy.start(CMB_joysticks.Text))
                {
                    CustomMessageBox.Show("Please Connect a Joystick", "No Joystick");
                    joy.Dispose();
                    return;
                }

                Settings.Instance["cam_joystick_name"] = CMB_joysticks.Text;
                MainV2.Camjoystick = joy;
                MainV2.Camjoystick.enabled = true;
                camjoystick_timer.Start();
            }
        }

        private void BUT_Save_Click(object sender, EventArgs e)
        {
            CamJoystick.self.saveconfig();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            PB_CH0.Value = MainV2.Camjoystick.ch_data[0];
            PB_CH1.Value = MainV2.Camjoystick.ch_data[1];
            PB_CH2.Value = MainV2.Camjoystick.ch_data[2];
            PB_CH3.Value = MainV2.Camjoystick.ch_data[3];
            PB_CH4.Value = MainV2.Camjoystick.ch_data[4];
            PB_CH5.Value = MainV2.Camjoystick.ch_data[5];
            PB_CH6.Value = MainV2.Camjoystick.ch_data[6];
            PB_CH7.Value = MainV2.Camjoystick.ch_data[7];
            PB_CH8.Value = MainV2.Camjoystick.ch_data[8];
            PB_CH9.Value = MainV2.Camjoystick.ch_data[9];
            PB_CH10.Value = MainV2.Camjoystick.ch_data[10];
            PB_CH11.Value = MainV2.Camjoystick.ch_data[11];
            PB_CH12.Value = MainV2.Camjoystick.ch_data[12];
            PB_CH13.Value = MainV2.Camjoystick.ch_data[13];
            PB_CH14.Value = MainV2.Camjoystick.ch_data[14];
            PB_CH15.Value = MainV2.Camjoystick.ch_data[15];
            PB_CH16.Value = MainV2.Camjoystick.ch_data[16];
            PB_CH17.Value = MainV2.Camjoystick.ch_data[17];
            PB_CH18.Value = MainV2.Camjoystick.ch_data[18];
            PB_CH19.Value = MainV2.Camjoystick.ch_data[19];
            PB_CH20.Value = MainV2.Camjoystick.ch_data[20];
            PB_CH21.Value = MainV2.Camjoystick.ch_data[21];
            PB_CH22.Value = MainV2.Camjoystick.ch_data[22];
            PB_CH23.Value = MainV2.Camjoystick.ch_data[23];
            PB_CH24.Value = MainV2.Camjoystick.ch_data[24];
            PB_CH25.Value = MainV2.Camjoystick.ch_data[25];
            PB_CH26.Value = MainV2.Camjoystick.ch_data[26];
            PB_CH27.Value = MainV2.Camjoystick.ch_data[27];

            try
            {
                if (MainV2.Camjoystick != null)
                {
                    PB_CH0.maxline = MainV2.Camjoystick.getRawValueForChannel(0);
                    PB_CH1.maxline = MainV2.Camjoystick.getRawValueForChannel(1);
                    PB_CH2.maxline = MainV2.Camjoystick.getRawValueForChannel(2);
                    PB_CH3.maxline = MainV2.Camjoystick.getRawValueForChannel(3);
                    PB_CH4.maxline = MainV2.Camjoystick.getRawValueForChannel(4);
                    PB_CH5.maxline = MainV2.Camjoystick.getRawValueForChannel(5);
                    PB_CH6.maxline = MainV2.Camjoystick.getRawValueForChannel(6);
                    PB_CH7.maxline = MainV2.Camjoystick.getRawValueForChannel(7);
                    PB_CH8.maxline = MainV2.Camjoystick.getRawValueForChannel(8);
                    PB_CH9.maxline = MainV2.Camjoystick.getRawValueForChannel(9);
                    PB_CH10.maxline = MainV2.Camjoystick.getRawValueForChannel(10);
                    PB_CH11.maxline = MainV2.Camjoystick.getRawValueForChannel(11);
                    PB_CH12.maxline = MainV2.Camjoystick.getRawValueForChannel(12);
                    PB_CH13.maxline = MainV2.Camjoystick.getRawValueForChannel(13);
                    PB_CH14.maxline = MainV2.Camjoystick.getRawValueForChannel(14);
                    PB_CH15.maxline = MainV2.Camjoystick.getRawValueForChannel(15);
                    PB_CH16.maxline = MainV2.Camjoystick.getRawValueForChannel(16);
                    PB_CH17.maxline = MainV2.Camjoystick.getRawValueForChannel(17);
                    PB_CH18.maxline = MainV2.Camjoystick.getRawValueForChannel(18);
                    PB_CH19.maxline = MainV2.Camjoystick.getRawValueForChannel(19);
                    PB_CH20.maxline = MainV2.Camjoystick.getRawValueForChannel(20);
                    PB_CH21.maxline = MainV2.Camjoystick.getRawValueForChannel(21);
                    PB_CH22.maxline = MainV2.Camjoystick.getRawValueForChannel(22);
                    PB_CH23.maxline = MainV2.Camjoystick.getRawValueForChannel(23);
                    PB_CH24.maxline = MainV2.Camjoystick.getRawValueForChannel(24);
                    PB_CH25.maxline = MainV2.Camjoystick.getRawValueForChannel(25);
                    PB_CH26.maxline = MainV2.Camjoystick.getRawValueForChannel(26);
                    PB_CH27.maxline = MainV2.Camjoystick.getRawValueForChannel(27);

                }
            }
            catch
            {
            }
        }

        private void BUT_detch0_Click(object sender, EventArgs e)
        {
            CMB_CH0.Text = CamJoystick.getMovingAxis(CMB_joysticks.Text, 16000).ToString();
        }

        private void BUT_detch1_Click(object sender, EventArgs e)
        {
            CMB_CH1.Text = CamJoystick.getMovingAxis(CMB_joysticks.Text, 16000).ToString();
        }

        private void BUT_detch2_Click(object sender, EventArgs e)
        {
            CMB_CH2.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch3_Click(object sender, EventArgs e)
        {
            CMB_CH3.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }
        private void BUT_detch4_Click(object sender, EventArgs e)
        {
            CMB_CH4.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch5_Click(object sender, EventArgs e)
        {
            CMB_CH5.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch6_Click(object sender, EventArgs e)
        {
            CMB_CH6.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch7_Click(object sender, EventArgs e)
        {
            CMB_CH7.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch8_Click(object sender, EventArgs e)
        {
            CMB_CH8.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch9_Click(object sender, EventArgs e)
        {
            CMB_CH9.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch10_Click(object sender, EventArgs e)
        {
            CMB_CH10.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch11_Click(object sender, EventArgs e)
        {
            CMB_CH11.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch12_Click(object sender, EventArgs e)
        {
            CMB_CH12.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch13_Click(object sender, EventArgs e)
        {
            CMB_CH13.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch14_Click(object sender, EventArgs e)
        {
            CMB_CH14.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch15_Click(object sender, EventArgs e)
        {
            CMB_CH15.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch16_Click(object sender, EventArgs e)
        {
            CMB_CH16.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }
        /******/
        private void BUT_detch17_Click(object sender, EventArgs e)
        {
            CMB_CH17.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch18_Click(object sender, EventArgs e)
        {
            CMB_CH18.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch19_Click(object sender, EventArgs e)
        {
            CMB_CH19.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch20_Click(object sender, EventArgs e)
        {
            CMB_CH20.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch21_Click(object sender, EventArgs e)
        {
            CMB_CH21.Text = CamJoystick.getMovingAxis(CMB_joysticks.Text, 16000).ToString();
        }

        private void BUT_detch22_Click(object sender, EventArgs e)
        {
            CMB_CH22.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch23_Click(object sender, EventArgs e)
        {
            CMB_CH23.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch24_Click(object sender, EventArgs e)
        {
            CMB_CH24.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch25_Click(object sender, EventArgs e)
        {
            CMB_CH25.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch26_Click(object sender, EventArgs e)
        {
            CMB_CH26.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void BUT_detch27_Click(object sender, EventArgs e)
        {
            CMB_CH27.Text = "btn" + CamJoystick.getPressedButton(CMB_joysticks.Text).ToString();
        }

        private void CMB_CH0_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(0,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(1,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(2,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(3,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(4,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(5,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(6,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }
        private void CMB_CH7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(7,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(8,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(9,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH10_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(10,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH11_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(11,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH12_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(12,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH13_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(13,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH14_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(14,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));

        }

        private void CMB_CH15_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(15,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH16_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(16,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH17_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(17,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH18_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(18,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH19_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(19,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH20_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(20,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH21_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(21,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH22_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(22,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH23_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(23,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH24_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(24,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH25_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(25,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH26_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(26,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void CMB_CH27_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick == null)
                return;
            MainV2.Camjoystick.setAxis(27,
            (CamJoystick.joystickaxis)Enum.Parse(typeof(CamJoystick.joystickaxis), ((ComboBox)sender).Text));
        }

        private void revCH0_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(0, ((CheckBox)sender).Checked);
        }

        private void revCH1_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(1, ((CheckBox)sender).Checked);
        }

        private void revCH2_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(2, ((CheckBox)sender).Checked);
        }

        private void revCH3_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(3, ((CheckBox)sender).Checked);
        }

        private void revCH4_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(4, ((CheckBox)sender).Checked);
        }

        private void revCH5_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(5, ((CheckBox)sender).Checked);
        }

        private void revCH6_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(6, ((CheckBox)sender).Checked);
        }

        private void revCH7_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(7, ((CheckBox)sender).Checked);
        }
        private void revCH8_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(8, ((CheckBox)sender).Checked);
        }

        private void revCH9_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(9, ((CheckBox)sender).Checked);
        }

        private void revCH10_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(10, ((CheckBox)sender).Checked);
        }

        private void revCH11_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(11, ((CheckBox)sender).Checked);
        }

        private void revCH12_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(12, ((CheckBox)sender).Checked);
        }

        private void revCH13_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(13, ((CheckBox)sender).Checked);
        }

        private void revCH14_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(14, ((CheckBox)sender).Checked);
        }

        private void revCH15_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(15, ((CheckBox)sender).Checked);
        }

        private void revCH16_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(16, ((CheckBox)sender).Checked);
        }

        private void revCH17_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(17, ((CheckBox)sender).Checked);
        }

        private void revCH18_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(18, ((CheckBox)sender).Checked);
        }

        private void revCH19_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(19, ((CheckBox)sender).Checked);
        }

        private void revCH20_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(20, ((CheckBox)sender).Checked);
        }

        private void RevCH21_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(21, ((CheckBox)sender).Checked);
        }

        private void RevCH22_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(22, ((CheckBox)sender).Checked);
        }

        private void RevCH23_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(23, ((CheckBox)sender).Checked);
        }

        private void RevCH24_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(24, ((CheckBox)sender).Checked);
        }

        private void RevCH25_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(25, ((CheckBox)sender).Checked);
        }

        private void RevCH26_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(26, ((CheckBox)sender).Checked);
        }

        private void RevCH27_CheckedChanged(object sender, EventArgs e)
        {
            if (MainV2.Camjoystick != null)
                MainV2.Camjoystick.setReverse(27, ((CheckBox)sender).Checked);
        }

        private void CMB_joysticks_MouseClick(object sender, MouseEventArgs e)
        {
            CMB_joysticks.Items.Clear();

            var joysticklist = CamJoystick.getDevices();
            int i = 1;

            foreach (DeviceInstance device in joysticklist)
            {
                CMB_joysticks.Items.Add(device.ProductName);
                i++;
            }

            if (CMB_joysticks.Items.Count > 0 && CMB_joysticks.SelectedIndex == -1)
                CMB_joysticks.SelectedIndex = 0;
        }

        private void CamJoystickSetup_FormClosed(object sender, FormClosedEventArgs e)
        {
            camjoystick_timer.Stop();
        }

        private void BUT_setJoyDz_Click(object sender, EventArgs e)
        {
            int joy_dz;
            if(!Int32.TryParse(joystickDzTextBox.Text, out joy_dz))
            {
                CustomMessageBox.Show("Joystick dz, Bad Value Entered");
                joystickDzTextBox.Text = Settings.Instance["joystickDZ"].ToString();
                return;
            }
            if(joy_dz < 0 || joy_dz > 200)
            {
                CustomMessageBox.Show("Joystick dz, Bad Value Entered, Valid Range Is: 0 - 200");
                joystickDzTextBox.Text = Settings.Instance["joystickDZ"].ToString();
                return;
            }
            MainV2.Camjoystick.joystick_DZ = joy_dz;
            Settings.Instance["joystickDZ"] = joystickDzTextBox.Text;
        }

        private void BUT_setJoyGain_Click(object sender, EventArgs e)
        {
            int joy_gain;
            if (!Int32.TryParse(joystickGainTextBox.Text, out joy_gain))
            {
                CustomMessageBox.Show("Joystick Gain, Bad Value Entered");
                joystickGainTextBox.Text = Settings.Instance["joystickGain"].ToString();
                return;
            }
            if (joy_gain < 10 || joy_gain > 200)
            {
                CustomMessageBox.Show("Joystick Gain, Bad Value Entered, Valid Range Is: 10 - 200 (%)");
                joystickGainTextBox.Text = Settings.Instance["joystickGain"].ToString();
                return;
            }
            MainV2.Camjoystick.joystick_gain = ((double)joy_gain/ (double)100.0);
            Settings.Instance["joystickGain"] = joystickGainTextBox.Text;
        }        
    }
}
