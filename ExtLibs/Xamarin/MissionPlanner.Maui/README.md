# MissionPlanner.Maui

.NET MAUI single-project head for Mission Planner. This replaces the four legacy
Xamarin heads with one cross-platform project:

| Legacy (Xamarin)      | Replaced by (this project)                 |
| --------------------- | ------------------------------------------ |
| `Xamarin.Android`     | `net8.0-android` target                    |
| `Xamarin.iOS`         | `net8.0-ios` target                        |
| `Xamarin.MacOS`       | `net8.0-maccatalyst` target                |
| `Xamarin.UWP`         | `net8.0-windows10.0.19041.0` target        |

## Status — Phase 1 scaffold + Phase 2 render loop

What is in place:

- MAUI single project (`MissionPlanner.Maui.csproj`) targeting Android / iOS /
  MacCatalyst / Windows on net8.0.
- App bootstrap: `MauiProgram`, `App`, `AppShell` (Shell flyout replaces the old
  `MasterDetailPage`).
- `GCSViews/WinFormsHostPage` — full MAUI port of the old `GCSViews/WinForms`
  render loop (touch→WinForms message translation, the SkiaSharp paint loop via
  `FormsRender.DrawOntoCanvas`, `Speech`/`Browser` services), using
  `SkiaSharp.Views.Maui.Controls.SKGLView` (was `SkiaSharp.Views.Forms`).
- Platform entry points under `Platforms/`.
- References: `MissionPlanner.Drawing` (SkiaSharp `System.Drawing`, net8),
  `MissionPlannerLib` (provides `FormsRender` via `ZZZLibShims.cs`), the Mono
  `System.Windows.Forms` driver (netstandard2.0), and the linked Xamarin-free
  helper `ITest.cs` (`Test`). SkiaSharp **2.88.x**.

See **`PHASE2-NOTES.md`** for the render-loop port details, the dependency
analysis (why the port is mostly net8-viable and the Xamarin.Forms WinForms
bridge is not needed), and the remaining work.

## Dependency migration map

| Legacy package              | MAUI replacement                          |
| --------------------------- | ----------------------------------------- |
| `SkiaSharp.Views.Forms`     | `SkiaSharp.Views.Maui.Controls`           |
| `Xamarin.Essentials`        | built-in MAUI Essentials                  |
| `Acr.UserDialogs`           | `CommunityToolkit.Maui` dialogs           |
| `Xamarin.Plugin.FilePicker` | `CommunityToolkit.Maui` / MAUI FilePicker |
| `Xam.Plugin.TabView`        | MAUI `TabbedPage` / Shell tabs            |
| `Xamarin.Forms.DataGrid`    | (choose a MAUI DataGrid; TODO)            |

## Remaining work (Phase 2+)

1. Retarget the shared `Xamarin` project (`ExtLibs/Xamarin/Xamarin`) from
   `netstandard2.0` + Xamarin.Forms to net8 + MAUI, then add a `ProjectReference`
   to it here.
2. Replace the placeholder paint/touch handlers in `WinFormsHostPage.xaml.cs`
   with the real Mono `System.Windows.Forms` render loop from the legacy
   `Xamarin/GCSViews/WinForms.xaml.cs` (hosted on `MissionPlanner.Drawing`).
3. Get Mono's `System.Windows.Forms` (the `ExtLibs/mono` submodule) building
   against net8.
4. Port the remaining pages (Video, Firmware, FlightData) and finalize
   dependency replacements above.
5. Drop in real branded assets: `Resources/AppIcon`, `Resources/Splash`,
   `Resources/Fonts` (OpenSans), and Windows tile logos.

## Building

Requires the .NET 8 SDK with the MAUI workload:

```
dotnet workload install maui
dotnet build ExtLibs/Xamarin/MissionPlanner.Maui/MissionPlanner.Maui.csproj -f net8.0-android
```

> Note: this scaffold was authored in an environment without the .NET SDK, so it
> has **not** been compile-verified. Expect to resolve workload/SDK and asset
> issues on first local build.
