using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Controls.Waypoints;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for <see cref="Spline2"/> position-vector conversions
    /// (the NED-from-home centimetre frame used by waypoint navigation): a
    /// location maps to a vector and back, and home maps to the origin.
    /// </summary>
    [TestClass]
    public class Spline2Tests
    {
        private static readonly PointLatLngAlt Home = new PointLatLngAlt(-35.363261, 149.165230, 584);

        [TestMethod]
        [TestCategory("Unit")]
        public void Home_MapsTo_OriginVector()
        {
            var spline = new Spline2(Home);
            var v = spline.pv_location_to_vector(new PointLatLngAlt(Home.Lat, Home.Lng, Home.Alt));

            Assert.AreEqual(0.0, v.xd, 1.0); // within a centimetre-scale tolerance
            Assert.AreEqual(0.0, v.yd, 1.0);
            // z is altitude in cm
            Assert.AreEqual(Home.Alt * 100.0, v.zd, 1.0);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LocationToVector_ThenBack_RoundTrips()
        {
            var spline = new Spline2(Home);
            var loc = new PointLatLngAlt(-35.360000, 149.167000, 620);

            var vec = spline.pv_location_to_vector(loc);
            var back = spline.pv_vector_to_location(vec);

            // The conversion goes through float degE7 and int truncation, so allow ~1e-5 deg.
            Assert.AreEqual(loc.Lat, back.Lat, 1e-5);
            Assert.AreEqual(loc.Lng, back.Lng, 1e-5);
            Assert.AreEqual(loc.Alt, back.Alt, 0.5);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void NorthwardLocation_HasPositiveNorthComponent()
        {
            var spline = new Spline2(Home);
            // A point due north of home (higher latitude) should have +X (north) in the vector.
            var north = new PointLatLngAlt(Home.Lat + 0.001, Home.Lng, Home.Alt);
            var v = spline.pv_location_to_vector(north);

            Assert.IsTrue(v.xd > 0, $"expected positive north component, got {v.xd}");
            Assert.AreEqual(0.0, v.yd, 1.0, "no east displacement expected");
        }
    }
}
