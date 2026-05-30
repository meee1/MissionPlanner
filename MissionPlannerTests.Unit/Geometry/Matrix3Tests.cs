using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for <see cref="Matrix3"/> rotation-matrix math: identity,
    /// transpose, matrix/vector and matrix/matrix products, euler round-trips.
    /// </summary>
    [TestClass]
    public class Matrix3Tests
    {
        private const double Tol = 1e-9;

        private static Matrix3 Identity()
        {
            var m = new Matrix3();
            m.identity();
            return m;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Identity_TimesVector_ReturnsSameVector()
        {
            var v = new Vector3(1, 2, 3);
            var r = Identity() * v;
            Assert.AreEqual(1.0, r.xd, Tol);
            Assert.AreEqual(2.0, r.yd, Tol);
            Assert.AreEqual(3.0, r.zd, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Trace_OfIdentity_IsThree()
        {
            Assert.AreEqual(3.0, Identity().trace(), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Transposed_SwapsOffDiagonalElements()
        {
            var m = new Matrix3(
                new Vector3(1, 2, 3),
                new Vector3(4, 5, 6),
                new Vector3(7, 8, 9));
            var t = m.transposed();

            // rows become columns
            Assert.AreEqual(1.0, t.a.xd, Tol);
            Assert.AreEqual(4.0, t.a.yd, Tol);
            Assert.AreEqual(7.0, t.a.zd, Tol);
            Assert.AreEqual(2.0, t.b.xd, Tol);
            Assert.AreEqual(6.0, t.c.yd, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Multiply_ByIdentity_ReturnsSameMatrix()
        {
            var m = new Matrix3(
                new Vector3(1, 2, 3),
                new Vector3(4, 5, 6),
                new Vector3(7, 8, 9));
            var r = m * Identity();

            Assert.AreEqual(m.a.xd, r.a.xd, Tol);
            Assert.AreEqual(m.b.yd, r.b.yd, Tol);
            Assert.AreEqual(m.c.zd, r.c.zd, Tol);
            Assert.AreEqual(m.a.zd, r.a.zd, Tol);
            Assert.AreEqual(m.c.xd, r.c.xd, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FromEuler_ThenToEuler_RoundTrips()
        {
            double roll = 0.25, pitch = -0.4, yaw = 1.2;
            var m = new Matrix3();
            m.from_euler(roll, pitch, yaw);

            double r = 0, p = 0, y = 0;
            m.to_euler(ref r, ref p, ref y);

            Assert.AreEqual(roll, r, 1e-9);
            Assert.AreEqual(pitch, p, 1e-9);
            Assert.AreEqual(yaw, y, 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RotationMatrix_PreservesVectorLength()
        {
            var m = new Matrix3();
            m.from_euler(0.3, 0.2, 1.0);
            var v = new Vector3(0, 3, 4); // length 5
            var r = m * v;
            Assert.AreEqual(5.0, r.length(), 1e-9);
        }
    }
}
