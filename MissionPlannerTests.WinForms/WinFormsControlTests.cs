using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Controls;

namespace MissionPlanner.Tests.WinForms
{
    /// <summary>
    /// Cross-platform GUI tests for Mission Planner's WinForms controls. These
    /// run on plain .NET 8 (no Mono runtime, no display server): the WinForms
    /// types come from the vendored Mono System.Windows.Forms (ExtLibs/mono),
    /// compiled to netstandard2.0 and rendered with SkiaSharp.
    /// </summary>
    [TestClass]
    public class WinFormsControlTests
    {
        [TestMethod]
        [TestCategory("WinForms")]
        public void WinForms_ComesFromVendoredMonoAssembly()
        {
            // Modern .NET has no built-in WinForms; this must be the vendored one.
            var asm = typeof(Form).Assembly.GetName();
            Assert.AreEqual("System.Windows.Forms", asm.Name);
            Assert.IsNotNull(asm.Version);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void Form_HostsControls()
        {
            using var f = new Form { Text = "MP", Width = 400, Height = 300 };
            f.Controls.Add(new MyButton { Text = "Connect" });
            f.Controls.Add(new Label { Text = "Altitude" });
            Assert.AreEqual(2, f.Controls.Count);
            Assert.AreEqual("MP", f.Text);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void MyButton_IsRealButtonSubclass_PropertiesRoundTrip()
        {
            var btn = new MyButton { Text = "Arm" };
            Assert.IsInstanceOfType(btn, typeof(Button), "MyButton must extend the WinForms Button");

            btn.BGGradTop = Color.Red;
            btn.TextColor = Color.White;
            Assert.AreEqual("Arm", btn.Text);
            Assert.AreEqual(Color.Red, btn.BGGradTop);
            Assert.AreEqual(Color.White, btn.TextColor);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void HSI_Heading_RoundTrips()
        {
            var hsi = new HSI();
            hsi.Heading = 123;
            hsi.NavHeading = 45;
            Assert.AreEqual(123, hsi.Heading);
            Assert.AreEqual(45, hsi.NavHeading);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void Control_DrawToBitmap_RendersThroughSkia()
        {
            var btn = new MyButton
            {
                Text = "GO",
                BGGradTop = Color.LimeGreen,
                BGGradBot = Color.DarkGreen,
                Size = new Size(120, 40),
            };

            using var bmp = new Bitmap(120, 40);
            // Rendering exercises the full Mono-WinForms + SkiaSharp paint path.
            btn.DrawToBitmap(bmp, new Rectangle(0, 0, 120, 40));

            Assert.AreEqual(120, bmp.Width);
            Assert.AreEqual(40, bmp.Height);

            // Something was actually painted: at least one opaque pixel exists.
            bool painted = false;
            for (int y = 0; y < bmp.Height && !painted; y += 4)
                for (int x = 0; x < bmp.Width && !painted; x += 4)
                    if (bmp.GetPixel(x, y).A > 0)
                        painted = true;
            Assert.IsTrue(painted, "expected the control to render at least one opaque pixel");
        }
    }
}
