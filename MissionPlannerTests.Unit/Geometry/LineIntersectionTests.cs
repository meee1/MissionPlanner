using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for segment intersection
    /// (<see cref="ImageProjection.FindLineIntersection"/>), used by footprint
    /// and coverage geometry. Returns the intersection point, or an empty
    /// <see cref="PointF"/> (0,0) when the segments are parallel or don't cross.
    /// </summary>
    [TestClass]
    public class LineIntersectionTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void CrossingSegments_IntersectAtMidpoint()
        {
            var p = ImageProjection.FindLineIntersection(
                new PointF(0, 0), new PointF(10, 10),
                new PointF(0, 10), new PointF(10, 0));

            Assert.AreEqual(5f, p.X, 1e-4);
            Assert.AreEqual(5f, p.Y, 1e-4);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ParallelSegments_ReturnEmptyPoint()
        {
            var p = ImageProjection.FindLineIntersection(
                new PointF(0, 0), new PointF(10, 0),
                new PointF(0, 5), new PointF(10, 5));

            Assert.AreEqual(new PointF(), p); // denom == 0 -> (0,0)
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void NonOverlappingSegments_ReturnEmptyPoint()
        {
            // Lines would cross if extended, but the segments themselves do not.
            var p = ImageProjection.FindLineIntersection(
                new PointF(0, 0), new PointF(1, 0),
                new PointF(5, 5), new PointF(5, 6));

            Assert.AreEqual(new PointF(), p);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TouchingAtEndpoint_ReturnsThatPoint()
        {
            var p = ImageProjection.FindLineIntersection(
                new PointF(0, 0), new PointF(5, 5),
                new PointF(5, 5), new PointF(10, 0));

            Assert.AreEqual(5f, p.X, 1e-4);
            Assert.AreEqual(5f, p.Y, 1e-4);
        }
    }
}
