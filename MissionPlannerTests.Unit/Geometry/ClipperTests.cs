using System;
using System.Collections.Generic;
using ClipperLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Unit.Geometry
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    /// <summary>
    /// Regression tests for polygon clipping (<see cref="Clipper"/>), used for
    /// survey/exclusion-zone geometry. Two overlapping 10x10 squares give a
    /// known intersection and union area.
    /// </summary>
    [TestClass]
    public class ClipperTests
    {
        private static Path Square(long x, long y, long size) => new Path
        {
            new IntPoint(x, y),
            new IntPoint(x + size, y),
            new IntPoint(x + size, y + size),
            new IntPoint(x, y + size),
        };

        private static Paths Clip(ClipType type, Path subject, Path clip)
        {
            var c = new Clipper();
            c.AddPath(subject, PolyType.ptSubject, true);
            c.AddPath(clip, PolyType.ptClip, true);
            var solution = new Paths();
            Assert.IsTrue(c.Execute(type, solution), "Clipper.Execute should succeed");
            return solution;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Intersection_OfOverlappingSquares_HasOverlapArea()
        {
            // (0,0)-(10,10) intersect (5,5)-(15,15) -> 5x5 square, area 25.
            var solution = Clip(ClipType.ctIntersection, Square(0, 0, 10), Square(5, 5, 10));

            Assert.AreEqual(1, solution.Count);
            Assert.AreEqual(25.0, Math.Abs(Clipper.Area(solution[0])), 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Union_OfOverlappingSquares_HasCombinedArea()
        {
            // area = 100 + 100 - 25 overlap = 175
            var solution = Clip(ClipType.ctUnion, Square(0, 0, 10), Square(5, 5, 10));

            double total = 0;
            foreach (var p in solution) total += Math.Abs(Clipper.Area(p));
            Assert.AreEqual(175.0, total, 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Intersection_OfDisjointSquares_IsEmpty()
        {
            var solution = Clip(ClipType.ctIntersection, Square(0, 0, 10), Square(100, 100, 10));
            Assert.AreEqual(0, solution.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Area_OfUnitSquare_Is100()
        {
            Assert.AreEqual(100.0, Math.Abs(Clipper.Area(Square(0, 0, 10))), 1e-6);
        }
    }
}
