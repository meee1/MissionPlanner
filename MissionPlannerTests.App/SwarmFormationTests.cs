using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner;
using MissionPlanner.Swarm;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// MAVState-driven swarm logic: FormationControl.getOffsetFromLeader projects
    /// the leader and follower lat/lng into UTM and returns the follower's offset
    /// rotated into the leader's heading frame. It reads only leader.cs/mav.cs and
    /// holds no instance state, so we drive it with lightweight MAVState fixtures.
    /// </summary>
    [TestClass]
    public class SwarmFormationTests
    {
        // MAVState(null, 0, 0): parent is never dereferenced and sysid/compid 0
        // skips the Proximity sub-object - a minimal, headless telemetry fixture.
        private static MAVState MavAt(double lat, double lng, float yaw = 0)
        {
            var m = new MAVState(null, 0, 0);
            m.cs.lat = lat;
            m.cs.lng = lng;
            m.cs.yaw = yaw;
            return m;
        }

        // getOffsetFromLeader uses no instance fields, so allocate the control
        // without running its WinForms constructor.
        private static FormationControl Control()
        {
            var fc = (FormationControl)RuntimeHelpers.GetUninitializedObject(typeof(FormationControl));
            GC.SuppressFinalize(fc); // it's a half-initialised Form; don't finalise it
            return fc;
        }

        [TestMethod]
        [TestCategory("App")]
        public void CoincidentMavs_GiveZeroOffset()
        {
            var leader = MavAt(-35.000, 149.000, yaw: 0);
            var follower = MavAt(-35.000, 149.000, yaw: 0);

            var off = Control().getOffsetFromLeader(leader, follower);

            Assert.AreEqual(0.0, off.length(), 0.5);
        }

        [TestMethod]
        [TestCategory("App")]
        public void FollowerNorthOfLeader_OffsetAlongX_WithZeroYaw()
        {
            // +0.001 deg latitude is ~111 m north; same longitude -> no easting.
            var leader = MavAt(-35.000, 149.000, yaw: 0);
            var follower = MavAt(-35.000 + 0.001, 149.000, yaw: 0);

            var off = Control().getOffsetFromLeader(leader, follower);

            Assert.AreEqual(111.0, off.x, 3.0, "north distance maps onto x at zero yaw");
            Assert.AreEqual(0.0, off.y, 3.0, "no easting component");
            Assert.AreEqual(0.0, off.z);
        }

        [TestMethod]
        [TestCategory("App")]
        public void LeaderYaw_RotatesOffset_PreservingLength()
        {
            var follower = MavAt(-35.000 + 0.001, 149.000, yaw: 0);
            var off0 = Control().getOffsetFromLeader(MavAt(-35.000, 149.000, 0), follower);
            var off90 = Control().getOffsetFromLeader(MavAt(-35.000, 149.000, 90), follower);

            // The final step rotates by heading = -yaw, so magnitude is preserved
            // and (x,y) at yaw=90 becomes (y, -x) of the yaw=0 result.
            Assert.AreEqual(off0.length(), off90.length(), 0.5, "rotation preserves magnitude");
            Assert.AreEqual(off0.y, off90.x, 2.0);
            Assert.AreEqual(-off0.x, off90.y, 2.0);
        }
    }
}
