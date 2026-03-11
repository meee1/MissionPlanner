# Xamarin.Unity – MissionPlanner Unity Render Host

This project provides a **Unity** backend for MissionPlanner, analogous to
`Xamarin.Android`.  It lets the shared `MissionPlannerLib` codebase run inside a
Unity application with its `System.Windows.Forms` UI rendered through Unity's
Canvas / UI system.

---

## Architecture overview

```
MissionPlannerLib (netstandard2.0)
    ↑ shared logic
    │
    ├─ Xamarin.Android  (monoandroid13.0)   ← existing Android build
    │      Xamarin.Forms + Android View system
    │
    └─ Xamarin.Unity    (netstandard2.0)    ← this project
           System.Drawing (Bitmap) → Texture2D → Unity Canvas
```

### How WinForms controls are rendered

```
System.Windows.Forms.Control
        │  OnPaint(PaintEventArgs)
        │       ↓
        │  System.Drawing.Graphics   (MissionPlanner.Drawing)
        │       ↓ paints into System.Drawing.Bitmap pixel memory
        │  Bitmap.LockBits() → byte[] BGRA32
        │       ↓
        │  UnityEngine.Texture2D.LoadRawTextureData()
        │       ↓
        └─ UnityEngine.UI.RawImage   ← displayed in scene
```

Each `Control` in the WinForms tree gets its own `UnityControlRenderer` which
owns:
- a `UnityGraphicsSurface` (Bitmap backing store + Texture2D)
- a child `GameObject` with a `RectTransform` sized/positioned to match the
  WinForms layout
- a `RawImage` component that displays the per-control texture

Input events (pointer click/down/up/drag) are forwarded back to the WinForms
control via `Control.OnMouse*` reflection calls.

---

## Quick-start (Unity Editor)

### 1. Build the managed DLLs

```bash
# From the MissionPlanner repo root:
dotnet build MissionPlannerLib.sln -c Release
```

Copy the following DLLs into your Unity project's `Assets/Plugins/` folder:

| DLL | Source |
|-----|--------|
| `MissionPlannerLib.dll` | `bin/Release/netstandard2.0/` |
| `MissionPlanner.Drawing.dll` | `ExtLibs/MissionPlanner.Drawing/bin/…` |
| `MissionPlanner.Drawing.Unity.dll` | `ExtLibs/MissionPlanner.Drawing.Unity/bin/…` |
| `System.Windows.Forms.dll` | `ExtLibs/mono/mcs/class/System.Windows.Forms/bin/…` |

### 2. Copy the Unity scripts

Copy the entire `Xamarin.Unity/` folder into `Assets/MissionPlanner/` in your
Unity project.  Unity will compile the scripts automatically.

### 3. Add the bootstrap MonoBehaviour

1. Create an empty GameObject in your scene (e.g. `MissionPlannerHost`).
2. Attach the `Xamarin.Unity.UnityMain` component.
3. Set **Display Width** / **Display Height** to your target resolution.
4. Press **Play**.

### 4. Streaming Assets

Place MissionPlanner resource files (config XMLs, map tiles, firmware, etc.)
in `Assets/StreamingAssets/` – `Application.streamingAssetsPath` is mapped to
`MissionPlanner.Utilities.Misc.StartupPath`.

---

## Connecting to an autopilot / SITL

Use `Xamarin.Unity.Comms.UnityTcpSerial` to connect over TCP:

```csharp
var serial = new UnityTcpSerial { Host = "127.0.0.1", Port = 5760 };
serial.Open();
MainV2.comPort.BaseStream = serial;
```

This mirrors how `AndroidSerial` provides the `ICommsSerial` implementation on
Android.

---

## Differences from Xamarin.Android

| Feature | Xamarin.Android | Xamarin.Unity |
|---------|-----------------|---------------|
| UI renderer | Xamarin.Forms + Android Views | Unity Canvas + RawImage |
| Drawing | System.Drawing → Android Bitmap | System.Drawing → Unity Texture2D |
| Serial | USB / Bluetooth (Hoho.Android) | TCP bridge / stub |
| Video | GStreamer Android plugin | Unity VideoPlayer |
| Permissions | AndroidManifest.xml | Unity Player Settings |
| Entry point | `MainActivity : FormsAppCompatActivity` | `UnityMain : MonoBehaviour` |

---

## Project files

```
Xamarin.Unity/
├── Xamarin.Unity.csproj       IDE / CI build project
├── Xamarin.Unity.asmdef       Unity Assembly Definition
├── UnityMain.cs               MonoBehaviour entry point
├── UnityApp.cs                Application initialisation
├── UnityPlatformServices.cs   Platform path / service registration
├── Forms/
│   ├── UnityFormHost.cs       Owns root Canvas, traverses control tree
│   ├── UnityControlRenderer.cs Per-control Bitmap→Texture2D renderer
│   └── ControlExtensions.cs   Reflection helpers for WinForms On* methods
├── Rendering/
│   └── UnityRenderer.cs       Low-level Bitmap/Graphics→Texture2D pipeline
└── Comms/
    └── UnitySerial.cs         TCP serial shim (SITL / MAVProxy)

ExtLibs/MissionPlanner.Drawing.Unity/
├── MissionPlanner.Drawing.Unity.csproj
├── MissionPlanner.Drawing.Unity.asmdef
├── UnityGraphics.cs           UnityGraphicsSurface + UnityGraphicsFactory
└── UnityBitmap.cs             Bitmap ↔ Texture2D conversions
```
