using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for the pure ArduPilot-style math helpers in
    /// <see cref="Utils"/> and <see cref="MathHelper"/> (constrain, safe asin,
    /// equality, radian/degree, and range mapping). These underpin a lot of the
    /// flight-data and navigation maths.
    /// </summary>
    [TestClass]
    public class MathHelpersTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ConstrainFloat_ClampsToRange()
        {
            Assert.AreEqual(5f, Utils.constrain_float(10f, 0f, 5f));
            Assert.AreEqual(0f, Utils.constrain_float(-3f, 0f, 5f));
            Assert.AreEqual(2.5f, Utils.constrain_float(2.5f, 0f, 5f));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ConstrainFloat_NaN_ReturnsMidpoint()
        {
            Assert.AreEqual(5f, Utils.constrain_float(float.NaN, 0f, 10f));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SafeAsin_ClampsOutOfRangeInputs()
        {
            Assert.AreEqual(Math.PI / 2, Utils.safe_asin(2.0f), 1e-6);   // >= 1 -> +pi/2
            Assert.AreEqual(-Math.PI / 2, Utils.safe_asin(-2.0f), 1e-6); // <= -1 -> -pi/2
            Assert.AreEqual(0.0f, Utils.safe_asin(float.NaN), 1e-9);     // NaN -> 0
            Assert.AreEqual(Math.Asin(0.5), Utils.safe_asin(0.5f), 1e-6);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void IsEqual_And_IsZero()
        {
            Assert.IsTrue(Utils.is_zero(0f));
            Assert.IsFalse(Utils.is_zero(0.01f));
            Assert.IsTrue(Utils.is_equal(1.0f, 1.0f));
            Assert.IsFalse(Utils.is_equal(1.0f, 1.001f));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RadiansDegrees_RoundTrip()
        {
            Assert.AreEqual(Math.PI, Utils.radians(180.0), 1e-9);
            Assert.AreEqual(180.0, Utils.degrees(Math.PI), 1e-9);
            Assert.AreEqual(90.0, Utils.degrees(Utils.radians(90.0)), 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Labs_And_Fabsf()
        {
            Assert.AreEqual(7, Utils.labs(-7.4));
            Assert.AreEqual(3.5f, Utils.fabsf(-3.5));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MathHelper_Map_IsLinear()
        {
            // map 0..100 to 0..1
            Assert.AreEqual(0.5, MathHelper.map(50, 0, 100, 0, 1), 1e-12);
            Assert.AreEqual(0.0, MathHelper.map(0, 0, 100, 0, 1), 1e-12);
            // map can extrapolate beyond the output range
            Assert.AreEqual(2.0, MathHelper.map(200, 0, 100, 0, 1), 1e-12);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MathHelper_MapConstrained_ClampsToOutputRange()
        {
            Assert.AreEqual(1.0, MathHelper.mapConstrained(200, 0, 100, 0, 1), 1e-12);
            Assert.AreEqual(0.0, MathHelper.mapConstrained(-50, 0, 100, 0, 1), 1e-12);
            Assert.AreEqual(0.5, MathHelper.mapConstrained(50, 0, 100, 0, 1), 1e-12);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MathHelper_Constrain_ClampsValue()
        {
            Assert.AreEqual(10.0, MathHelper.constrain(99, -10, 10), 1e-12);
            Assert.AreEqual(-10.0, MathHelper.constrain(-99, -10, 10), 1e-12);
            Assert.AreEqual(3.0, MathHelper.constrain(3, -10, 10), 1e-12);
        }
    }
}
