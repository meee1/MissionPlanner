using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Gps
{
    /// <summary>
    /// Regression tests for u-blox UBX framing (<see cref="Ubx.generate"/>):
    /// sync header, class/id, little-endian length, and the 8-bit Fletcher
    /// checksum over the packet body.
    /// </summary>
    [TestClass]
    public class UbxTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Generate_WritesHeader_LengthAndChecksum()
        {
            byte cls = 0x06, subclass = 0x01;
            byte[] payload = { 0x10, 0x20, 0x30, 0x40, 0x50 };

            byte[] pkt = Ubx.generate(cls, subclass, payload);

            // sync chars
            Assert.AreEqual(0xB5, pkt[0]);
            Assert.AreEqual(0x62, pkt[1]);
            // class / id
            Assert.AreEqual(cls, pkt[2]);
            Assert.AreEqual(subclass, pkt[3]);
            // length, little-endian
            Assert.AreEqual(payload.Length & 0xFF, pkt[4]);
            Assert.AreEqual((payload.Length >> 8) & 0xFF, pkt[5]);
            // total length = 6 header + payload + 2 checksum
            Assert.AreEqual(6 + payload.Length + 2, pkt.Length);

            // Independently recompute the Fletcher-8 checksum over bytes [2 .. len-2).
            byte ckA = 0, ckB = 0;
            for (int i = 2; i < pkt.Length - 2; i++)
            {
                ckA = (byte)(ckA + pkt[i]);
                ckB = (byte)(ckB + ckA);
            }
            Assert.AreEqual(ckA, pkt[pkt.Length - 2], "checksum A");
            Assert.AreEqual(ckB, pkt[pkt.Length - 1], "checksum B");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Generate_EmptyPayload_HasZeroLength()
        {
            byte[] pkt = Ubx.generate(0x0A, 0x04, new byte[0]);
            Assert.AreEqual(8, pkt.Length); // 6 header + 0 payload + 2 checksum
            Assert.AreEqual(0, pkt[4]);
            Assert.AreEqual(0, pkt[5]);
        }
    }
}
