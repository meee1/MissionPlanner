using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner;
using MissionPlanner.Log;
using MissionPlanner.Radio;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// Pure parsing / encoding helpers from the main app (MissionPlannerLib):
    /// hex byte parsing, the XMODEM CRC-16, and log colour parsing.
    /// </summary>
    [TestClass]
    public class ParsingTests
    {
        [TestMethod]
        [TestCategory("App")]
        public void StringToByteArray_ParsesHexPairs()
        {
            CollectionAssert.AreEqual(new byte[] { 0x0A, 0x1B, 0xFF }, temp.StringToByteArray("0A1BFF"));
            CollectionAssert.AreEqual(new byte[] { 0x00 }, temp.StringToByteArray("00"));
            CollectionAssert.AreEqual(new byte[] { 0xFF }, temp.StringToByteArray("ff")); // case-insensitive
            CollectionAssert.AreEqual(new byte[0], temp.StringToByteArray(""));
        }

        [TestMethod]
        [TestCategory("App")]
        public void XModemCrc_IsDeterministicWithKnownInvariants()
        {
            // All-zero input is a fixed point of this CRC: it stays 0.
            Assert.AreEqual(0, XModem.CRC_calc(new byte[8], 8));
            Assert.AreEqual(0, XModem.CRC_calc(new byte[0], 0));

            // Non-zero data yields a non-zero, repeatable checksum.
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            ushort first = XModem.CRC_calc(data, data.Length);
            Assert.AreNotEqual(0, first);
            Assert.AreEqual(first, XModem.CRC_calc(data, data.Length));

            // A changed byte changes the checksum.
            var data2 = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x06 };
            Assert.AreNotEqual(first, XModem.CRC_calc(data2, data2.Length));
        }

        [TestMethod]
        [TestCategory("App")]
        public void HexStringToColor_ParsesAabbggrrLayout()
        {
            // Layout is AA BB GG RR (alpha, blue, green, red).
            var red = MavlinkLogBase.HexStringToColor("FF0000FF");
            Assert.AreEqual(255, red.A);
            Assert.AreEqual(255, red.R);
            Assert.AreEqual(0, red.G);
            Assert.AreEqual(0, red.B);

            var blue = MavlinkLogBase.HexStringToColor("FFFF0000");
            Assert.AreEqual(255, blue.A);
            Assert.AreEqual(0, blue.R);
            Assert.AreEqual(0, blue.G);
            Assert.AreEqual(255, blue.B);

            // Wrong length -> empty colour.
            Assert.IsTrue(MavlinkLogBase.HexStringToColor("FFF").IsEmpty);
        }
    }
}
