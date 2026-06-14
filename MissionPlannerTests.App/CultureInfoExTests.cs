using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// CultureInfoEx (root Utilities/LangUtility.cs) - the culture-hierarchy
    /// helpers Mission Planner uses to resolve translations.
    /// </summary>
    [TestClass]
    public class CultureInfoExTests
    {
        [TestMethod]
        [TestCategory("App")]
        public void GetCultureInfo_ReturnsCultureForValidName()
        {
            var c = CultureInfoEx.GetCultureInfo("en-US");
            Assert.IsNotNull(c);
            Assert.AreEqual("en-US", c.Name);
        }

        [TestMethod]
        [TestCategory("App")]
        public void IsChildOf_WalksTheParentChain()
        {
            var enUS = new CultureInfo("en-US");
            var en = new CultureInfo("en");
            var fr = new CultureInfo("fr");

            Assert.IsTrue(enUS.IsChildOf(en), "en-US descends from en");
            Assert.IsFalse(enUS.IsChildOf(fr), "en-US does not descend from fr");
            Assert.IsFalse(en.IsChildOf(enUS), "parent is not a child of its descendant");
            Assert.IsTrue(enUS.IsChildOf(enUS), "a culture is its own ancestor in this walk");
        }

        [TestMethod]
        [TestCategory("App")]
        public void IsChildOf_NullSafe()
        {
            var en = new CultureInfo("en");
            Assert.IsFalse(((CultureInfo)null).IsChildOf(en));
            Assert.IsFalse(en.IsChildOf(null));
        }
    }
}
