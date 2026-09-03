using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Logs
{
    /// <summary>
    /// Regression tests for binary DataFlash (.bin) parsing
    /// (<see cref="BinaryLog.ReadMessage"/>). A small self-describing .bin is
    /// built in memory: FMT messages (0xA3 0x95 0x80 ...) define formats, and a
    /// data message is decoded by its registered format string.
    /// </summary>
    [TestClass]
    public class BinaryLogTests
    {
        // ArduPilot .bin framing.
        private const byte Head1 = 0xA3;
        private const byte Head2 = 0x95;
        private const byte FmtType = 0x80;

        private static byte[] Ascii(string s, int size)
        {
            var b = new byte[size];
            Encoding.ASCII.GetBytes(s, 0, Math.Min(s.Length, size), b, 0);
            return b;
        }

        /// <summary>An FMT message: header + 0x80 + type,length,name[4],format[16],labels[64].</summary>
        private static IEnumerable<byte> Fmt(byte type, byte length, string name, string format, string labels)
        {
            var b = new List<byte> { Head1, Head2, FmtType, type, length };
            b.AddRange(Ascii(name, 4));
            b.AddRange(Ascii(format, 16));
            b.AddRange(Ascii(labels, 64));
            return b;
        }

        private static byte[] BuildLog()
        {
            var bytes = new List<byte>();
            // Define a "TST" message: format "Ihf" => uint32, int16, float (10 byte payload),
            // total message length = 3 (header+type) + 10 = 13.
            bytes.AddRange(Fmt(10, 13, "TST", "Ihf", "T,V,X"));
            // One TST data record.
            bytes.AddRange(new byte[] { Head1, Head2, 10 });
            bytes.AddRange(BitConverter.GetBytes((uint)4000000000));
            bytes.AddRange(BitConverter.GetBytes((short)-1234));
            bytes.AddRange(BitConverter.GetBytes(1.5f));
            return bytes.ToArray();
        }

        private static List<string> ReadAll(byte[] data)
        {
            var bin = new BinaryLog();
            var ms = new MemoryStream(data);
            var lines = new List<string>();
            string line;
            while (ms.Position < ms.Length && (line = bin.ReadMessage(ms, ms.Length)) != "")
                lines.Add(line.TrimEnd('\r', '\n'));
            return lines;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadMessage_EmitsFmtDefinitionLine()
        {
            var lines = ReadAll(BuildLog());
            // The FMT message decodes to: FMT, <type>, <length>, <name>, <format>, <labels>
            var fmt = lines.FirstOrDefault(l => l.StartsWith("FMT, 10,"));
            Assert.IsNotNull(fmt, "expected an FMT definition line for TST");
            StringAssert.Contains(fmt, "TST");
            StringAssert.Contains(fmt, "Ihf");
            StringAssert.Contains(fmt, "T,V,X");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadMessage_DecodesDataRecordByItsFormat()
        {
            var lines = ReadAll(BuildLog());
            var tst = lines.FirstOrDefault(l => l.StartsWith("TST,"));
            Assert.IsNotNull(tst, "expected a decoded TST data line");

            string[] cols = tst.Split(',').Select(c => c.Trim()).ToArray();
            Assert.AreEqual("TST", cols[0]);
            Assert.AreEqual("4000000000", cols[1]); // I  -> uint32
            Assert.AreEqual("-1234", cols[2]);       // h  -> int16
            Assert.AreEqual("1.5", cols[3]);         // f  -> float
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadMessage_ScaledAndLatLngTypes_Decode()
        {
            // Format "Le": L => int32 / 1e7 (lat/lng), e => int32 / 100.
            var bytes = new List<byte>();
            bytes.AddRange(Fmt(11, 11, "POS", "Le", "Lat,Cm")); // payload 4+4=8, length 11
            bytes.AddRange(new byte[] { Head1, Head2, 11 });
            bytes.AddRange(BitConverter.GetBytes((int)(-353632610)));  // -35.363261 deg
            bytes.AddRange(BitConverter.GetBytes((int)12345));         // 123.45

            var pos = ReadAll(bytes.ToArray()).First(l => l.StartsWith("POS,"));
            string[] cols = pos.Split(',').Select(c => c.Trim()).ToArray();
            Assert.AreEqual(-35.363261, double.Parse(cols[1], System.Globalization.CultureInfo.InvariantCulture), 1e-7);
            Assert.AreEqual(123.45, double.Parse(cols[2], System.Globalization.CultureInfo.InvariantCulture), 1e-9);
        }
    }
}
