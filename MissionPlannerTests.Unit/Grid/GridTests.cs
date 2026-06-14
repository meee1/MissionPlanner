using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Grid
{
    /// <summary>
    /// Regression tests for survey grid generation (<see cref="MissionPlanner.Utilities.Grid.CreateGrid"/>).
    /// Survey output is easy to regress silently, so we pin deterministic
    /// structural properties for a fixed square polygon and parameter set.
    /// </summary>
    [TestClass]
    public class GridTests
    {
        // ~400 m square near Canberra (CCW), altitude unused for geometry checks.
        private static List<PointLatLngAlt> Square()
        {
            return new List<PointLatLngAlt>
            {
                new PointLatLngAlt(-35.36500, 149.16500, 0, ""),
                new PointLatLngAlt(-35.36500, 149.16940, 0, ""),
                new PointLatLngAlt(-35.36140, 149.16940, 0, ""),
                new PointLatLngAlt(-35.36140, 149.16500, 0, ""),
            };
        }

        private static List<PointLatLngAlt> RunGrid(double laneSpacing, double angle)
        {
            var poly = Square();
            return MissionPlanner.Utilities.Grid.CreateGrid(
                polygon: poly,
                altitude: 50,
                distance: laneSpacing,
                spacing: 0,
                angle: angle,
                overshoot1: 0,
                overshoot2: 0,
                startpos: MissionPlanner.Utilities.Grid.StartPosition.Home,
                shutter: false,
                minLaneSeparation: 0,
                leadin1: 0,
                leadin2: 0,
                HomeLocation: poly[0]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateGrid_EmptyPolygon_ReturnsEmpty()
        {
            var result = MissionPlanner.Utilities.Grid.CreateGrid(
                new List<PointLatLngAlt>(), 50, 30, 0, 0, 0, 0,
                MissionPlanner.Utilities.Grid.StartPosition.Home, false, 0, 0, 0, PointLatLngAlt.Zero);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateGrid_ProducesWaypoints()
        {
            var result = RunGrid(laneSpacing: 40, angle: 0);
            Assert.IsTrue(result.Count > 0, "grid should produce waypoints for a non-empty polygon");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateGrid_TighterSpacing_ProducesMoreWaypoints()
        {
            int coarse = RunGrid(laneSpacing: 80, angle: 0).Count;
            int fine = RunGrid(laneSpacing: 20, angle: 0).Count;
            Assert.IsTrue(fine > coarse,
                $"finer lane spacing must yield more waypoints (coarse={coarse}, fine={fine})");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateGrid_WaypointsStayNearPolygon()
        {
            var poly = Square();
            var result = RunGrid(laneSpacing: 40, angle: 0);

            double minLat = poly.Min(p => p.Lat), maxLat = poly.Max(p => p.Lat);
            double minLng = poly.Min(p => p.Lng), maxLng = poly.Max(p => p.Lng);
            // Allow a generous margin (~1 lane, ~0.001 deg) for turn-around geometry.
            const double margin = 0.001;

            foreach (var p in result)
            {
                Assert.IsTrue(p.Lat >= minLat - margin && p.Lat <= maxLat + margin,
                    $"lat {p.Lat} outside polygon+margin");
                Assert.IsTrue(p.Lng >= minLng - margin && p.Lng <= maxLng + margin,
                    $"lng {p.Lng} outside polygon+margin");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateGrid_IsDeterministic()
        {
            // Same inputs must yield identical output, run to run.
            var a = RunGrid(laneSpacing: 40, angle: 0);
            var b = RunGrid(laneSpacing: 40, angle: 0);
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Lat, b[i].Lat, 1e-12);
                Assert.AreEqual(a[i].Lng, b[i].Lng, 1e-12);
            }
        }
    }
}
