using System;
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
    /// driving the same <see cref="MAVLinkInterface"/> API the GUI uses:
    /// heartbeat, parameter download / set-get, mission upload/download, home
    /// position, and a full GUIDED arm + takeoff climb.
    ///
    /// One SITL instance and one connection are shared across all tests (an
    /// external sim is expensive to spin up, and a single MAVLinkInterface per
    /// process is the supported usage). The tests are independent: the read-only
    /// ones do not care whether the vehicle is armed or flying. When no SITL
    /// binary is available every test is marked inconclusive.
    /// </summary>
    [TestClass]
    public class SitlTests
    {
        private const uint CopterGuidedMode = 4;          // ArduCopter GUIDED custom_mode
        private const byte MavModeFlagSafetyArmed = 128;  // MAV_MODE_FLAG.SAFETY_ARMED

        private static SitlFixture _sitl;

        private static MAVLinkInterface Mav => _sitl.Mav;
        private static byte Sysid => _sitl.Sysid;
        private static byte Compid => _sitl.Compid;

        [ClassInitialize]
        public static void StartSitl(TestContext _)
        {
            if (!SitlFixture.IsAvailable)
                return;

            _sitl = SitlFixture.StartCopter();
            // Download the parameter list once, as the GUI does on connect, so
            // setParamAsync can resolve parameters by name.
            Mav.getParamListAsync(Sysid, Compid).GetAwaiter().GetResult();
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
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
            var hb = await Mav.getHeartBeatAsync();
            Assert.IsNotNull(hb);
            Assert.IsTrue(Sysid > 0, "expected a non-zero system id after heartbeat");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task ParameterDownload_ReturnsManyParameters()
        {
            var paramlist = await Mav.getParamListAsync(Sysid, Compid);
            // ArduCopter exposes hundreds of parameters; a healthy download is well over 100.
            Assert.IsTrue(paramlist.Count > 100, $"expected >100 params, got {paramlist.Count}");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task SetParam_ThenGetParam_RoundTrips()
        {
            var paramlist = await Mav.getParamListAsync(Sysid, Compid);
            Assert.IsTrue(paramlist.Count > 0, "no parameters downloaded");

            // Choose an existing parameter rather than hard-coding a name, since
            // ArduPilot renames/regroups parameters across versions. Prefer a
            // historically stable, writable one; fall back to whatever is present.
            string name = new[] { "WPNAV_SPEED", "PILOT_SPEED_UP", "ANGLE_MAX" }
                              .FirstOrDefault(paramlist.ContainsKey)
                          ?? paramlist.First().Name;

            double original = paramlist[name].Value;

            // Round-trip the value through a real PARAM_SET / PARAM_VALUE exchange.
            // force:true ensures the set is sent even when the value is unchanged,
            // so the value stays in range and we exercise the full wire path.
            bool set = await Mav.setParamAsync(Sysid, Compid, name, original, force: true);
            Assert.IsTrue(set, $"failed to set {name}");

            float readback = await Mav.GetParamAsync(Sysid, Compid, name);
            Assert.AreEqual(original, readback, Math.Max(1e-3, Math.Abs(original) * 1e-4),
                $"readback of {name} did not match");
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

            await mav_mission.upload(Mav, Sysid, Compid, MAVLink.MAV_MISSION_TYPE.MISSION, mission);

            var downloaded = await mav_mission.download(Mav, Sysid, Compid, MAVLink.MAV_MISSION_TYPE.MISSION);

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
            var home = await Mav.getHomePositionAsync(Sysid, Compid);
            // Home should be near the launch location we passed to SITL.
            Assert.AreEqual(-35.363261, home.lat, 0.01, "home latitude near launch point");
            Assert.AreEqual(149.165230, home.lng, 0.01, "home longitude near launch point");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task ArmInGuided_Takeoff_ReachesAltitude()
        {
            const double targetAlt = 10.0;

            // 1. Get into GUIDED. GUIDED only engages once the EKF has a position
            //    estimate, so a successful switch also means we are ready to arm.
            bool guided = await EnsureGuided(TimeSpan.FromSeconds(120));
            Assert.IsTrue(guided, "vehicle did not enter GUIDED (no position estimate?) within timeout");

            // 2. Arm (retry while prearm settles), then confirm via heartbeat.
            bool armed = await TryArm(TimeSpan.FromSeconds(60));
            Assert.IsTrue(armed, "vehicle did not arm in GUIDED within the timeout");
            Assert.IsTrue(await WaitForArmedState(true, TimeSpan.FromSeconds(15)),
                "arm command returned success but heartbeat never reported armed");

            // 3. Command takeoff (param7 = altitude in m), retrying in case the
            //    first command races the armed-state transition.
            bool accepted = await TakeoffWithRetries(targetAlt, TimeSpan.FromSeconds(30));
            Assert.IsTrue(accepted, "takeoff command was not accepted");

            // 4. Watch live telemetry until the vehicle reaches ~90% of target.
            double reached = await WaitForAltitude(targetAlt * 0.9, TimeSpan.FromSeconds(120));
            Assert.IsTrue(reached >= targetAlt * 0.9,
                $"expected to climb to >= {targetAlt * 0.9:0.0} m, reached {reached:0.0} m");
        }

        // --- GUIDED flight helpers --------------------------------------------

        /// <summary>Repeatedly request GUIDED until a heartbeat confirms it engaged.</summary>
        private static async Task<bool> EnsureGuided(TimeSpan timeout)
        {
            // Set the mode by numeric custom_mode rather than by name: name
            // translation depends on cs.firmware, which the GUI populates via
            // UpdateCurrentSettings and is not set in this headless harness.
            var guided = new MAVLink.mavlink_set_mode_t
            {
                target_system = Sysid,
                base_mode = (byte)MAVLink.MAV_MODE_FLAG.CUSTOM_MODE_ENABLED,
                custom_mode = CopterGuidedMode,
            };

            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                Mav.setMode(Sysid, Compid, guided);

                var window = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                while (DateTime.UtcNow < window)
                {
                    var hb = await ReadHeartbeat(TimeSpan.FromSeconds(1));
                    if (hb.HasValue && hb.Value.custom_mode == CopterGuidedMode)
                        return true;
                }
            }
            return false;
        }

        /// <summary>Attempt to arm repeatedly until it succeeds or the deadline passes.</summary>
        private static async Task<bool> TryArm(TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    if (await Mav.doARMAsync(Sysid, Compid, true))
                        return true;
                }
                catch
                {
                    // arm rejected (prearm not ready yet) — keep trying
                }
                await Task.Delay(2000);
            }
            return false;
        }

        /// <summary>Send the takeoff command, retrying until accepted or timeout.</summary>
        private static async Task<bool> TakeoffWithRetries(double targetAlt, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    if (await Mav.doCommandAsync(Sysid, Compid, MAVLink.MAV_CMD.TAKEOFF,
                            0, 0, 0, 0, 0, 0, (float)targetAlt))
                        return true;
                }
                catch
                {
                    // transiently rejected — retry
                }
                await Task.Delay(1500);
            }
            return false;
        }

        /// <summary>Pump heartbeats until the armed flag matches, or timeout.</summary>
        private static async Task<bool> WaitForArmedState(bool wantArmed, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                var hb = await ReadHeartbeat(TimeSpan.FromSeconds(2));
                if (hb.HasValue)
                {
                    bool isArmed = (hb.Value.base_mode & MavModeFlagSafetyArmed) != 0;
                    if (isArmed == wantArmed)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Pump packets and return the relative altitude (m) once it reaches
        /// <paramref name="minAltitude"/>, or the last value seen at timeout.
        /// </summary>
        private static async Task<double> WaitForAltitude(double minAltitude, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            double alt = 0;
            while (DateTime.UtcNow < deadline)
            {
                var msg = await Mav.readPacketAsync();
                if (msg == null)
                    continue;

                if (msg.msgid == (uint)MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT)
                {
                    var pos = msg.ToStructure<MAVLink.mavlink_global_position_int_t>();
                    alt = pos.relative_alt / 1000.0; // mm -> m
                    if (alt >= minAltitude)
                        return alt;
                }
            }
            return alt;
        }

        /// <summary>Read packets until a HEARTBEAT arrives, or the window elapses.</summary>
        private static async Task<MAVLink.mavlink_heartbeat_t?> ReadHeartbeat(TimeSpan window)
        {
            var deadline = DateTime.UtcNow + window;
            while (DateTime.UtcNow < deadline)
            {
                var msg = await Mav.readPacketAsync();
                if (msg != null && msg.msgid == (uint)MAVLink.MAVLINK_MSG_ID.HEARTBEAT)
                    return msg.ToStructure<MAVLink.mavlink_heartbeat_t>();
            }
            return null;
        }
    }
}
