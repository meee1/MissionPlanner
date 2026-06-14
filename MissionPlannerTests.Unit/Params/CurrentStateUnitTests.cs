using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner;

namespace MissionPlanner.Tests.Unit.Params
{
    /// <summary>
    /// Regression tests for the global display-unit conversion helpers on
    /// <see cref="CurrentState"/>. These multipliers drive every distance/speed/
    /// altitude shown in the UI, so the to/from pair must round-trip exactly.
    /// The multipliers are static global state, so each test restores them.
    /// </summary>
    [TestClass]
    public class CurrentStateUnitTests
    {
        [TestCleanup]
        public void RestoreDefaults()
        {
            CurrentState.multiplierdist = 1f;
            CurrentState.multiplierspeed = 1f;
            CurrentState.multiplieralt = 1f;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DefaultMultipliers_AreMetric_Identity()
        {
            Assert.AreEqual(1f, CurrentState.multiplierdist);
            Assert.AreEqual(1f, CurrentState.multiplierspeed);
            Assert.AreEqual(1f, CurrentState.multiplieralt);
            Assert.AreEqual(123.45, CurrentState.toDistDisplayUnit(123.45), 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DistDisplayUnit_RoundTrips_UnderFeetMultiplier()
        {
            // metres -> feet
            CurrentState.multiplierdist = 3.2808399f;
            double metres = 100.0;
            double display = CurrentState.toDistDisplayUnit(metres);
            Assert.AreEqual(328.08399, display, 1e-3);
            Assert.AreEqual(metres, CurrentState.fromDistDisplayUnit(display), 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SpeedDisplayUnit_RoundTrips()
        {
            CurrentState.multiplierspeed = 3.6f; // m/s -> km/h
            double ms = 10.0;
            double display = CurrentState.toSpeedDisplayUnit(ms);
            Assert.AreEqual(36.0, display, 1e-4);
            Assert.AreEqual(ms, CurrentState.fromSpeedDisplayUnit(display), 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void AltDisplayUnit_AppliesMultiplier()
        {
            CurrentState.multiplieralt = 3.2808399f;
            Assert.AreEqual(32.808399, CurrentState.toAltDisplayUnit(10.0), 1e-4);
        }
    }
}
