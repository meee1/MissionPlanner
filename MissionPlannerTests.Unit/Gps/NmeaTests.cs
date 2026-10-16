using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Gps
{
    /// <summary>
    /// Regression tests for the streaming NMEA-0183 parser (<see cref="nmea"/>):
    /// it accepts a checksum-valid GPS sentence fed byte-by-byte and rejects a
    /// corrupted one.
    /// </summary>
    [TestClass]
    public class NmeaTests
    {
        // Build a "$G..." sentence with a correct NMEA XOR checksum (XOR of all
        // chars between '$' and '*'), terminated with CRLF.
        private static byte[] Sentence(string body)
        {
            int checksum = 0;
            foreach (char c in body) checksum ^= c;
            return Encoding.ASCII.GetBytes("$" + body + "*" + checksum.ToString("X2") + "\r\n");
        }

        private static int FeedAll(nmea parser, byte[] data)
        {
            int last = -1;
            foreach (byte b in data) last = parser.Read(b);
            return last;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Read_ValidSentence_Returns1()
        {
            var parser = new nmea();
            byte[] sentence = Sentence("GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,");
            Assert.AreEqual(1, FeedAll(parser, sentence), "a checksum-valid sentence should parse");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Read_BadChecksum_DoesNotReturn1()
        {
            var parser = new nmea();
            byte[] good = Sentence("GPRMC,081836,A,3751.65,S,14507.36,E,000.0,360.0,130998,011.3,E");
            // Corrupt the checksum hex (last two chars before CRLF).
            good[good.Length - 3] ^= 0xFF;

            int last = -1;
            foreach (byte b in good) last = parser.Read(b);
            Assert.AreNotEqual(1, last, "a corrupted sentence must not be accepted");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Read_RecoversAndParsesAfterGarbage()
        {
            var parser = new nmea();
            // leading noise then a valid sentence
            FeedAll(parser, Encoding.ASCII.GetBytes("noise$$garbage\r\n"));
            byte[] sentence = Sentence("GPGGA,000000,0000.000,N,00000.000,E,0,00,0.0,0.0,M,0.0,M,,");
            Assert.AreEqual(1, FeedAll(parser, sentence));
        }
    }
}
