using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// MAVState-driven derived telemetry: CurrentState (mav.cs) computes many
    /// values from raw fields. These exercise that logic via a headless MAVState
    /// fixture. (The Unit tier only covers the static unit-multiplier helpers.)
    /// </summary>
    [TestClass]
    public class CurrentStateDerivedTests
    {
        // cs taken from a MAVState, with metric (identity) unit multipliers.
        private static CurrentState NewCs()
        {
            CurrentState.multiplierdist = 1f;
            CurrentState.multiplierspeed = 1f;
            CurrentState.multiplieralt = 1f;
            return new MAVState(null, 0, 0).cs;
        }

        [TestMethod]
        [TestCategory("App")]
        public void Location_ComposesLatLngAlt()
        {
            var cs = NewCs();
            cs.lat = 12.34;
            cs.lng = 56.78;
            cs.altasl = 100;

            var loc = cs.Location;
            Assert.AreEqual(12.34, loc.Lat, 1e-9);
            Assert.AreEqual(56.78, loc.Lng, 1e-9);
            Assert.AreEqual(100, loc.Alt, 1e-6);
        }

        [TestMethod]
        [TestCategory("App")]
        public void TimeInAirMinSec_FormatsAsMinutesDotSeconds()
        {
            var cs = NewCs();
            cs.timeInAir = 125f; // 2 min 5 s -> 2.05
            Assert.AreEqual(2.05f, cs.timeInAirMinSec, 1e-4);

            cs.timeInAir = 90f;  // 1 min 30 s -> 1.30
            Assert.AreEqual(1.30f, cs.timeInAirMinSec, 1e-4);
        }

        [TestMethod]
        [TestCategory("App")]
        public void Tot_IsWaypointDistanceOverGroundspeed()
        {
            var cs = NewCs();
            cs.wp_dist = 100f;
            cs.groundspeed = 10f;
            Assert.AreEqual(10, cs.tot);

            cs.groundspeed = 0f;
            Assert.AreEqual(0, cs.tot, "guards divide-by-zero");
        }

        [TestMethod]
        [TestCategory("App")]
        public void DistToHome_EquirectangularFromTrackerLocation()
        {
            var cs = NewCs();
            cs.TrackerLocation = new PointLatLngAlt(-35.0, 149.0, 0);
            cs.lat = -35.0 + 0.001; // ~111 m north
            cs.lng = 149.0;

            Assert.AreEqual(111.32f, cs.DistToHome, 0.5f);
        }
    }
}
