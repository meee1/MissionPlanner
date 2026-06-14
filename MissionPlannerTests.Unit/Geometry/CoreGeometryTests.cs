using System;
using Core.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for the core 2D geometry primitives
    /// (<see cref="PointE"/> and <see cref="RectangleE"/>) used widely for
    /// map/shape calculations.
    /// </summary>
    [TestClass]
    public class CoreGeometryTests
    {
        private const double Tol = 1e-9;

        [TestMethod]
        [TestCategory("Unit")]
        public void PointE_Dist_ThreeFour_IsFive()
        {
            Assert.AreEqual(5.0, new PointE(0, 0).Dist(new PointE(3, 4)), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void PointE_DotProduct()
        {
            Assert.AreEqual(0.0, new PointE(1, 0).DotProduct(new PointE(0, 1)), Tol);
            Assert.AreEqual(11.0, new PointE(1, 2).DotProduct(new PointE(3, 4)), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void PointE_GetNormalized_HasUnitLength()
        {
            var n = new PointE(0, 5).GetNormalized();
            Assert.AreEqual(0.0, n.X, Tol);
            Assert.AreEqual(1.0, n.Y, Tol);
            Assert.AreEqual(1.0, Math.Sqrt(n.X * n.X + n.Y * n.Y), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RectangleE_Contains_Point()
        {
            var r = new RectangleE(0, 0, 10, 10);
            Assert.IsTrue(r.Contains(new PointE(5, 5)));
            Assert.IsFalse(r.Contains(new PointE(15, 5)));
            Assert.IsFalse(r.Contains(new PointE(5, -1)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RectangleE_Contains_Rectangle()
        {
            var outer = new RectangleE(0, 0, 10, 10);
            var inner = new RectangleE(2, 2, 4, 4);
            var overlapping = new RectangleE(5, 5, 10, 10);

            Assert.IsTrue(outer.Contains(inner));
            Assert.IsFalse(outer.Contains(overlapping));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RectangleE_IntersectsWith()
        {
            var a = new RectangleE(0, 0, 10, 10);
            Assert.IsTrue(a.IntersectsWith(new RectangleE(5, 5, 10, 10)));
            Assert.IsFalse(a.IntersectsWith(new RectangleE(100, 100, 5, 5)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RectangleE_Edges()
        {
            // RectangleE uses a Y-up convention: Top is the larger Y edge.
            var r = new RectangleE(1, 2, 10, 20);
            Assert.AreEqual(1.0, r.Left, Tol);
            Assert.AreEqual(11.0, r.Right, Tol);
            Assert.AreEqual(22.0, r.Top, Tol);
            Assert.AreEqual(2.0, r.Bottom, Tol);
        }
    }
}
