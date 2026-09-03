using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MissionPlanner.Tests.Unit.Mavlink
{
    /// <summary>
    /// Regression tests for the MAVLink X25/MCRF4XX CRC implementation
    /// (<see cref="MAVLink.MavlinkCRC"/>). Pinning these protects every byte
    /// that goes on the wire: a change here breaks comms with every vehicle.
    /// </summary>
    [TestClass]
    public class MavlinkCrcTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void CrcAccumulate_StandardCheckVector_Is0x6F91()
        {
            // CRC-16/MCRF4XX check value over the ASCII string "123456789",
            // seeded with the X25 init 0xFFFF. This is the canonical vector.
            ushort crc = 0xFFFF;
            foreach (byte b in Encoding.ASCII.GetBytes("123456789"))
                crc = MAVLink.MavlinkCRC.crc_accumulate(b, crc);

            Assert.AreEqual(0x6F91, crc);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CrcCalculate_SkipsFirstByte()
        {
            // crc_calculate intentionally skips index 0 (the start-of-frame byte),
            // so the first byte must not influence the result.
            byte[] a = { 0xFE, 1, 2, 3, 4, 5 };
            byte[] b = { 0x00, 1, 2, 3, 4, 5 };

            Assert.AreEqual(
                MAVLink.MavlinkCRC.crc_calculate(a, a.Length),
                MAVLink.MavlinkCRC.crc_calculate(b, b.Length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CrcCalculate_EmptyOrSingleByte_ReturnsInit()
        {
            // length < 1 short-circuits to 0xffff; a single byte loops zero times
            // (loop starts at index 1) and also yields the seed.
            Assert.AreEqual((ushort)0xFFFF, MAVLink.MavlinkCRC.crc_calculate(new byte[0], 0));
            Assert.AreEqual((ushort)0xFFFF, MAVLink.MavlinkCRC.crc_calculate(new byte[] { 0xAB }, 1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CrcCalculate_MatchesManualAccumulateFromIndexOne()
        {
            byte[] buf = { 0xFE, 9, 0, 1, 1, 0, 10, 20, 30 };

            ushort expected = 0xFFFF;
            for (int i = 1; i < buf.Length; i++)
                expected = MAVLink.MavlinkCRC.crc_accumulate(buf[i], expected);

            Assert.AreEqual(expected, MAVLink.MavlinkCRC.crc_calculate(buf, buf.Length));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CrcAccumulate_IsDeterministic()
        {
            ushort first = MAVLink.MavlinkCRC.crc_accumulate(0x42, 0xFFFF);
            ushort second = MAVLink.MavlinkCRC.crc_accumulate(0x42, 0xFFFF);
            Assert.AreEqual(first, second);
        }
    }
}
