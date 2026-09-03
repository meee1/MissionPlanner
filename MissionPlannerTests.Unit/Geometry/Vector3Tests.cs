using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for <see cref="Vector3"/> arithmetic: add/sub, scalar
    /// multiply/divide, dot product (operator *), cross product (operator %),
    /// length and normalisation.
    /// </summary>
    [TestClass]
    public class Vector3Tests
    {
        private const double Tol = 1e-9;

        [TestMethod]
        [TestCategory("Unit")]
        public void Addition_And_Subtraction()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(4, 5, 6);

            var sum = a + b;
            Assert.AreEqual(5, sum.xd, Tol);
            Assert.AreEqual(7, sum.yd, Tol);
            Assert.AreEqual(9, sum.zd, Tol);

            var diff = b - a;
            Assert.AreEqual(3, diff.xd, Tol);
            Assert.AreEqual(3, diff.yd, Tol);
            Assert.AreEqual(3, diff.zd, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ScalarMultiply_And_Divide()
        {
            var a = new Vector3(1, 2, 3);
            var scaled = a * 2.0;
            Assert.AreEqual(2, scaled.xd, Tol);
            Assert.AreEqual(4, scaled.yd, Tol);
            Assert.AreEqual(6, scaled.zd, Tol);

            // commutativity of scalar * vector
            var scaled2 = 2.0 * a;
            Assert.AreEqual(scaled.xd, scaled2.xd, Tol);

            var halved = a / 2.0;
            Assert.AreEqual(0.5, halved.xd, Tol);
            Assert.AreEqual(1.0, halved.yd, Tol);
            Assert.AreEqual(1.5, halved.zd, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DotProduct_OfOrthogonalVectors_IsZero()
        {
            var x = new Vector3(1, 0, 0);
            var y = new Vector3(0, 1, 0);
            double dot = x * y; // operator * is dot product
            Assert.AreEqual(0.0, dot, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DotProduct_KnownValue()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(4, -5, 6);
            // 1*4 + 2*-5 + 3*6 = 4 - 10 + 18 = 12
            Assert.AreEqual(12.0, a * b, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CrossProduct_OfUnitAxes_GivesThirdAxis()
        {
            var x = new Vector3(1, 0, 0);
            var y = new Vector3(0, 1, 0);
            var z = x % y; // operator % is cross product
            Assert.AreEqual(0.0, z.xd, Tol);
            Assert.AreEqual(0.0, z.yd, Tol);
            Assert.AreEqual(1.0, z.zd, Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Length_OfThreeFourZero_IsFive()
        {
            var a = new Vector3(3, 4, 0);
            Assert.AreEqual(5.0, a.length(), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Normalized_HasUnitLength_AndPreservesDirection()
        {
            var a = new Vector3(0, 3, 4);
            var n = a.normalized();
            Assert.AreEqual(1.0, n.length(), 1e-9);
            // direction preserved: components scaled by 1/5
            Assert.AreEqual(0.0, n.xd, Tol);
            Assert.AreEqual(0.6, n.yd, Tol);
            Assert.AreEqual(0.8, n.zd, Tol);
        }
    }
}
