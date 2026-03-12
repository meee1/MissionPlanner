// UnityApp.cs
// Application-level initialisation – equivalent to the Xamarin.Forms App class.
//
// Responsible for:
//   • registering platform services (serial, settings, …)
//   • creating the top-level MissionPlanner.MainV2 form
//   • handing the form tree to the UnityFormHost

using System;
using MissionPlanner;
using MissionPlanner.Utilities;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Unity
{
    /// <summary>
    /// Bootstraps the MissionPlanner application object inside Unity.
    /// </summary>
    public sealed class UnityApp
    {
        // The shared platform-independent MainV2 form (System.Windows.Forms.Form).
        public System.Windows.Forms.Form? MainForm { get; private set; }

        public void Start()
        {
            // Apply any platform-specific DPI / font overrides before the form
            // tree is constructed.
            ApplyPlatformDefaults();

            // Instantiate the main WinForms form.  MainV2 is the root form of
            // MissionPlanner; it creates all child views in its constructor.
            MainForm = new MainV2();

#if UNITY_ENGINE_PRESENT
            Debug.Log($"[MissionPlanner] MainForm created: {MainForm.GetType().Name}");
#endif
        }

        private static void ApplyPlatformDefaults()
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
