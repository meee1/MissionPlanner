using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Mission
{
    /// <summary>
    /// Regression tests for <see cref="Locationwp"/> &lt;-&gt; MAVLink mission item
    /// conversions (the implicit operators / <c>Convert</c> path used on every
    /// mission upload and download). These guard the lat/lng 1e7 scaling that
    /// distinguishes location commands from non-location commands.
    /// </summary>
    [TestClass]
    public class LocationwpTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void RoundTrip_Through_MissionItemInt_PreservesLocation()
        {
            var wp = new Locationwp().Set(-35.363261, 149.165230, 100, (ushort)MAVLink.MAV_CMD.WAYPOINT);
            wp.p1 = 5;
            wp.p2 = 6;
            wp.p3 = 7;
            wp.p4 = 8;

            MAVLink.mavlink_mission_item_int_t item = wp;   // implicit, scales lat/lng by 1e7
            Locationwp back = item;                          // implicit, scales back

            // int_t stores lat/lng as int32 degE7, so precision is ~1e-7 deg.
            Assert.AreEqual(wp.lat, back.lat, 1e-6);
            Assert.AreEqual(wp.lng, back.lng, 1e-6);
            Assert.AreEqual(wp.alt, back.alt, 1e-3);
            Assert.AreEqual(wp.id, back.id);
            Assert.AreEqual(wp.p1, back.p1, 1e-6);
            Assert.AreEqual(wp.p4, back.p4, 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MissionItemInt_ScalesLocationCommandsByE7()
        {
            var wp = new Locationwp().Set(-35.363261, 149.165230, 100, (ushort)MAVLink.MAV_CMD.WAYPOINT);
            MAVLink.mavlink_mission_item_int_t item = wp;

            Assert.AreEqual((int)(-35.363261 * 1e7), item.x);
            Assert.AreEqual((int)(149.165230 * 1e7), item.y);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void IsLocationCommand_Waypoint_IsTrue_DoJump_IsFalse()
        {
            Assert.IsTrue(Locationwp.isLocationCommand((ushort)MAVLink.MAV_CMD.WAYPOINT));
            Assert.IsFalse(Locationwp.isLocationCommand((ushort)MAVLink.MAV_CMD.DO_JUMP));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void NonLocationCommand_IsNotScaled_InMissionItemInt()
        {
            // DO_JUMP carries counts in params, not a location; x/y must not be E7-scaled.
            var wp = new Locationwp { id = (ushort)MAVLink.MAV_CMD.DO_JUMP, lat = 2, lng = 3, p1 = 1, p2 = 1 };
            MAVLink.mavlink_mission_item_int_t item = wp;
            Assert.AreEqual(2, item.x);
            Assert.AreEqual(3, item.y);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RoundTrip_Through_MissionFileItem_PreservesCoreFields()
        {
            var wp = new Locationwp().Set(-35.0, 149.0, 50, (ushort)MAVLink.MAV_CMD.WAYPOINT);
            MissionFile.Item item = wp;     // implicit
            Locationwp back = item;          // implicit

            Assert.AreEqual(wp.id, back.id);
            Assert.AreEqual(wp.lat, back.lat, 1e-9);
            Assert.AreEqual(wp.lng, back.lng, 1e-9);
            Assert.AreEqual(wp.alt, back.alt, 1e-3);
        }
    }
}
