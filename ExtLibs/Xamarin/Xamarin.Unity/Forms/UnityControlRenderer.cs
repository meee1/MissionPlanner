// UnityControlRenderer.cs
// Renders a single System.Windows.Forms.Control into a Unity RawImage.
//
// Each renderer:
//   • Creates a child GameObject with a RectTransform sized to the control.
//   • Owns a UnityGraphicsSurface (Bitmap → LockBits → Texture2D).
//   • Subscribes to Control.Paint / Invalidated events and marks itself dirty.
//   • Each frame (via Tick → FlushTexture) the dirty surface is uploaded to the GPU.
//   • Maps Unity pointer events back to WinForms MouseEventArgs.
//
// This is analogous to how Xamarin.Android's MySKCanvasView wraps a canvas view
// and a Xamarin.Forms.View into an Android native view.

using System;
using System.Drawing;
using System.Windows.Forms;
using MissionPlanner.Drawing.Unity;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#endif

namespace Xamarin.Unity.Forms
{
    /// <summary>
    /// Bridges a single <see cref="Control"/> to a Unity
    /// <c>RawImage</c> GameObject.
    /// </summary>
    public sealed class UnityControlRenderer : IDisposable
#if UNITY_ENGINE_PRESENT
        , IPointerClickHandler
        , IPointerDownHandler
        , IPointerUpHandler
        , IDragHandler
#endif
    {
        private readonly Control             _control;
        private readonly UnityGraphicsSurface _surface;
        private          bool                _disposed;

        public bool IsDirty { get; private set; } = true;

#if UNITY_ENGINE_PRESENT
        public  GameObject  GameObject  { get; }
        private RawImage?   _rawImage;
        private RectTransform? _rect;
#else
        public  object      GameObject  { get; } = new object();
#endif

        // ------------------------------------------------------------------ //

#if UNITY_ENGINE_PRESENT
        public UnityControlRenderer(Control control, GameObject parent)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _surface = new UnityGraphicsSurface();

            // ---- Create the GameObject ----
            GameObject = new UnityEngine.GameObject(control.Name ?? control.GetType().Name);
            GameObject.transform.SetParent(parent.transform, false);

            _rect = GameObject.AddComponent<RectTransform>();
            ApplyLayout();

            // RawImage displays the control's rendered Texture2D.
            _rawImage = GameObject.AddComponent<RawImage>();

            // Allow pointer events so we can forward them to WinForms.
            var eventTrigger = GameObject.AddComponent<EventTrigger>();
            var raycaster    = GameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Subscribe to layout and paint events.
            _control.Resize    += OnControlResize;
            _control.Paint     += OnControlPaint;
            _control.Invalidated += OnControlInvalidated;
            _control.LocationChanged += OnControlLocationChanged;

            // Initial size.
            _surface.Resize(
                Math.Max(1, _control.Width),
                Math.Max(1, _control.Height));
        }
#else
        public UnityControlRenderer(Control control, object parent)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _surface = new UnityGraphicsSurface();

            _control.Resize      += OnControlResize;
            _control.Paint       += OnControlPaint;
            _control.Invalidated += OnControlInvalidated;

            _surface.Resize(
                Math.Max(1, _control.Width),
                Math.Max(1, _control.Height));
        }
#endif

        // ------------------------------------------------------------------ //
        //  Per-frame                                                           //
        // ------------------------------------------------------------------ //

        /// <summary>Forces a repaint of this control into its backing Bitmap.</summary>
        public void Repaint()
        {
            if (_disposed) return;

            var g = _surface.CreateGraphics();
            // Clear to control's BackColor.
            var bg = _control.BackColor;
            g.Clear(bg);

            // Raise the WinForms paint event so the control draws itself.
            var args = new PaintEventArgs(g,
                new Rectangle(0, 0, _control.Width, _control.Height));

            // Invoke via reflection to access the protected OnPaint method.
            try
            {
                var method = _control.GetType().GetMethod("OnPaint",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
                method?.Invoke(_control, new object[] { args });
            }
            catch (Exception ex)
            {
#if UNITY_ENGINE_PRESENT
                UnityEngine.Debug.LogException(ex);
#else
                Console.Error.WriteLine(ex);
#endif
            }

            IsDirty = false;
        }

        /// <summary>
        /// Uploads the latest Bitmap pixels to the Unity Texture2D.
        /// Must be called on the Unity main thread.
        /// </summary>
        public void FlushTexture()
        {
            if (_disposed) return;

            _surface.FlushToTexture();

#if UNITY_ENGINE_PRESENT
            if (_rawImage != null && _surface.Texture != null)
                _rawImage.texture = _surface.Texture;
#endif
        }

        // ------------------------------------------------------------------ //
        //  WinForms event handlers                                             //
        // ------------------------------------------------------------------ //

        private void OnControlResize(object? sender, EventArgs e)
        {
            _surface.Resize(
                Math.Max(1, _control.Width),
                Math.Max(1, _control.Height));
            ApplyLayout();
            IsDirty = true;
        }

        private void OnControlPaint(object? sender, PaintEventArgs e)
        {
            // Control is painting via its own path – mark dirty so we upload
            // the result next frame.
            IsDirty = true;
        }

        private void OnControlInvalidated(object? sender, InvalidateEventArgs e)
        {
            IsDirty = true;
        }

        private void OnControlLocationChanged(object? sender, EventArgs e)
        {
            ApplyLayout();
        }

        // ------------------------------------------------------------------ //
        //  Layout helpers                                                      //
        // ------------------------------------------------------------------ //

        private void ApplyLayout()
        {
#if UNITY_ENGINE_PRESENT
            if (_rect == null) return;

            // WinForms uses top-left origin; Unity RectTransform uses bottom-left.
            // We use anchored position relative to the parent RectTransform.
            _rect.anchorMin = new Vector2(0, 1);   // top-left anchor
            _rect.anchorMax = new Vector2(0, 1);
            _rect.pivot     = new Vector2(0, 1);

            _rect.anchoredPosition = new Vector2(_control.Left, -_control.Top);
            _rect.sizeDelta        = new Vector2(_control.Width, _control.Height);
#endif
        }

        // ------------------------------------------------------------------ //
        //  Unity pointer event forwarding                                      //
        // ------------------------------------------------------------------ //

#if UNITY_ENGINE_PRESENT
        public void OnPointerClick(PointerEventData eventData)
        {
            var pt = ScreenToControl(eventData.position);
            _control.InvokeOnClick(pt);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var pt  = ScreenToControl(eventData.position);
            var btn = MapMouseButton(eventData.button);
            _control.InvokeMouseDown(new System.Windows.Forms.MouseEventArgs(
                btn, 1, pt.X, pt.Y, 0));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var pt  = ScreenToControl(eventData.position);
            var btn = MapMouseButton(eventData.button);
            _control.InvokeMouseUp(new System.Windows.Forms.MouseEventArgs(
                btn, 1, pt.X, pt.Y, 0));
        }

        public void OnDrag(PointerEventData eventData)
        {
            var pt = ScreenToControl(eventData.position);
            _control.InvokeMouseMove(new System.Windows.Forms.MouseEventArgs(
                System.Windows.Forms.MouseButtons.Left, 0, pt.X, pt.Y, 0));
        }

        // Maps a Unity screen-space position to control-local pixel coords.
        private System.Drawing.Point ScreenToControl(Vector2 screenPos)
        {
            if (_rect == null) return System.Drawing.Point.Empty;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect, screenPos, null, out var local);

            // local is in Unity UI space (origin at pivot); map to [0,w] × [0,h].
            int x = (int)(local.x);
            int y = (int)(-local.y);  // flip Y

            x = System.Math.Clamp(x, 0, _control.Width  - 1);
            y = System.Math.Clamp(y, 0, _control.Height - 1);
            return new System.Drawing.Point(x, y);
        }

        private static System.Windows.Forms.MouseButtons MapMouseButton(
            PointerEventData.InputButton b)
        {
            return b switch
            {
                PointerEventData.InputButton.Left   => System.Windows.Forms.MouseButtons.Left,
                PointerEventData.InputButton.Right  => System.Windows.Forms.MouseButtons.Right,
                PointerEventData.InputButton.Middle => System.Windows.Forms.MouseButtons.Middle,
                _                                   => System.Windows.Forms.MouseButtons.None,
            };
        }
#endif

        // ------------------------------------------------------------------ //

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _control.Resize          -= OnControlResize;
            _control.Paint           -= OnControlPaint;
            _control.Invalidated     -= OnControlInvalidated;
            _control.LocationChanged -= OnControlLocationChanged;

            _surface.Dispose();

#if UNITY_ENGINE_PRESENT
            if (GameObject != null)
                UnityEngine.Object.Destroy(GameObject);
#endif
        }
    }
}
