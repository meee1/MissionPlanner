// UnityMain.cs
// Unity entry point for MissionPlanner – analogous to MainActivity.cs in
// Xamarin.Android.
//
// Attach this MonoBehaviour to the root GameObject in your Unity scene.
// It bootstraps the shared MissionPlannerLib, wires up platform services,
// and hands control to the UnityFormHost which renders the WinForms UI tree
// onto Unity Canvas objects.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MissionPlanner;
using MissionPlanner.Utilities;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Unity
{
#if UNITY_ENGINE_PRESENT
    /// <summary>
    /// Root MonoBehaviour – attach to the scene's bootstrap GameObject.
    /// </summary>
    public sealed class UnityMain : MonoBehaviour
    {
        [Header("Display")]
        [Tooltip("Target rendering resolution (logical pixels).")]
        public int displayWidth  = 1280;
        public int displayHeight = 800;

        [Tooltip("Background colour shown before the first paint.")]
        public Color backgroundColor = Color.black;

        // ------------------------------------------------------------------ //
        private UnityFormHost? _formHost;
        private bool           _initialised;

        // ------------------------------------------------------------------ //
        //  Unity lifecycle                                                     //
        // ------------------------------------------------------------------ //

        private void Awake()
        {
            // Keep the app alive across scene loads.
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Initialise on the main thread so Unity APIs are available.
            try
            {
                Boot();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void Update()
        {
            if (!_initialised) return;

            try
            {
                _formHost?.Tick();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void OnDestroy()
        {
            _formHost?.Dispose();
        }

        // ------------------------------------------------------------------ //
        //  Boot                                                                //
        // ------------------------------------------------------------------ //

        private void Boot()
        {
            UnityApp.ApplyPlatformDefaults();
            UnityPlatformServices.Register();

            // MissionPlanner uses a static "Settings" object – point it at the
            // persistent data path so saves survive app restarts.
            var settingsDir = Application.persistentDataPath;
            Directory.CreateDirectory(settingsDir);
            Settings.Instance = new Settings(
                Path.Combine(settingsDir, "missionplanner.xml"), false);

            // Create the form host – this is the equivalent of calling
            //   Forms.Init();  LoadApplication(new App());
            // in a Xamarin.Android MainActivity.
            _formHost = new UnityFormHost(gameObject, displayWidth, displayHeight);
            _formHost.Launch();

            _initialised = true;

            Debug.Log("[MissionPlanner] Unity host initialised.");
        }
    }

#else
    // Stub so the file compiles without UnityEngine referenced (CI / IDE).
    public sealed class UnityMain
    {
        public int displayWidth  = 1280;
        public int displayHeight = 800;
    }
#endif
}
