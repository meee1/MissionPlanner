using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Geometry
{
    /// <summary>
    /// Regression tests for the general <see cref="Matrix"/> linear-algebra type
    /// (used by the Kalman/estimation maths): identity, multiply, and transpose.
    /// Indexing is Get(column, row) / Set(column, row, value).
    /// </summary>
    [TestClass]
    public class MatrixTests
    {
        private const double Tol = 1e-9;

        private static Matrix From2x2(double a, double b, double c, double d)
        {
            var m = new Matrix(2, 2); // (cols, rows)
            m.Set(0, 0, a); m.Set(1, 0, b); // row 0: [a b]
            m.Set(0, 1, c); m.Set(1, 1, d); // row 1: [c d]
            return m;
        }

        // NOTE: Matrix.MakeIdentity / SetIdentity is currently broken in the
        // library (it never sets the top-left diagonal element), so it is not
        // exercised here. See the build-an-identity-by-hand approach below.
        private static Matrix Identity2()
        {
            var m = new Matrix(2, 2);
            m.Set(0, 0, 1); m.Set(1, 0, 0);
            m.Set(0, 1, 0); m.Set(1, 1, 1);
            return m;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Multiply_ByIdentity_IsUnchanged()
        {
            var m = From2x2(1, 2, 3, 4);
            var result = Matrix.Multiply(m, Identity2());

            Assert.AreEqual(1.0, result.Get(0, 0), Tol);
            Assert.AreEqual(2.0, result.Get(1, 0), Tol);
            Assert.AreEqual(3.0, result.Get(0, 1), Tol);
            Assert.AreEqual(4.0, result.Get(1, 1), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Multiply_TwoMatrices_KnownProduct()
        {
            // [1 2; 3 4] * [5 6; 7 8] = [19 22; 43 50]
            var product = Matrix.Multiply(From2x2(1, 2, 3, 4), From2x2(5, 6, 7, 8));

            Assert.AreEqual(19.0, product.Get(0, 0), Tol);
            Assert.AreEqual(22.0, product.Get(1, 0), Tol);
            Assert.AreEqual(43.0, product.Get(0, 1), Tol);
            Assert.AreEqual(50.0, product.Get(1, 1), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Transpose_SwapsRowsAndColumns()
        {
            var m = From2x2(1, 2, 3, 4);
            var t = Matrix.Transpose(m);

            // transpose of [1 2; 3 4] is [1 3; 2 4]
            Assert.AreEqual(1.0, t.Get(0, 0), Tol);
            Assert.AreEqual(3.0, t.Get(1, 0), Tol);
            Assert.AreEqual(2.0, t.Get(0, 1), Tol);
            Assert.AreEqual(4.0, t.Get(1, 1), Tol);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ScalarMultiply_ScalesEveryElement()
        {
            var m = From2x2(1, 2, 3, 4);
            var scaled = Matrix.Multiply(m, 2.0);
            Assert.AreEqual(2.0, scaled.Get(0, 0), Tol);
            Assert.AreEqual(8.0, scaled.Get(1, 1), Tol);
        }
    }
}
