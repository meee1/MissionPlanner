using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// Exercises real Mission Planner application logic (the magnetometer
    /// hard-iron least-squares solver in the root MagCalib class) on plain
    /// .NET 8, via MissionPlannerLib. This is genuine main-app code, not an
    /// ExtLib - it lives in MagCalib.cs at the repository root.
    /// </summary>
    [TestClass]
    public class MagCalibTests
    {
        /// <summary>Well-distributed points on a sphere of radius <paramref name="r"/> centred at (cx,cy,cz).</summary>
        private static List<Tuple<float, float, float>> SphereSamples(double cx, double cy, double cz, double r, int n)
        {
            var data = new List<Tuple<float, float, float>>(n);
            double golden = Math.PI * (1 + Math.Sqrt(5)); // golden-angle spiral -> even coverage
            for (int i = 0; i < n; i++)
            {
                double phi = Math.Acos(1 - 2 * (i + 0.5) / n);
                double theta = golden * i;
                double ux = Math.Cos(theta) * Math.Sin(phi);
                double uy = Math.Sin(theta) * Math.Sin(phi);
                double uz = Math.Cos(phi);
                data.Add(Tuple.Create((float)(cx + r * ux), (float)(cy + r * uy), (float)(cz + r * uz)));
            }
            return data;
        }

        [TestMethod]
        [TestCategory("App")]
        public void LeastSq_RecoversKnownHardIronOffset()
        {
            // True sphere centre (the hard-iron bias the solver must find).
            const double cx = 40, cy = -25, cz = 15, radius = 450;
            var samples = SphereSamples(cx, cy, cz, radius, 300);

            // sphere_error minimises r - |sample + ofs|, so the recovered offset
            // is the negative of the sphere centre, and x[3] is the radius.
            double[] x = MagCalib.LeastSq(samples, ellipsoid: false);

            Assert.AreEqual(4, x.Length);
            Assert.AreEqual(-cx, x[0], 1.0, "offset X");
            Assert.AreEqual(-cy, x[1], 1.0, "offset Y");
            Assert.AreEqual(-cz, x[2], 1.0, "offset Z");
            Assert.AreEqual(radius, x[3], 2.0, "radius");
        }

        [TestMethod]
        [TestCategory("App")]
        public void LeastSq_ZeroCentredData_GivesNearZeroOffset()
        {
            var samples = SphereSamples(0, 0, 0, 300, 200);
            double[] x = MagCalib.LeastSq(samples, ellipsoid: false);

            Assert.AreEqual(0.0, x[0], 0.5);
            Assert.AreEqual(0.0, x[1], 0.5);
            Assert.AreEqual(0.0, x[2], 0.5);
            Assert.AreEqual(300.0, x[3], 1.0);
        }

        [TestMethod]
        [TestCategory("App")]
        public void LeastSq_EllipsoidFit_RecoversOffsetAndUnitDiagonals()
        {
            // Perfect sphere -> ellipsoid fit should still find the offset and
            // diagonal scale factors near 1.
            const double cx = -60, cy = 35, cz = -10, radius = 500;
            var samples = SphereSamples(cx, cy, cz, radius, 300);

            double[] x = MagCalib.LeastSq(samples, ellipsoid: true);

            Assert.IsTrue(x.Length >= 6, "ellipsoid fit returns offsets + diagonals (+ offdiagonals)");
            Assert.AreEqual(-cx, x[0], 2.0, "offset X");
            Assert.AreEqual(-cy, x[1], 2.0, "offset Y");
            Assert.AreEqual(-cz, x[2], 2.0, "offset Z");
            Assert.AreEqual(1.0, x[3], 0.05, "diagonal X scale");
            Assert.AreEqual(1.0, x[4], 0.05, "diagonal Y scale");
            Assert.AreEqual(1.0, x[5], 0.05, "diagonal Z scale");
        }
    }
}
