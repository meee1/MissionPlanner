using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Mission
{
    /// <summary>
    /// Regression tests for the QGC-style JSON .plan format
    /// (<see cref="MissionFile.ReadFile"/> / <see cref="MissionFile.WriteFile"/>
    /// and <see cref="MissionFile.ConvertToLocationwps"/>).
    /// </summary>
    [TestClass]
    public class MissionFileJsonTests
    {
        private static MissionFile.RootObject BuildPlan()
        {
            return new MissionFile.RootObject
            {
                fileType = "Plan",
                version = 1,
                groundStation = "MissionPlannerTests",
                mission = new MissionFile.Mission
                {
                    version = 2,
                    firmwareType = 12,
                    vehicleType = 2,
                    cruiseSpeed = 15,
                    hoverSpeed = 5,
                    plannedHomePosition = new List<double> { -35.363261, 149.165230, 584 },
                    items = new List<MissionFile.Item>
                    {
                        new MissionFile.Item
                        {
                            type = "SimpleItem", command = 22, frame = 3, autoContinue = true, doJumpId = 1,
                            @params = new List<double?> { 0, 0, 0, 0, 0, 0, 20 } // TAKEOFF to 20 m
                        },
                        new MissionFile.Item
                        {
                            type = "SimpleItem", command = 16, frame = 3, autoContinue = true, doJumpId = 2,
                            @params = new List<double?> { 0, 0, 0, 0, -35.359800, 149.164300, 30 }
                        },
                        new MissionFile.Item
                        {
                            type = "SimpleItem", command = 16, frame = 3, autoContinue = true, doJumpId = 3,
                            @params = new List<double?> { 0, 0, 0, 0, -35.360000, 149.167000, 40 }
                        },
                    }
                }
            };
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void WriteFile_ThenReadFile_PreservesMission()
        {
            string file = Path.Combine(Path.GetTempPath(), "mp_unit_" + System.Guid.NewGuid().ToString("N") + ".plan");
            try
            {
                MissionFile.WriteFile(file, BuildPlan());
                Assert.IsTrue(File.Exists(file));

                var read = MissionFile.ReadFile(file);
                Assert.AreEqual("Plan", read.fileType);
                Assert.AreEqual(3, read.mission.items.Count);

                // first item is the takeoff
                Assert.AreEqual(22, read.mission.items[0].command);
                Assert.AreEqual(20, read.mission.items[0].@params[6]);

                // second item is a waypoint at a known coordinate
                Assert.AreEqual(16, read.mission.items[1].command);
                Assert.AreEqual(-35.359800, read.mission.items[1].@params[4].Value, 1e-9);
                Assert.AreEqual(149.164300, read.mission.items[1].@params[5].Value, 1e-9);

                CollectionAssert.AreEqual(
                    new List<double> { -35.363261, 149.165230, 584 },
                    read.mission.plannedHomePosition);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ConvertToLocationwps_PrependsHome_AndMapsItems()
        {
            List<Locationwp> wps = MissionFile.ConvertToLocationwps(BuildPlan());

            // home (from plannedHomePosition) + 3 items
            Assert.AreEqual(4, wps.Count);

            // index 0 is home
            Assert.AreEqual(-35.363261, wps[0].lat, 1e-9);
            Assert.AreEqual(149.165230, wps[0].lng, 1e-9);

            // takeoff command id carried through
            Assert.AreEqual(22, wps[1].id);
            Assert.AreEqual(20, wps[1].alt, 1e-3);

            // waypoint coordinates carried through (params[4]=lat, [5]=lng)
            Assert.AreEqual(-35.359800, wps[2].lat, 1e-9);
            Assert.AreEqual(149.164300, wps[2].lng, 1e-9);
            Assert.AreEqual(16, wps[2].id);
        }
    }
}
