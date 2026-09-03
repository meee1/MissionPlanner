using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simplify;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for Douglas-Peucker polyline simplification
    /// (<see cref="Douglas"/>), used to thin tracks/paths.
    /// </summary>
    [TestClass]
    public class DouglasPeuckerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void PerpendicularDistance_PointAboveBaseline()
        {
            // distance of (5,3) from the segment (0,0)-(10,0) is 3.
            double d = Douglas.PerpendicularDistance(new PointF(0, 0), new PointF(10, 0), new PointF(5, 3));
            Assert.AreEqual(3.0, d, 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Reduction_StraightLine_KeepsOnlyEndpoints()
        {
            // Collinear points (with tiny noise below tolerance) collapse to the ends.
            var pts = new List<PointF>
            {
                new PointF(0, 0), new PointF(1, 0.01f), new PointF(2, 0),
                new PointF(3, 0.01f), new PointF(4, 0), new PointF(5, 0),
            };
            var reduced = Douglas.DouglasPeuckerReduction(pts, 0.5);
            Assert.AreEqual(2, reduced.Count);
            Assert.AreEqual(new PointF(0, 0), reduced[0]);
            Assert.AreEqual(new PointF(5, 0), reduced[reduced.Count - 1]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Reduction_KeepsSignificantVertex()
        {
            // A clear peak in the middle must be preserved.
            var pts = new List<PointF>
            {
                new PointF(0, 0), new PointF(5, 10), new PointF(10, 0),
            };
            var reduced = Douglas.DouglasPeuckerReduction(pts, 1.0);
            CollectionAssert.Contains(reduced, new PointF(5, 10));
            Assert.AreEqual(3, reduced.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Reduction_FewerThanThreePoints_ReturnedUnchanged()
        {
            var pts = new List<PointF> { new PointF(0, 0), new PointF(1, 1) };
            var reduced = Douglas.DouglasPeuckerReduction(pts, 0.5);
            Assert.AreEqual(2, reduced.Count);
        }
    }
}
