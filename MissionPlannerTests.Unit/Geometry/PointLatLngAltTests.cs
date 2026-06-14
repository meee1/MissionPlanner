using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for <see cref="PointLatLngAlt"/> geographic math
    /// (great-circle distance, projection of a point, bearing, UTM zoning).
    /// Values are deterministic and offline.
    /// </summary>
    [TestClass]
    public class PointLatLngAltTests
    {
        // Canberra-ish reference point used across ArduPilot autotest.
        private static readonly PointLatLngAlt Home = new PointLatLngAlt(-35.363261, 149.165230, 584, "home");

        private const double MetersTolerance = 0.5; // half a metre
        private const double DegTolerance = 1e-6;

        [TestMethod]
        [TestCategory("Unit")]
        public void GetDistance_SamePoint_IsZero()
        {
            Assert.AreEqual(0.0, Home.GetDistance(new PointLatLngAlt(Home)), 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetDistance_OneDegreeLatitude_IsAboutOneeleventhKm()
        {
            // 1 degree of latitude ~= 111.195 km on a 6371 km sphere (haversine).
            var a = new PointLatLngAlt(0, 0);
            var b = new PointLatLngAlt(1, 0);
            double d = a.GetDistance(b);
            Assert.AreEqual(111194.9, d, 5.0);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetDistance_IsSymmetric()
        {
            var a = new PointLatLngAlt(-35.0, 149.0);
            var b = new PointLatLngAlt(-35.1, 149.2);
            Assert.AreEqual(a.GetDistance(b), b.GetDistance(a), 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Newpos_Then_GetDistance_RoundTripsToRequestedRange()
        {
            // Project 250 m due east and confirm the distance back is ~250 m.
            const double range = 250.0;
            var moved = Home.newpos(90.0, range);
            Assert.AreEqual(range, Home.GetDistance(moved), MetersTolerance);
            // Altitude and tag are carried through.
            Assert.AreEqual(Home.Alt, moved.Alt, 1e-9);
            Assert.AreEqual(Home.Tag, moved.Tag);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Newpos_North_IncreasesLatitudeOnly()
        {
            var moved = Home.newpos(0.0, 1000.0);
            Assert.IsTrue(moved.Lat > Home.Lat, "northward move must increase latitude");
            Assert.AreEqual(Home.Lng, moved.Lng, 1e-6, "northward move must not change longitude");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetBearing_DueNorth_IsZero()
        {
            var north = new PointLatLngAlt(Home.Lat + 0.1, Home.Lng);
            Assert.AreEqual(0.0, Home.GetBearing(north), 1e-3);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetBearing_DueEast_IsNinety()
        {
            var east = new PointLatLngAlt(Home.Lat, Home.Lng + 0.1);
            Assert.AreEqual(90.0, Home.GetBearing(east), 1e-1);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetBearing_IsAlwaysNormalisedZeroTo360()
        {
            var west = new PointLatLngAlt(Home.Lat, Home.Lng - 0.1);
            double b = Home.GetBearing(west);
            Assert.IsTrue(b >= 0.0 && b < 360.0, $"bearing {b} not in [0,360)");
            Assert.AreEqual(270.0, b, 1e-1);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GpsOffset_EastNorth_MatchesNewpos()
        {
            // gps_offset(east, north) is documented as a metre offset helper.
            var byOffset = Home.gps_offset(0, 500); // 500 m north
            var byNewpos = Home.newpos(0, 500);
            Assert.AreEqual(byNewpos.Lat, byOffset.Lat, DegTolerance);
            Assert.AreEqual(byNewpos.Lng, byOffset.Lng, DegTolerance);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetUTMZone_KnownLocations()
        {
            // Canberra is in UTM zone 55 (southern hemisphere -> negative here).
            Assert.AreEqual(-55, Home.GetUTMZone());
            // London ~ zone 30 N.
            Assert.AreEqual(30, new PointLatLngAlt(51.5, -0.12).GetUTMZone());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Equals_And_HashCode_AreConsistent()
        {
            var a = new PointLatLngAlt(-35.0, 149.0, 100, "wp");
            var b = new PointLatLngAlt(-35.0, 149.0, 100, "wp");
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }
    }
}
