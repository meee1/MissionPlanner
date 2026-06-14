using Core.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for <see cref="IntervalE"/> (a 1D min/max range used by
    /// RectangleE and other shape maths): containment, intersection, dimension.
    /// </summary>
    [TestClass]
    public class IntervalETests
    {
        private const double Tol = 1e-9;

        [TestMethod]
        [TestCategory("Unit")]
        public void Dimension_IsMaxMinusMin()
        {
            Assert.AreEqual(8.0, new IntervalE(2, 10).Dimension, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Contains_Value()
        {
            var i = new IntervalE(0, 10);
            Assert.IsTrue(i.Contains(5));
            Assert.IsTrue(i.Contains(0));   // inclusive
            Assert.IsTrue(i.Contains(10));  // inclusive
            Assert.IsFalse(i.Contains(-1));
            Assert.IsFalse(i.Contains(11));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Contains_Interval()
        {
            var outer = new IntervalE(0, 10);
            Assert.IsTrue(outer.Contains(new IntervalE(2, 8)));
            Assert.IsFalse(outer.Contains(new IntervalE(5, 15)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void IntersectsWith()
        {
            var a = new IntervalE(0, 10);
            Assert.IsTrue(a.IntersectsWith(new IntervalE(5, 15)));
            Assert.IsTrue(a.IntersectsWith(new IntervalE(-5, 5)));
            Assert.IsFalse(a.IntersectsWith(new IntervalE(20, 30)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void IsEmpty_WhenZeroWidth()
        {
            Assert.IsTrue(new IntervalE(5, 5).IsEmpty);
            Assert.IsFalse(new IntervalE(5, 6).IsEmpty);
        }
    }
}
