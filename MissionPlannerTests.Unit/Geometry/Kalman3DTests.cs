using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for the <see cref="Kalman3D"/> position/velocity/
    /// acceleration filter: it converges to a steady measurement and reduces
    /// its variance as measurements arrive.
    /// </summary>
    [TestClass]
    public class Kalman3DTests
    {
        private static Kalman3D MakeFilter(double initial)
        {
            var k = new Kalman3D();
            // qx,qv,qa = process noise; r = measurement covariance; pd = initial
            // variance; ix = initial position.
            k.Reset(0.1, 0.1, 0.1, 1.0, 100.0, initial);
            return k;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Reset_SetsInitialValue()
        {
            var k = MakeFilter(7.0);
            Assert.AreEqual(7.0, k.Value, 1e-9);
            Assert.AreEqual(100.0, k.Variance(), 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ConvergesToSteadyMeasurement()
        {
            var k = MakeFilter(0.0);
            const double target = 10.0;
            double last = 0;
            for (int i = 0; i < 200; i++)
                last = k.Update(target, 1.0);

            Assert.AreEqual(target, last, 0.5, "filter output should converge to the measurement");
            Assert.AreEqual(target, k.Value, 0.5);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Variance_DecreasesAfterUpdates()
        {
            var k = MakeFilter(0.0);
            double before = k.Variance();
            for (int i = 0; i < 20; i++)
                k.Update(5.0, 1.0);

            Assert.IsTrue(k.Variance() < before,
                $"variance should drop after updates (before={before}, after={k.Variance()})");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Prediction_ReturnsFiniteValue()
        {
            var k = MakeFilter(0.0);
            for (int i = 0; i < 10; i++)
                k.Update(3.0, 1.0);

            double pred = k.Predicition(1.0);
            Assert.IsFalse(double.IsNaN(pred) || double.IsInfinity(pred));
        }
    }
}
