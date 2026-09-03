using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Tests for LocationProjection.Project (ExtLibs/Utilities/LocationProjection.cs):
    /// dead-reckons a position forward from a velocity vector over a time delta.
    /// </summary>
    [TestClass]
    public class LocationProjectionTests
    {
        private static readonly PointLatLngAlt Origin = new PointLatLngAlt(-35.0, 149.0, 100);
        private static readonly DateTime T0 = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        [TestCategory("Unit")]
        public void ZeroVelocity_ReturnsSamePosition()
        {
            var p = LocationProjection.Project(Origin, new Vector3(0, 0, 0), T0, T0.AddSeconds(5));
            Assert.AreEqual(0.0, Origin.GetDistance(p), 0.01);
            Assert.AreEqual(Origin.Alt, p.Alt, 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ZeroTimeDelta_ReturnsSamePosition()
        {
            var p = LocationProjection.Project(Origin, new Vector3(10, 5, 2), T0, T0);
            Assert.AreEqual(0.0, Origin.GetDistance(p), 0.01);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Distance_EqualsSpeedTimesTime()
        {
            var v = new Vector3(8, 0, 0); // 8 m/s
            var p1 = LocationProjection.Project(Origin, v, T0, T0.AddSeconds(1));
            var p2 = LocationProjection.Project(Origin, v, T0, T0.AddSeconds(2));
            Assert.AreEqual(8.0, Origin.GetDistance(p1), 0.5);
            Assert.AreEqual(16.0, Origin.GetDistance(p2), 0.5);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void VelocityDirection_MapsToBearing()
        {
            // bearing = atan2(vy, vx); Mission Planner bearing 0 = North, 90 = East.
            var north = LocationProjection.Project(Origin, new Vector3(10, 0, 0), T0, T0.AddSeconds(1));
            Assert.IsTrue(north.Lat > Origin.Lat, "x-velocity -> bearing 0 -> north");
            Assert.AreEqual(Origin.Lng, north.Lng, 1e-4);

            var east = LocationProjection.Project(Origin, new Vector3(0, 10, 0), T0, T0.AddSeconds(1));
            Assert.IsTrue(east.Lng > Origin.Lng, "y-velocity -> bearing 90 -> east");
            Assert.AreEqual(Origin.Lat, east.Lat, 1e-4);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void VerticalVelocity_ChangesAltitude()
        {
            var p = LocationProjection.Project(Origin, new Vector3(0, 0, 5), T0, T0.AddSeconds(2));
            Assert.AreEqual(Origin.Alt + 10.0, p.Alt, 0.01); // 5 m/s * 2 s
        }
    }
}
