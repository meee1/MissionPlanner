// ControlExtensions.cs
// Extension / helper methods that expose WinForms Control internals needed by
// the Unity renderer without requiring direct references to Windows Forms
// implementation details.
//
// WinForms' Click, MouseDown, MouseUp, and MouseMove events are raised through
// protected On* methods.  We invoke them here via reflection to avoid forking
// the System.Windows.Forms source.

using System;
using System.Reflection;
using System.Windows.Forms;

namespace MissionPlanner.Unity.Forms
{
    internal static class ControlExtensions
    {
        private static readonly BindingFlags _flags =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>Raises the Click event on <paramref name="c"/>.</summary>
        public static void InvokeOnClick(this Control c, System.Drawing.Point pt)
        {
            Invoke(c, "OnClick", EventArgs.Empty);
        }

        /// <summary>Raises the MouseDown event on <paramref name="c"/>.</summary>
        public static void InvokeMouseDown(this Control c, MouseEventArgs args)
        {
            Invoke(c, "OnMouseDown", args);
        }

        /// <summary>Raises the MouseUp event on <paramref name="c"/>.</summary>
        public static void InvokeMouseUp(this Control c, MouseEventArgs args)
        {
            Invoke(c, "OnMouseUp", args);
        }

        /// <summary>Raises the MouseMove event on <paramref name="c"/>.</summary>
        public static void InvokeMouseMove(this Control c, MouseEventArgs args)
        {
            Invoke(c, "OnMouseMove", args);
        }

        // ------------------------------------------------------------------ //

        private static void Invoke(Control control, string methodName, EventArgs args)
        {
            try
            {
                var m = control.GetType().GetMethod(methodName, _flags);
                m?.Invoke(control, new object[] { args });
            }
            catch (Exception ex)
            {
                // Swallow reflection errors gracefully; they indicate a missing
                // override rather than a critical fault.
#if UNITY_ENGINE_PRESENT
                UnityEngine.Debug.LogWarning(
                    $"[Unity] ControlExtensions.Invoke({methodName}): {ex.Message}");
#else
                Console.Error.WriteLine($"ControlExtensions.Invoke({methodName}): {ex.Message}");
#endif
            }
        }
    }
}
