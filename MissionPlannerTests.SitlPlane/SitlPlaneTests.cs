using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Sitl
{
    /// <summary>
    /// Fixed-wing (ArduPlane) coverage: confirms the harness connects to a
    /// different vehicle type and that core telemetry/parameters work. Runs in
    /// its own SITL instance; the instance/port is released by the copter class
    /// first (ClassCleanup is EndOfClass), so only one sim runs at a time.
    ///
    /// Skips (inconclusive) when no ArduPlane binary is available.
    /// </summary>
    [TestClass]
    public class SitlPlaneTests
    {
        private static SitlFixture _sitl;

        private static MAVLinkInterface Mav => _sitl.Mav;
        private static byte Sysid => _sitl.Sysid;
        private static byte Compid => _sitl.Compid;

        [ClassInitialize]
        public static void StartSitl(TestContext _)
        {
            if (SitlFixture.VehicleAvailable("arduplane"))
                _sitl = SitlFixture.StartPlane();
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static void StopSitl() => _sitl?.Dispose();

        [TestInitialize]
        public void RequireSitl()
        {
            if (_sitl == null)
                Assert.Inconclusive("arduplane binary not available; skipping fixed-wing SITL test.");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task Plane_Heartbeat_ReportsFixedWing()
        {
            var hb = await Mav.getHeartBeatAsync();
            Assert.IsNotNull(hb);
            var beat = hb.ToStructure<MAVLink.mavlink_heartbeat_t>();
            Assert.AreEqual((byte)MAVLink.MAV_TYPE.FIXED_WING, beat.type, "expected a fixed-wing vehicle type");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task Plane_ParameterDownload_ReturnsManyParameters()
        {
            var paramlist = await Mav.getParamListAsync(Sysid, Compid);
            Assert.IsTrue(paramlist.Count > 100, $"expected >100 params, got {paramlist.Count}");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task Plane_Gps_AcquiresFix()
        {
            byte fix = 0;
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
            while (DateTime.UtcNow < deadline)
            {
                var msg = await Mav.readPacketAsync();
                if (msg == null) continue;
                if (msg.msgid == (uint)MAVLink.MAVLINK_MSG_ID.GPS_RAW_INT)
                {
                    fix = msg.ToStructure<MAVLink.mavlink_gps_raw_int_t>().fix_type;
                    if (fix >= 3) break;
                }
            }
            Assert.IsTrue(fix >= 3, $"expected a 3D GPS fix (>=3), got fix_type {fix}");
        }
    }
}
