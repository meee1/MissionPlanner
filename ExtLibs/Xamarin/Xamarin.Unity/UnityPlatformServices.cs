// UnityPlatformServices.cs
// Registers Unity-specific implementations of the platform-service interfaces
// that MissionPlanner expects (file I/O paths, settings path, etc.).
//
// Mirrors the Xamarin.Android pattern where platform services are set up in
// MainActivity.Boot() before Forms.Init().

using System;
using System.IO;
using MissionPlanner.Utilities;
using Interfaces;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Unity
{
    public static class UnityPlatformServices
    {
        private static bool _registered;

        /// <summary>
        /// Call once from <see cref="UnityMain.Boot"/> before any MissionPlanner
        /// code runs.
        /// </summary>
        public static void Register()
        {
            if (_registered) return;
            _registered = true;

            // ----------------------------------------------------------------
            // Base paths
            // ----------------------------------------------------------------
#if UNITY_ENGINE_PRESENT
            string persistentPath = Application.persistentDataPath;
            string streamingPath  = Application.streamingAssetsPath;
#else
            string persistentPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MissionPlanner");
#endif

            Directory.CreateDirectory(persistentPath);

            // Inform MissionPlanner where to store user data.
            Settings.CustomUserDataDirectory = persistentPath;

            // ----------------------------------------------------------------
            // Log initialisation
            // ----------------------------------------------------------------
#if UNITY_ENGINE_PRESENT
            Debug.Log($"[MissionPlanner] PersistentPath = {persistentPath}");
            Debug.Log($"[MissionPlanner] StreamingPath  = {streamingPath}");
#endif
        }
    }
}
