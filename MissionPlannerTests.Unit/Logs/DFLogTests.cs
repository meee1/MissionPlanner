using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Logs
{
    /// <summary>
    /// Regression tests for DataFlash text-log parsing (<see cref="DFLog"/>):
    /// FMT message registration, field-by-name lookup, and message typing.
    /// The fixture is a small self-describing text log built in memory.
    /// </summary>
    [TestClass]
    public class DFLogTests
    {
        // A minimal self-describing DataFlash text log: format lines (FMT) followed
        // by data rows. FMT columns are: Type,Length,Name,Format,<field names...>.
        // Fields are comma-separated with no spaces, matching how DFLog splits
        // data rows (it does not trim individual tokens).
        private const string LogText =
            "FMT,128,89,FMT,BBnNZ,Type,Length,Name,Format,Columns\n" +
            "FMT,130,45,GPS,BIHBcLLeeEefI,Status,TimeMS,Week,NSats,HDop,Lat,Lng,RelAlt,Alt,Spd,GCrs,VZ,T\n" +
            "FMT,136,30,ATT,IccccCC,TimeMS,DesRoll,Roll,DesPitch,Pitch,DesYaw,Yaw\n" +
            "GPS,3,130040903,1769,10,0.00,-35.3547178,149.1696673,885.52,870.45,24.56,321.44,2.45,127615\n" +
            "ATT,1000,1,2,3,4,50,99\n" +
            "ATT,1100,5,6,7,8,60,110\n";

        private static List<DFLog.DFItem> Parse(out DFLog log)
        {
            log = new DFLog(null);
            var bytes = Encoding.ASCII.GetBytes(LogText);
            return log.ReadLog(new MemoryStream(bytes));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadLog_RegistersAllFormats()
        {
            Parse(out var log);
            Assert.IsTrue(log.logformat.ContainsKey("GPS"), "GPS format not registered");
            Assert.IsTrue(log.logformat.ContainsKey("ATT"), "ATT format not registered");

            var att = log.logformat["ATT"];
            Assert.AreEqual(136, att.Id);
            CollectionAssert.AreEqual(
                new[] { "TimeMS", "DesRoll", "Roll", "DesPitch", "Pitch", "DesYaw", "Yaw" },
                att.FieldNames.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadLog_ParsesAllRows()
        {
            var items = Parse(out _);
            // 3 FMT lines + 1 GPS + 2 ATT = 6 items
            Assert.AreEqual(6, items.Count);

            int attCount = items.Count(i => i.msgtype == "ATT");
            int gpsCount = items.Count(i => i.msgtype == "GPS");
            Assert.AreEqual(2, attCount);
            Assert.AreEqual(1, gpsCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DFItem_FieldLookupByName_ReturnsValues()
        {
            var items = Parse(out _);
            var firstAtt = items.First(i => i.msgtype == "ATT");

            // Values come straight from the first ATT row.
            Assert.AreEqual("1000", firstAtt["TimeMS"]);
            Assert.AreEqual("2", firstAtt["Roll"]);
            Assert.AreEqual("4", firstAtt["Pitch"]);
            Assert.AreEqual("99", firstAtt["Yaw"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DFItem_GpsCoordinates_AreParsed()
        {
            var items = Parse(out _);
            var gps = items.First(i => i.msgtype == "GPS");
            Assert.AreEqual("-35.3547178", gps["Lat"]);
            Assert.AreEqual("149.1696673", gps["Lng"]);
            Assert.AreEqual("10", gps["NSats"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DFItem_UnknownField_ReturnsNull()
        {
            var items = Parse(out _);
            var att = items.First(i => i.msgtype == "ATT");
            Assert.IsNull(att["NotAField"]);
        }
    }
}
