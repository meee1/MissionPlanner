using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Unit.Mavlink
{
    /// <summary>
    /// Regression tests for MAVLink packet (de)serialization
    /// (<see cref="MAVLink.MavlinkParse"/>): generate a packet, parse it back,
    /// and confirm the header and payload survive — for both MAVLink v1 and v2,
    /// and for the timestamped ".tlog" framing.
    /// </summary>
    [TestClass]
    public class MavlinkParseTests
    {
        private static MAVLink.mavlink_global_position_int_t SamplePosition() => new MAVLink.mavlink_global_position_int_t
        {
            time_boot_ms = 123456,
            lat = -353632610,   // degE7
            lon = 1491652300,
            alt = 600000,       // mm
            relative_alt = 20000,
            vx = 1, vy = -2, vz = 3,
            hdg = 9000,
        };

        [TestMethod]
        [TestCategory("Unit")]
        public void GeneratePacketV2_ThenRead_RoundTrips()
        {
            var parse = new MAVLink.MavlinkParse();
            var pos = SamplePosition();

            byte[] packet = parse.GenerateMAVLinkPacket20(
                MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT, pos, sign: false, sysid: 7, compid: 1);

            var msg = parse.ReadPacket(new MemoryStream(packet));

            Assert.IsNotNull(msg);
            Assert.IsTrue(msg.ismavlink2, "expected a MAVLink v2 frame");
            Assert.AreEqual((uint)MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT, msg.msgid);
            Assert.AreEqual(7, msg.sysid);
            Assert.AreEqual(1, msg.compid);

            var back = msg.ToStructure<MAVLink.mavlink_global_position_int_t>();
            Assert.AreEqual(pos.lat, back.lat);
            Assert.AreEqual(pos.lon, back.lon);
            Assert.AreEqual(pos.relative_alt, back.relative_alt);
            Assert.AreEqual(pos.hdg, back.hdg);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GeneratePacketV1_ThenRead_RoundTrips()
        {
            var parse = new MAVLink.MavlinkParse();
            var hb = new MAVLink.mavlink_heartbeat_t
            {
                type = (byte)MAVLink.MAV_TYPE.QUADROTOR,
                autopilot = (byte)MAVLink.MAV_AUTOPILOT.ARDUPILOTMEGA,
                base_mode = (byte)MAVLink.MAV_MODE_FLAG.CUSTOM_MODE_ENABLED,
                custom_mode = 4,
                system_status = (byte)MAVLink.MAV_STATE.STANDBY,
                mavlink_version = 3,
            };

            byte[] packet = parse.GenerateMAVLinkPacket10(
                MAVLink.MAVLINK_MSG_ID.HEARTBEAT, hb, sysid: 12, compid: 1);

            var msg = parse.ReadPacket(new MemoryStream(packet));

            Assert.IsNotNull(msg);
            Assert.IsFalse(msg.ismavlink2, "expected a MAVLink v1 frame");
            Assert.AreEqual((uint)MAVLink.MAVLINK_MSG_ID.HEARTBEAT, msg.msgid);
            Assert.AreEqual(12, msg.sysid);

            var back = msg.ToStructure<MAVLink.mavlink_heartbeat_t>();
            Assert.AreEqual(hb.custom_mode, back.custom_mode);
            Assert.AreEqual(hb.type, back.type);
            Assert.AreEqual(hb.autopilot, back.autopilot);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TimestampedTlogFrame_ParsesPayload_AndDecodesTime()
        {
            // A .tlog frame is an 8-byte big-endian microseconds-since-epoch
            // timestamp followed by the raw MAVLink packet.
            var writer = new MAVLink.MavlinkParse();          // no timestamp on write
            var pos = SamplePosition();
            byte[] packet = writer.GenerateMAVLinkPacket20(
                MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT, pos, sysid: 1, compid: 1);

            var when = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            ulong micros = (ulong)(when - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000UL;
            byte[] ts = BitConverter.GetBytes(micros);
            Array.Reverse(ts); // tlog stores the timestamp big-endian

            using var ms = new MemoryStream();
            ms.Write(ts, 0, ts.Length);
            ms.Write(packet, 0, packet.Length);
            ms.Position = 0;

            var reader = new MAVLink.MavlinkParse(hasTimestamp: true);
            var msg = reader.ReadPacket(ms);

            Assert.IsNotNull(msg);
            Assert.AreEqual((uint)MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT, msg.msgid);
            Assert.AreEqual(pos.lat, msg.ToStructure<MAVLink.mavlink_global_position_int_t>().lat);
            // rxtime is converted to local time on read; compare in UTC.
            Assert.AreEqual(when, msg.rxtime.ToUniversalTime(), "decoded tlog timestamp mismatch");
        }
    }
}
