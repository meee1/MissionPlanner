using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner.Utilities;

namespace MissionPlanner.Tests.Unit.Mission
{
    /// <summary>
    /// Regression tests for geofence serialization (<see cref="Fence"/>): the
    /// FenceToLocation / LocationToFence pair that maps fence shapes to and from
    /// the MAVLink mission-item list used on every fence upload/download.
    /// </summary>
    [TestClass]
    public class FenceTests
    {
        private static Fence BuildFence()
        {
            var fence = new Fence();

            // An inclusion polygon (4 vertices).
            fence.Fences.Add(new FencePolygon
            {
                Mode = FencePolygon.PolyType.Inclusive,
                Points = new List<PointLatLngAlt>
                {
                    new PointLatLngAlt(-35.3600, 149.1600),
                    new PointLatLngAlt(-35.3600, 149.1650),
                    new PointLatLngAlt(-35.3560, 149.1650),
                    new PointLatLngAlt(-35.3560, 149.1600),
                }
            });

            // An exclusion circle.
            fence.Fences.Add(new FenceCircle
            {
                Mode = FenceCircle.PolyType.Exclusive,
                Center = new PointLatLngAlt(-35.3580, 149.1625),
                Radius = 75f
            });

            return fence;
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void FenceToLocation_EmitsVertexPerPolygonPointPlusCircle()
        {
            var locs = BuildFence().FenceToLocation();
            // 4 polygon vertices + 1 circle row
            Assert.AreEqual(5, locs.Count);

            // Polygon vertices carry the vertex count in p1.
            var polyRows = locs.Where(l => l.id == (ushort)MAVLink.MAV_CMD.FENCE_POLYGON_VERTEX_INCLUSION).ToList();
            Assert.AreEqual(4, polyRows.Count);
            Assert.IsTrue(polyRows.All(r => r.p1 == 4));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void RoundTrip_PreservesPolygonAndCircle()
        {
            var original = BuildFence();
            List<Locationwp> locs = original.FenceToLocation();

            var rebuilt = new Fence();
            rebuilt.LocationToFence(locs);

            Assert.AreEqual(2, rebuilt.Fences.Count);

            var poly = rebuilt.Fences.OfType<FencePolygon>().Single();
            Assert.AreEqual(FencePolygon.PolyType.Inclusive, poly.Mode);
            Assert.AreEqual(4, poly.Points.Count);
            Assert.AreEqual(-35.3600, poly.Points[0].Lat, 1e-9);
            Assert.AreEqual(149.1600, poly.Points[0].Lng, 1e-9);

            var circle = rebuilt.Fences.OfType<FenceCircle>().Single();
            Assert.AreEqual(FenceCircle.PolyType.Exclusive, circle.Mode);
            Assert.AreEqual(75f, circle.Radius, 1e-3);
            Assert.AreEqual(-35.3580, circle.Center.Lat, 1e-9);
            Assert.AreEqual(149.1625, circle.Center.Lng, 1e-9);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LocationToFence_GroupsPolygonVerticesByCount()
        {
            // Two separate inclusion polygons (3 + 3 vertices) must split into two.
            var locs = new List<Locationwp>();
            for (int poly = 0; poly < 2; poly++)
                for (int i = 0; i < 3; i++)
                    locs.Add(new Locationwp
                    {
                        id = (ushort)MAVLink.MAV_CMD.FENCE_POLYGON_VERTEX_INCLUSION,
                        p1 = 3,
                        lat = -35.0 - poly,
                        lng = 149.0 + i
                    });

            var fence = new Fence();
            fence.LocationToFence(locs);

            Assert.AreEqual(2, fence.Fences.OfType<FencePolygon>().Count());
            Assert.IsTrue(fence.Fences.OfType<FencePolygon>().All(p => p.Points.Count == 3));
        }
    }
}
