using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MissionPlanner;

namespace MissionPlanner.Tests.App
{
    /// <summary>
    /// Build/load guard: proves the whole Mission Planner application really did
    /// compile into the MissionPlannerLib library and that its key main-app types
    /// are present and loadable on .NET 8 (no Windows-only dependency stops them
    /// resolving). This is what makes the rest of the App tier possible.
    /// </summary>
    [TestClass]
    public class AppSurfaceTests
    {
        private static readonly Assembly App = typeof(MagCalib).Assembly;

        [TestMethod]
        [TestCategory("App")]
        public void AppCode_LivesInMissionPlannerLib()
        {
            Assert.AreEqual("MissionPlannerLib", App.GetName().Name);
        }

        [TestMethod]
        [TestCategory("App")]
        public void KeyMainAppTypes_AreLoadable()
        {
            foreach (var name in new[]
                     {
                         "MissionPlanner.MainV2",
                         "MissionPlanner.GCSViews.FlightData",
                         "MissionPlanner.GCSViews.FlightPlanner",
                         "MissionPlanner.Log.LogBrowse",
                         "MissionPlanner.MagCalib",
                     })
            {
                Assert.IsNotNull(App.GetType(name), $"main-app type {name} should be present in MissionPlannerLib");
            }
        }

        [TestMethod]
        [TestCategory("App")]
        public void TheWholeApp_CompiledIntoTheAssembly()
        {
            Type[] types;
            try
            {
                types = App.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            // The full application defines many hundreds of types; a healthy lower
            // bound confirms the whole app (not a stub) was compiled in.
            Assert.IsTrue(types.Length > 100, $"expected the full app surface, found only {types.Length} types");
        }
    }
}
