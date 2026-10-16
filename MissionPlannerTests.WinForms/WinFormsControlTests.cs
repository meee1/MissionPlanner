using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Controls;

namespace MissionPlanner.Tests.WinForms
{
    /// <summary>
    /// Cross-platform GUI tests for Mission Planner's WinForms controls, running
    /// on plain .NET 8 (no Mono runtime, no display server). The WinForms types
    /// come from the vendored Mono System.Windows.Forms (ExtLibs/mono), compiled
    /// to netstandard2.0 and rendered with SkiaSharp via MissionPlanner.Drawing.
    /// </summary>
    [TestClass]
    public class WinFormsControlTests
    {
        private static readonly Assembly ControlsAssembly = typeof(MyButton).Assembly;

        /// <summary>Every public, concrete control with a parameterless constructor.</summary>
        private static IEnumerable<Type> AllControlTypes() =>
            ControlsAssembly.GetTypes()
                .Where(t => t.IsPublic && !t.IsAbstract && typeof(Control).IsAssignableFrom(t))
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.FullName);

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
        public void AllPublicControls_Instantiate()
        {
            var types = AllControlTypes().ToList();
            Assert.IsTrue(types.Count >= 35, $"expected the full control catalogue, found only {types.Count}");

            var failures = new List<string>();
            foreach (var t in types)
            {
                try
                {
                    var c = (Control)Activator.CreateInstance(t);
                    c.Dispose();
                }
                catch (Exception ex)
                {
                    var e = ex.InnerException ?? ex;
                    failures.Add($"{t.Name}: {e.GetType().Name}: {e.Message.Split('\n')[0]}");
                }
            }

            Assert.AreEqual(0, failures.Count,
                $"{failures.Count}/{types.Count} controls failed to instantiate:\n" + string.Join("\n", failures));
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void AllPublicControls_CanBeHostedOnAForm()
        {
            using var form = new Form();
            // Form-derived types (dialogs) are top-level windows, not child controls.
            var hostable = AllControlTypes().Where(t => !typeof(Form).IsAssignableFrom(t)).ToList();

            var failures = new List<string>();
            foreach (var t in hostable)
            {
                try
                {
                    var c = (Control)Activator.CreateInstance(t);
                    form.Controls.Add(c);
                }
                catch (Exception ex)
                {
                    var e = ex.InnerException ?? ex;
                    failures.Add($"{t.Name}: {e.GetType().Name}");
                }
            }
            Assert.AreEqual(0, failures.Count, "controls that could not be added to a Form:\n" + string.Join("\n", failures));
            Assert.AreEqual(hostable.Count, form.Controls.Count);
            Assert.IsTrue(form.Controls.Count >= 35);
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
            var hsi = new HSI { Heading = 123, NavHeading = 45 };
            Assert.AreEqual(123, hsi.Heading);
            Assert.AreEqual(45, hsi.NavHeading);
        }

        [TestMethod]
        [TestCategory("WinForms")]
        public void TrackBar_And_ProgressBar_ValuesRoundTrip()
        {
            var tb = new MyTrackBar { Minimum = 0, Maximum = 100, Value = 42 };
            Assert.AreEqual(42, tb.Value);

            var pb = new HorizontalProgressBar { Minimum = 0, Maximum = 50 };
            pb.Value = 25;
            Assert.AreEqual(25, pb.Value);
        }

        [DataTestMethod]
        [TestCategory("WinForms")]
        [DataRow(typeof(MyButton))]
        [DataRow(typeof(HSI))]
        [DataRow(typeof(WindDir))]
        [DataRow(typeof(GradientBG))]
        [DataRow(typeof(HorizontalProgressBar2))]
        public void Control_RendersThroughSkia(Type controlType)
        {
            var c = (Control)Activator.CreateInstance(controlType);
            using (c)
            {
                c.Size = new Size(140, 90);
                using var bmp = new Bitmap(c.Width, c.Height);
                c.DrawToBitmap(bmp, new Rectangle(0, 0, c.Width, c.Height));

                bool painted = false;
                for (int y = 0; y < bmp.Height && !painted; y += 4)
                    for (int x = 0; x < bmp.Width && !painted; x += 4)
                        if (bmp.GetPixel(x, y).A > 0)
                            painted = true;

                Assert.IsTrue(painted, $"{controlType.Name} did not render any opaque pixels");
            }
        }
    }
}
