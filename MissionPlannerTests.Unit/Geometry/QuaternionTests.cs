using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for <see cref="Quaternion"/> attitude math: identity,
    /// euler round-trips, magnitude, conjugate and frame rotations. These back
    /// every attitude/orientation calculation in the HUD and logs.
    /// </summary>
    [TestClass]
    public class QuaternionTests
    {
        private const double Tol = 1e-9;

        [TestMethod]
        [TestCategory("Unit")]
        public void DefaultConstructor_IsIdentity()
        {
            var q = new Quaternion();
            Assert.AreEqual(1.0, q.q1, Tol);
            Assert.AreEqual(0.0, q.q2, Tol);
            Assert.AreEqual(0.0, q.q3, Tol);
            Assert.AreEqual(0.0, q.q4, Tol);
            Assert.AreEqual(1.0, q.length(), Tol);
            Assert.AreEqual(0.0, q.get_euler_roll(), Tol);
            Assert.AreEqual(0.0, q.get_euler_pitch(), Tol);
            Assert.AreEqual(0.0, q.get_euler_yaw(), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FromEuler_ThenGetEuler_RoundTrips()
        {
            // Stay clear of gimbal lock (|pitch| != pi/2).
            double roll = 0.30, pitch = -0.45, yaw = 1.10;
            var q = Quaternion.from_euler(roll, pitch, yaw);

            Assert.AreEqual(roll, q.get_euler_roll(), 1e-9);
            Assert.AreEqual(pitch, q.get_euler_pitch(), 1e-9);
            Assert.AreEqual(yaw, q.get_euler_yaw(), 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FromEuler_Zero_IsIdentity()
        {
            var q = Quaternion.from_euler(0, 0, 0);
            Assert.AreEqual(1.0, q.q1, Tol);
            Assert.AreEqual(0.0, q.q2, Tol);
            Assert.AreEqual(0.0, q.q3, Tol);
            Assert.AreEqual(0.0, q.q4, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Length_KnownValue()
        {
            var q = new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(Math.Sqrt(30.0), q.length(), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Conjugate_NegatesVectorPartOnly()
        {
            var q = new Quaternion(1, 2, 3, 4);
            var c = q.conjugate();
            Assert.AreEqual(1.0, c.q1, Tol);
            Assert.AreEqual(-2.0, c.q2, Tol);
            Assert.AreEqual(-3.0, c.q3, Tol);
            Assert.AreEqual(-4.0, c.q4, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Normalize_GivesUnitLength()
        {
            var q = new Quaternion(1, 2, 3, 4);
            q.normalize();
            Assert.AreEqual(1.0, q.length(), 1e-12);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BodyToEarth_ThenEarthToBody_RoundTripsVector()
        {
            var q = Quaternion.from_euler(0.2, -0.3, 0.7);
            var v = new Vector3(1.0, 2.0, 3.0);

            var earth = q.body_to_earth(v);
            var back = q.earth_to_body(earth);

            Assert.AreEqual(v.xd, back.xd, 1e-9);
            Assert.AreEqual(v.yd, back.yd, 1e-9);
            Assert.AreEqual(v.zd, back.zd, 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Rotation_PreservesVectorLength()
        {
            var q = Quaternion.from_euler(0.5, 0.5, 0.5);
            var v = new Vector3(3.0, 4.0, 12.0); // length 13
            var rotated = q.body_to_earth(v);
            Assert.AreEqual(13.0, rotated.length(), 1e-9);
        }
    }
}
