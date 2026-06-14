using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Gps
{
    /// <summary>
    /// Regression tests for RTCM/GNSS maths in <see cref="rtcm3"/>: the
    /// geodetic&lt;-&gt;ECEF coordinate transforms and the CRC-24Q checksum used
    /// to validate every RTCM3 correction frame.
    /// </summary>
    [TestClass]
    public class RtcmTests
    {
        private const double Deg2Rad = Math.PI / 180.0;

        [TestMethod]
        [TestCategory("Unit")]
        public void Pos2Ecef_ThenEcef2Pos_RoundTrips()
        {
            // pos = { lat(rad), lng(rad), height(m) }
            double[] pos = { -35.363261 * Deg2Rad, 149.165230 * Deg2Rad, 584.0 };
            double[] r = new double[3];
            rtcm3.pos2ecef(pos, ref r);

            double[] back = new double[3];
            rtcm3.ecef2pos(r, ref back);

            Assert.AreEqual(pos[0], back[0], 1e-9, "latitude");
            Assert.AreEqual(pos[1], back[1], 1e-9, "longitude");
            Assert.AreEqual(pos[2], back[2], 1e-3, "height");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Pos2Ecef_Equator_PrimeMeridian_IsOnXAxis()
        {
            // lat=0, lng=0, h=0 -> on the +X axis at the WGS84 semi-major radius.
            double[] pos = { 0, 0, 0 };
            double[] r = new double[3];
            rtcm3.pos2ecef(pos, ref r);

            Assert.AreEqual(6378137.0, r[0], 1e-3); // RE_WGS84
            Assert.AreEqual(0.0, r[1], 1e-6);
            Assert.AreEqual(0.0, r[2], 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Crc24q_IsDeterministic()
        {
            byte[] data = { 0xD3, 0x00, 0x13, 0x3E, 0xD7, 0xD3, 0x02 };
            uint a = rtcm3.crc24.crc24q(data, (uint)data.Length, 0);
            uint b = rtcm3.crc24.crc24q(data, (uint)data.Length, 0);
            Assert.AreEqual(a, b);
            Assert.IsTrue(a <= 0xFFFFFF, "CRC-24 must fit in 24 bits");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Crc24q_AppendedChecksum_RecomputesToZero()
        {
            // CRC-24Q property: appending the big-endian CRC to the message and
            // running the CRC over the whole thing yields 0.
            byte[] msg = { 0xD3, 0x00, 0x04, 0x4C, 0xE0, 0x00, 0x80 };
            uint crc = rtcm3.crc24.crc24q(msg, (uint)msg.Length, 0);

            byte[] framed = new byte[msg.Length + 3];
            Array.Copy(msg, framed, msg.Length);
            framed[msg.Length + 0] = (byte)((crc >> 16) & 0xFF);
            framed[msg.Length + 1] = (byte)((crc >> 8) & 0xFF);
            framed[msg.Length + 2] = (byte)(crc & 0xFF);

            Assert.AreEqual(0u, rtcm3.crc24.crc24q(framed, (uint)framed.Length, 0));
        }
    }
}
