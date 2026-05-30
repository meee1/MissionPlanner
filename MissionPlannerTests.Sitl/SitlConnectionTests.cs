using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.ArduPilot;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Sitl
{
    /// <summary>
    /// End-to-end integration tests against a real ArduPilot SITL instance,
    /// driving the same <see cref="MAVLinkInterface"/> API the GUI uses.
    ///
    /// These run only when a SITL binary is available (SITL_BIN_DIR); otherwise
    /// each test is marked inconclusive so the suite stays green everywhere.
    /// One SITL instance is shared across the class for speed.
    /// </summary>
    [TestClass]
    public class SitlConnectionTests
    {
        private static SitlFixture _sitl;

        [ClassInitialize]
        public static void StartSitl(TestContext _)
        {
            if (SitlFixture.IsAvailable)
                _sitl = SitlFixture.StartCopter();
        }

        [ClassCleanup]
        public static void StopSitl() => _sitl?.Dispose();

        [TestInitialize]
        public void RequireSitl()
        {
            if (_sitl == null)
                Assert.Inconclusive("SITL_BIN_DIR not set or binary missing; skipping SITL integration test.");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task Heartbeat_IsReceived()
        {
            var hb = await _sitl.Mav.getHeartBeatAsync();
            Assert.IsNotNull(hb);
            Assert.IsTrue(_sitl.Sysid > 0, "expected a non-zero system id after heartbeat");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task ParameterDownload_ReturnsManyParameters()
        {
            var paramlist = await _sitl.Mav.getParamListAsync(_sitl.Sysid, _sitl.Compid);
            // ArduCopter exposes hundreds of parameters; a healthy download is well over 100.
            Assert.IsTrue(paramlist.Count > 100, $"expected >100 params, got {paramlist.Count}");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task SetParam_ThenGetParam_RoundTrips()
        {
            // RTL_ALT exists on copter; round-trip a distinctive value (in cm).
            const string name = "RTL_ALT";
            const double value = 2500;

            bool set = await _sitl.Mav.setParamAsync(_sitl.Sysid, _sitl.Compid, name, value);
            Assert.IsTrue(set, $"failed to set {name}");

            float readback = await _sitl.Mav.GetParamAsync(_sitl.Sysid, _sitl.Compid, name);
            Assert.AreEqual(value, readback, 1e-3);
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task MissionUpload_ThenDownload_RoundTrips()
        {
            var mission = new List<Locationwp>
            {
                // index 0 is home in Mission Planner's convention
                new Locationwp().Set(-35.363261, 149.165230, 0, (ushort)MAVLink.MAV_CMD.WAYPOINT),
                new Locationwp().Set(0, 0, 20, (ushort)MAVLink.MAV_CMD.TAKEOFF),
                new Locationwp().Set(-35.359800, 149.164300, 30, (ushort)MAVLink.MAV_CMD.WAYPOINT),
                new Locationwp().Set(-35.360000, 149.167000, 40, (ushort)MAVLink.MAV_CMD.WAYPOINT),
            };

            await mav_mission.upload(_sitl.Mav, _sitl.Sysid, _sitl.Compid,
                MAVLink.MAV_MISSION_TYPE.MISSION, mission);

            var downloaded = await mav_mission.download(_sitl.Mav, _sitl.Sysid, _sitl.Compid,
                MAVLink.MAV_MISSION_TYPE.MISSION);

            Assert.AreEqual(mission.Count, downloaded.Count, "round-tripped mission item count");

            // The two navigation waypoints (indices 2 and 3) must survive unchanged.
            foreach (int i in new[] { 2, 3 })
            {
                Assert.AreEqual((ushort)MAVLink.MAV_CMD.WAYPOINT, downloaded[i].id);
                Assert.AreEqual(mission[i].lat, downloaded[i].lat, 1e-5, $"lat mismatch at {i}");
                Assert.AreEqual(mission[i].lng, downloaded[i].lng, 1e-5, $"lng mismatch at {i}");
            }
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task HomePosition_IsReported()
        {
            var home = await _sitl.Mav.getHomePositionAsync(_sitl.Sysid, _sitl.Compid);
            // Home should be near the launch location we passed to SITL.
            Assert.AreEqual(-35.363261, home.lat, 0.01, "home latitude near launch point");
            Assert.AreEqual(149.165230, home.lng, 0.01, "home longitude near launch point");
        }
    }
}
