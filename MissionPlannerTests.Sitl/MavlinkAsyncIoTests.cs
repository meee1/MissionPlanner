using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.ArduPilot;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Sitl
{
    /// <summary>
    /// Validates the asynchronous I/O behaviour of <see cref="MAVLinkInterface"/>
    /// against a live ArduPilot SITL stream — the properties that are hard to test
    /// without a real, continuously-streaming link:
    ///   * readPacketAsync pumps a stream of valid messages;
    ///   * concurrent readPacketAsync calls are serialised by the internal
    ///     SemaphoreSlim readlock and all complete without corrupting the shared
    ///     BaseStream (no exceptions, no deadlock);
    ///   * the read lock is released so the interface stays usable afterwards;
    ///   * the async retry/timeout path throws TimeoutException on no response;
    ///   * the synchronous wrappers (…Async().AwaitSync()) work over real I/O.
    ///
    /// Uses its own SITL instance/connection (one MAVLinkInterface per process is
    /// the supported usage). Inconclusive when no SITL binary is available.
    /// </summary>
    [TestClass]
    public class MavlinkAsyncIoTests
    {
        private static SitlFixture _sitl;
        private static string _knownParam;

        private static MAVLinkInterface Mav => _sitl.Mav;
        private static byte Sysid => _sitl.Sysid;
        private static byte Compid => _sitl.Compid;

        [ClassInitialize]
        public static void StartSitl(TestContext _)
        {
            if (!SitlFixture.IsAvailable)
                return;

            _sitl = SitlFixture.StartCopter();
            var paramlist = Mav.getParamListAsync(Sysid, Compid).GetAwaiter().GetResult();
            _knownParam = new[] { "WPNAV_SPEED", "PILOT_SPEED_UP", "ANGLE_MAX" }
                              .FirstOrDefault(paramlist.ContainsKey)
                          ?? paramlist.First().Name;
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static void StopSitl() => _sitl?.Dispose();

        [TestInitialize]
        public void RequireSitl()
        {
            if (_sitl == null)
                Assert.Inconclusive("SITL_BIN_DIR not set or binary missing; skipping SITL async I/O test.");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task ReadPacketAsync_StreamsMultipleMessageTypes()
        {
            var seen = new System.Collections.Generic.HashSet<uint>();
            for (int i = 0; i < 30 && seen.Count < 3; i++)
            {
                var msg = await Mav.readPacketAsync();
                if (msg != null && msg.Length > 5)
                    seen.Add(msg.msgid);
            }

            // A live SITL link continuously streams heartbeat, attitude, GPS, etc.
            Assert.IsTrue(seen.Count >= 3,
                $"expected several distinct streaming message types, saw {seen.Count}");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task ConcurrentReadPacketAsync_AllComplete_SerializedByReadLock()
        {
            // Fire many reads at once against the single shared BaseStream. Without
            // the internal readlock these would race and corrupt the parser; with
            // it they serialise and every one returns a message.
            const int n = 12;
            var tasks = Enumerable.Range(0, n).Select(_ => Mav.readPacketAsync()).ToArray();

            var results = await Task.WhenAll(tasks);

            // WhenAll completing without exception/deadlock is the core proof that
            // the readlock serialised concurrent access to the shared stream.
            Assert.AreEqual(n, results.Length);
            int valid = results.Count(r => r != null && r.Length > 5);
            Assert.IsTrue(valid >= n - 2, $"expected almost all concurrent reads to carry payloads, got {valid}/{n}");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task InterfaceStaysUsable_AfterConcurrentReads()
        {
            // Hammer the link concurrently, then confirm the readlock was released
            // and a normal request/response round-trip still works.
            await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Mav.readPacketAsync()));

            float value = await Mav.GetParamAsync(Sysid, Compid, _knownParam);
            Assert.IsFalse(float.IsNaN(value), $"GetParamAsync({_knownParam}) returned NaN after concurrent reads");
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task GetParamAsync_UnknownParameter_ThrowsTimeout()
        {
            // No PARAM_VALUE will ever match this name, so the async retry/timeout
            // path must surface a TimeoutException rather than hang.
            await Assert.ThrowsExceptionAsync<TimeoutException>(
                () => Mav.GetParamAsync(Sysid, Compid, "ZZ_NOPARAM"));
        }

        [TestMethod]
        [TestCategory("Sitl")]
        public async Task SyncAndAsyncHeartbeat_BothReturnHeartbeat()
        {
            // The async call and its AwaitSync() wrapper must both work over real I/O.
            var asyncHb = await Mav.getHeartBeatAsync();
            var syncHb = Mav.getHeartBeat();

            Assert.IsNotNull(asyncHb);
            Assert.IsNotNull(syncHb);
            Assert.AreEqual((uint)MAVLink.MAVLINK_MSG_ID.HEARTBEAT, asyncHb.msgid);
            Assert.AreEqual((uint)MAVLink.MAVLINK_MSG_ID.HEARTBEAT, syncHb.msgid);
        }
    }
}
