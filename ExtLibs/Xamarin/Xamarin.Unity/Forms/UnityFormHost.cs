// UnityFormHost.cs
// Top-level host that owns the root Unity Canvas and routes WinForms
// Form/UserControl trees into Unity UI GameObjects.
//
// Architecture (mirrors Xamarin.Android):
//
//   Xamarin.Android                    Xamarin.Unity
//   ─────────────────────────────────  ──────────────────────────────────────
//   FormsAppCompatActivity             UnityMain (MonoBehaviour)
//   Xamarin.Forms.Application          UnityApp
//   ContentPage / StackLayout          UnityFormHost   ← this file
//   Custom drawn view                  UnityControlRenderer (per control)
//   Android View system                Unity Canvas / RectTransform
//
// Each System.Windows.Forms.Control gets a corresponding UnityControlRenderer
// that owns a child GameObject with a RawImage component.  Paint events drive
// System.Drawing into a per-control Texture2D that is uploaded each frame.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
using UnityEngine.UI;
#endif

namespace MissionPlanner.Unity.Forms
{
    /// <summary>
    /// Creates and manages the Unity scene hierarchy that renders a WinForms
    /// <see cref="Form"/> and all its child controls.
    /// </summary>
    public sealed class UnityFormHost : IDisposable
    {
        private readonly int  _width;
        private readonly int  _height;
        private          UnityApp?  _app;
        private          bool       _disposed;

        // Renderer tree: one entry per visible Control.
        private readonly Dictionary<Control, UnityControlRenderer> _renderers
            = new Dictionary<Control, UnityControlRenderer>();

#if UNITY_ENGINE_PRESENT
        private readonly GameObject _hostGo;   // root scene object
        private          Canvas?    _canvas;
#else
        private readonly object _hostGo;
#endif

        // ------------------------------------------------------------------ //

#if UNITY_ENGINE_PRESENT
        public UnityFormHost(GameObject parent, int width, int height)
        {
            _width  = width;
            _height = height;
            _hostGo = new GameObject("MissionPlannerCanvas");
            _hostGo.transform.SetParent(parent.transform, false);
        }
#else
        public UnityFormHost(object parent, int width, int height)
        {
            _width  = width;
            _height = height;
            _hostGo = parent;
        }
#endif

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Boots the MissionPlanner app and sets up the initial render tree.
        /// Call once from <see cref="UnityMain.Boot"/>.
        /// </summary>
        public void Launch()
        {
            SetupCanvas();

            _app = new UnityApp();
            _app.Start();

            if (_app.MainForm != null)
                TraverseAndRegister(_app.MainForm);
        }

        /// <summary>
        /// Per-frame update – flushes dirty textures, propagates input events.
        /// Call from <see cref="UnityMain.Update"/>.
        /// </summary>
        public void Tick()
        {
            foreach (var kvp in _renderers)
            {
                if (kvp.Value.IsDirty)
                    kvp.Value.Repaint();
                kvp.Value.FlushTexture();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var r in _renderers.Values)
                r.Dispose();
            _renderers.Clear();

#if UNITY_ENGINE_PRESENT
            UnityEngine.Object.Destroy(_hostGo);
#endif
        }

        // ------------------------------------------------------------------ //
        //  Canvas setup                                                        //
        // ------------------------------------------------------------------ //

        private void SetupCanvas()
        {
#if UNITY_ENGINE_PRESENT
            _canvas = _hostGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = _hostGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution  = new Vector2(_width, _height);
            scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight   = 0.5f;

            _hostGo.AddComponent<GraphicRaycaster>();
#endif
        }

        // ------------------------------------------------------------------ //
        //  Control tree traversal                                              //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Recursively walks the WinForms control tree and creates a
        /// <see cref="UnityControlRenderer"/> for each visible <see cref="Control"/>.
        /// </summary>
        private void TraverseAndRegister(Control control,
#if UNITY_ENGINE_PRESENT
            GameObject? parentGo = null)
#else
            object? parentGo = null)
#endif
        {
            if (control == null || !control.Visible) return;

            var renderer = new UnityControlRenderer(control,
#if UNITY_ENGINE_PRESENT
                parentGo ?? _hostGo
#else
                parentGo ?? _hostGo
#endif
            );
            _renderers[control] = renderer;

            foreach (Control child in control.Controls)
#if UNITY_ENGINE_PRESENT
                TraverseAndRegister(child, renderer.GameObject);
#else
                TraverseAndRegister(child, null);
#endif

            // Listen for new child controls being added at runtime.
            control.ControlAdded += (_, e) =>
            {
                if (e.Control != null)
#if UNITY_ENGINE_PRESENT
                    TraverseAndRegister(e.Control, renderer.GameObject);
#else
                    TraverseAndRegister(e.Control, null);
#endif
            };

            control.ControlRemoved += (_, e) =>
            {
                if (e.Control != null && _renderers.TryGetValue(e.Control, out var r))
                {
                    r.Dispose();
                    _renderers.Remove(e.Control);
                }
            };
        }
    }
}
