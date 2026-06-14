using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Params
{
    /// <summary>
    /// Regression tests for the ArduPilot 4.7 parameter rename table
    /// (<see cref="ParamChanges47"/>) — the old↔new name mapping used to keep
    /// older saved parameter files working after the 4.7 renames.
    /// </summary>
    [TestClass]
    public class ParamChanges47Tests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ChangedByOldParam_MapsOldToNew()
        {
            // ANGLE_MAX was renamed to ATC_ANGLE_MAX in 4.7.
            Assert.AreEqual("ATC_ANGLE_MAX", ParamChanges47.changedByOldParam("ANGLE_MAX"));
            Assert.AreEqual("CIRCLE_RADIUS_M", ParamChanges47.changedByOldParam("CIRCLE_RADIUS"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ChangedByNewParam_MapsNewToOld()
        {
            Assert.AreEqual("ANGLE_MAX", ParamChanges47.changedByNewParam("ATC_ANGLE_MAX"));
            Assert.AreEqual("LAND_SPEED", ParamChanges47.changedByNewParam("LAND_SPD_MS"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void OldToNew_RoundTrips()
        {
            // For a known rename, new -> old -> new must return the original new name.
            const string newName = "ATC_ANGLE_MAX";
            string old = ParamChanges47.changedByNewParam(newName);
            Assert.IsNotNull(old);
            Assert.AreEqual(newName, ParamChanges47.changedByOldParam(old));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Warning_IncludesArrowAndDescription()
        {
            string warning = ParamChanges47.changedByNewParamWarning("ATC_ANGLE_MAX");
            Assert.IsNotNull(warning);
            StringAssert.Contains(warning, "→");
            StringAssert.Contains(warning, "ANGLE_MAX");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void UnknownParam_ReturnsNull()
        {
            Assert.IsNull(ParamChanges47.changedByOldParam("NOT_A_REAL_PARAM"));
            Assert.IsNull(ParamChanges47.changedByNewParam("NOT_A_REAL_PARAM"));
            Assert.IsNull(ParamChanges47.changedByNewParamWarning("NOT_A_REAL_PARAM"));
        }
    }
}
