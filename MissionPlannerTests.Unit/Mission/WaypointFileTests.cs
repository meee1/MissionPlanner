using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Mission
{
    /// <summary>
    /// Regression tests for <see cref="WaypointFile.ReadWaypointFile"/>, which
    /// parses the standard "QGC WPL 110" tab-separated mission format. The
    /// fixture is written to a temp file so the test stays self-contained.
    /// </summary>
    [TestClass]
    public class WaypointFileTests
    {
        private string _file;

        [TestInitialize]
        public void Setup()
        {
            // Columns: seq cur frame cmd p1 p2 p3 p4 lat lng alt autocontinue
            // Index 0 is home (MAV_CMD 16 = WAYPOINT), then a takeoff and two waypoints.
            string[] lines =
            {
                "QGC WPL 110",
                "0\t1\t0\t16\t0\t0\t0\t0\t-35.363261\t149.165230\t584.000000\t1",
                "1\t0\t3\t22\t0\t0\t0\t0\t-35.363261\t149.165230\t20.000000\t1",
                "2\t0\t3\t16\t0\t0\t0\t0\t-35.359800\t149.164300\t30.000000\t1",
                "3\t0\t3\t16\t0\t0\t0\t0\t-35.360000\t149.167000\t40.000000\t1",
            };
            _file = Path.Combine(Path.GetTempPath(), "mp_unit_" + System.Guid.NewGuid().ToString("N") + ".waypoints");
            File.WriteAllLines(_file, lines);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (_file != null && File.Exists(_file))
                File.Delete(_file);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadWaypointFile_ParsesAllRows()
        {
            List<Locationwp> wps = WaypointFile.ReadWaypointFile(_file);
            Assert.AreEqual(4, wps.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadWaypointFile_MapsColumnsToFields()
        {
            List<Locationwp> wps = WaypointFile.ReadWaypointFile(_file);

            // Home (row 0)
            Assert.AreEqual((ushort)MAVLink.MAV_CMD.WAYPOINT, wps[0].id);
            Assert.AreEqual(0, wps[0].frame);
            Assert.AreEqual(-35.363261, wps[0].lat, 1e-9);
            Assert.AreEqual(149.165230, wps[0].lng, 1e-9);
            Assert.AreEqual(584.0, wps[0].alt, 1e-3);

            // Takeoff (row 1) - MAV_CMD 22
            Assert.AreEqual((ushort)MAVLink.MAV_CMD.TAKEOFF, wps[1].id);
            Assert.AreEqual(3, wps[1].frame);
            Assert.AreEqual(20.0, wps[1].alt, 1e-3);

            // Waypoint (row 2)
            Assert.AreEqual(-35.359800, wps[2].lat, 1e-9);
            Assert.AreEqual(149.164300, wps[2].lng, 1e-9);
            Assert.AreEqual(30.0, wps[2].alt, 1e-3);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ReadWaypointFile_PrependsHome_WhenFirstIndexNotZero()
        {
            // A file whose first row is index 1 should get a synthetic home prepended.
            string[] lines =
            {
                "QGC WPL 110",
                "1\t0\t3\t16\t0\t0\t0\t0\t-35.36\t149.16\t30.000000\t1",
            };
            string file = Path.Combine(Path.GetTempPath(), "mp_unit_" + System.Guid.NewGuid().ToString("N") + ".waypoints");
            File.WriteAllLines(file, lines);
            try
            {
                List<Locationwp> wps = WaypointFile.ReadWaypointFile(file);
                Assert.AreEqual(2, wps.Count, "expected a synthetic home plus the one row");
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}
