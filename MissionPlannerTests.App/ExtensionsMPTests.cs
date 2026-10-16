using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// Real main-app helper logic (ExtensionsMP, in the root Utilities folder)
    /// exercised on .NET 8 via MissionPlannerLib. GetPercent/GetPixel are the
    /// percent&lt;-&gt;pixel size conversions used throughout the UI layout code.
    /// </summary>
    [TestClass]
    public class ExtensionsMPTests
    {
        private static Control Sized(int w, int h) => new Panel { Width = w, Height = h };

        [TestMethod]
        [TestCategory("App")]
        public void GetPixel_ConvertsPercentOfWidthAndHeight()
        {
            var c = Sized(200, 100);
            Assert.AreEqual(100, c.GetPixel(50));            // 50% of width 200
            Assert.AreEqual(50, c.GetPixel(25));             // 25% of width 200
            Assert.AreEqual(50, c.GetPixel(50, height: true)); // 50% of height 100
        }

        [TestMethod]
        [TestCategory("App")]
        public void GetPercent_ConvertsPixelsOfWidthAndHeight()
        {
            var c = Sized(200, 100);
            Assert.AreEqual(50, c.GetPercent(100));            // 100px of width 200
            Assert.AreEqual(25, c.GetPercent(50));             // 50px of width 200
            Assert.AreEqual(40, c.GetPercent(40, height: true)); // 40px of height 100
        }

        [TestMethod]
        [TestCategory("App")]
        public void PercentPixel_RoundTrips()
        {
            // Dimensions chosen so the selected percents map to whole pixels and
            // back exactly (the conversions truncate to int, so they are only a
            // true inverse on clean boundaries).
            var c = Sized(400, 200);
            foreach (var pct in new[] { 0, 10, 25, 50, 75, 100 })
            {
                Assert.AreEqual(pct, c.GetPercent(c.GetPixel(pct)), $"width round-trip at {pct}%");
                Assert.AreEqual(pct, c.GetPercent(c.GetPixel(pct, true), true), $"height round-trip at {pct}%");
            }
        }
    }
}
