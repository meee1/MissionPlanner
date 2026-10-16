# Phase 2 — shared project retarget + render-loop port

## What landed in Phase 2

- **Render loop ported** to `GCSViews/WinFormsHostPage.xaml.cs` — a faithful MAUI
  translation of the legacy `ExtLibs/Xamarin/Xamarin/GCSViews/WinForms.xaml.cs`:
  - `Xamarin.Forms` → `Microsoft.Maui.Controls`
  - `SkiaSharp.Views.Forms` → `SkiaSharp.Views.Maui`
  - `Xamarin.Essentials` → `Microsoft.Maui.{Devices,Media,ApplicationModel}`
    (`TextToSpeech.Default`, `Browser.Default`, `DeviceInfo`, `DeviceDisplay`)
  - `Device.RuntimePlatform` → `DeviceInfo.Platform` / `DevicePlatform`
  - `Device.BeginInvokeOnMainThread` → `MainThread.BeginInvokeOnMainThread`
  - `Device.StartTimer` → `IDispatcher.StartTimer`
  - The `Speech`, `Browser`, `TouchInfo` helper classes were ported too.
- **References wired** in `MissionPlanner.Maui.csproj`: `MissionPlannerLib` (which
  already compiles `ZZZLibShims.cs`, so `FormsRender` resolves through it), the Mono
  `System.Windows.Forms` driver, and `MissionPlanner.Drawing`. `ITest.cs` (`Test`) is
  linked from the legacy tree because `ExtLibs\Xamarin\**` is excluded from
  MissionPlannerLib.

## Key architectural finding — the port is mostly net8-viable

While wiring this up, the dependency analysis came out much better than expected:

| Dependency | Target | net8 / MAUI status |
| --- | --- | --- |
| Mono `System.Windows.Forms` | **netstandard2.0** | ✅ consumable from net8 as-is |
| `FormsRender.DrawOntoCanvas` (ZZZLibShims.cs) | — | ✅ SkiaSharp + WinForms + System.Drawing only, **no Xamarin.Forms** |
| `Test` facade (ITest.cs) | — | ✅ Xamarin.Forms-free |
| `MissionPlannerLib` | netstandard2.0 | ✅ consumable from net8 |
| `MissionPlanner.Drawing` | netstandard2.0;**net8.0** | ✅ (multi-targeted in Phase 0) |
| `Xamarin.Forms.Platform.WinForms` bridge | net472 + Xamarin.Forms | ❌ **not needed** under MAUI |

The only Xamarin.Forms-coupled thing the render loop touched was
`Xamarin.Forms.Platform.WinForms.Forms.UIThread`, a single static `int` thread-id
field. The actual WinForms→Skia rendering (`FormsRender.DrawOntoCanvas`) does **not**
use Xamarin.Forms. So the bridge project is dead weight under MAUI — the port now
keeps the UI-thread id in a local `WinFormsHostPage.UIThread` field and drops the
bridge entirely.

## "Retarget the shared Xamarin project" — decision

The legacy `ExtLibs/Xamarin/Xamarin` shared project was **not** rewritten in place,
on purpose:

1. It is being **superseded** by this `MissionPlanner.Maui` project — the migration
   moves code here incrementally (the render loop is the first big move) rather than
   mutating ~30 Xamarin.Forms XAML/code files that would be a large, unverifiable,
   guaranteed-broken diff.
2. Leaving it intact keeps the old heads building during the transition.

Its reusable, Xamarin.Forms-free pieces (`ITest.cs`, `ZZZLibShims.cs`) are **linked**
here instead of copied, so there is a single source of truth.

## Remaining work

1. **Build & verify on net8** (needs the .NET 8 SDK + MAUI workload — not available in
   the authoring environment). Resolve the dependency closure of `ZZZLibShims.cs`
   (e.g. `Microsoft.Scripting.Hosting`/IronPython via MissionPlannerLib) and any
   netstandard2.0→net8 type gaps.
2. **App bootstrap**: replace the placeholder `AppShell` content / register the real
   pages; decide whether `MissionPlanner.Program.Main` is launched from
   `WinFormsHostPage.StartThreads` (as ported) or from `MauiProgram`.
3. **Port remaining pages** still living in the legacy shared project: `MainPage`
   (master/detail → Shell), `Video`, `Firmware`, `FlightData`, and the menu.
4. **Platform services** (`Test.UsbDevices` / `Test.BlueToothDevice` / `Test.GPS` /
   `Test.Speech`): port the per-platform implementations from `Xamarin.Android` /
   `Xamarin.iOS` / `Xamarin.MacOS` / `Xamarin.UWP` into this project's `Platforms/`.
5. **Dependency replacements**: finish swapping `Acr.UserDialogs`,
   `Xam.Plugin.TabView`, `Xamarin.Forms.DataGrid`, `Xamarin.Plugin.FilePicker`
   (see the table in `README.md`).
6. **Assets**: real branded icon/splash/fonts and Windows tile logos.
7. Once everything is moved, **retire** the legacy `Xamarin`, `Xamarin.Android`,
   `Xamarin.iOS`, `Xamarin.MacOS`, `Xamarin.UWP` projects and the
   `Xamarin.Forms.Platform.WinForms` bridge.
