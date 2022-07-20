using System;
using System.Reflection;
using System.Windows.Forms;
using MissionPlanner.NvExt;

namespace MissionPlanner
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();

            string strVersion = typeof(Splash).GetType().Assembly.GetName().Version.ToString();

            TXT_version.Text = "Version: " + Application.ProductVersion + NvMavExtCmds.NvVersion; // +" Build " + strVersion;

            Console.WriteLine(strVersion);

            if (Program.Logo != null)
            {
                pictureBox1.BackgroundImage = MissionPlanner.Properties.Resources.bgdark;
                pictureBox1.Image = Program.Logo;
                pictureBox1.Visible = true;
            }

            Console.WriteLine("Splash .ctor");
        }
    }
}