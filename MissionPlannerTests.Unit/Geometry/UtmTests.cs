using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for UTM projection round-trips
    /// (<see cref="PointLatLngAlt.ToUTM()"/> / <see cref="PointLatLngAlt.FromUTM"/>)
    /// and <see cref="utmpos"/> conversions and metric distance. These back the
    /// survey-grid and georeferencing maths, which work in projected metres.
    /// </summary>
    [TestClass]
    public class UtmTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ToUTM_ThenFromUTM_RoundTrips_SouthernHemisphere()
        {
            var p = new PointLatLngAlt(-35.363261, 149.165230);
            int zone = p.GetUTMZone();
            double[] utm = p.ToUTM();                       // {easting, northing}
            var back = PointLatLngAlt.FromUTM(zone, utm[0], utm[1]);

            Assert.AreEqual(p.Lat, back.Lat, 1e-6);
            Assert.AreEqual(p.Lng, back.Lng, 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToUTM_ThenFromUTM_RoundTrips_NorthernHemisphere()
        {
            var p = new PointLatLngAlt(51.501, -0.1425); // London
            int zone = p.GetUTMZone();
            double[] utm = p.ToUTM();
            var back = PointLatLngAlt.FromUTM(zone, utm[0], utm[1]);

            Assert.AreEqual(p.Lat, back.Lat, 1e-6);
            Assert.AreEqual(p.Lng, back.Lng, 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void UtmPos_FromLatLng_ThenToLLA_RoundTrips()
        {
            var p = new PointLatLngAlt(-35.363261, 149.165230);
            var utm = new utmpos(p);
            PointLatLngAlt back = utm.ToLLA();

            Assert.AreEqual(p.Lat, back.Lat, 1e-6);
            Assert.AreEqual(p.Lng, back.Lng, 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void UtmPos_GetDistance_MatchesGreatCircleForShortRange()
        {
            // Over a few hundred metres, planar UTM distance and the haversine
            // distance should agree closely.
            var a = new PointLatLngAlt(-35.363261, 149.165230);
            var b = a.newpos(45, 300); // 300 m to the NE

            var ua = new utmpos(a);
            var ub = new utmpos(b);

            double planar = ua.GetDistance(ub);
            double greatCircle = a.GetDistance(b);

            Assert.AreEqual(greatCircle, planar, 1.0); // within a metre
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void UtmPos_SamePoint_DistanceIsZero()
        {
            var u = new utmpos(new PointLatLngAlt(-35.0, 149.0));
            Assert.AreEqual(0.0, u.GetDistance(u), 1e-6);
        }
    }
}
