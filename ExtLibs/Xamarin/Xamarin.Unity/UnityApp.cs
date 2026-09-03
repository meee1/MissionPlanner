// UnityApp.cs
// Application-level initialisation – equivalent to the Xamarin.Forms App class.
//
// Responsible for:
//   • applying platform defaults (locale, logging)
//
// Note: form-tree creation and lifecycle are managed by UnityMain → UnityFormHost.
// The cross-platform MainV2 lives in Xamarin/Linked/MainV2.cs which is compiled
// into Xamarin.csproj, not MissionPlannerLib, so it is not directly reachable
// here.  UnityMain.Boot() owns the bootstrap sequence.

using System;
using MissionPlanner.Utilities;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Unity
{
    /// <summary>
    /// Applies process-wide defaults before the MissionPlanner form tree is created.
    /// Call <see cref="ApplyPlatformDefaults"/> once from <see cref="UnityMain"/>.
    /// </summary>
    public static class UnityApp
    {
        public static void ApplyPlatformDefaults()
        {
            // Mirror what Xamarin.Android does: set up locale, logging, etc.
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture =
                System.Globalization.CultureInfo.InvariantCulture;

            // Route MissionPlanner's log4net output to the console / Unity log.
            var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)
                log4net.LogManager.GetRepository(typeof(UnityApp).Assembly);
            if (!hierarchy.Configured)
            {
                var layout = new log4net.Layout.PatternLayout();
                layout.ConversionPattern = "%-5level [%logger] - %message%newline";
                layout.ActivateOptions();
                var appender = new log4net.Appender.ConsoleAppender { Layout = layout };
                appender.ActivateOptions();
                hierarchy.Root.AddAppender(appender);
                hierarchy.Root.Level = log4net.Core.Level.Debug;
                hierarchy.Configured = true;
            }
        }
    }
}
