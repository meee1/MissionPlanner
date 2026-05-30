# MissionPlannerTests.App

Cross-platform tests for **real Mission Planner application logic**, running on
plain **.NET 8** — no Mono runtime, no display server.

## How this is possible

`MissionPlannerLib` is the **entire Mission Planner application compiled as a
netstandard2.0 library** (same root sources as the `MissionPlanner.csproj`
Windows executable, with the `LIB` define; see `MissionPlannerLib.csproj`). A
.NET 8 test project can reference it and call genuine main-app code directly —
not just the `ExtLibs/*` libraries.

This complements the other tiers:

| Tier | What it covers |
|------|----------------|
| `MissionPlannerTests.Unit` | pure logic in `ExtLibs/*` |
| `MissionPlannerTests.WinForms` | the WinForms controls in `ExtLibs/Controls` |
| **`MissionPlannerTests.App`** | **main-app code in `MissionPlannerLib` (root sources)** |
| `MissionPlannerTests.Sitl` / `.SitlPlane` | MAVLink against ArduPilot SITL |

## Running

```bash
git submodule update --init --depth 1 ExtLibs/mono   # one-time (vendored WinForms)
tests/run-app-tests.sh
```

## What's covered

- **`MagCalibTests`** — the `MagCalib` magnetometer hard-iron least-squares
  solver (root `MagCalib.cs`, using alglib). Synthetic sphere samples with a
  known centre are fed in and the recovered offsets/radius are asserted, for
  both the sphere and ellipsoid fits.
- **`ExtensionsMPTests`** — the `ExtensionsMP` percent↔pixel layout conversions
  (root `Utilities/ExtensionsMP.cs`), including round-trips, which also exercises
  the vendored WinForms stack through the app library.

This tier is intentionally seeded with dependency-light, deterministic logic;
it is straightforward to extend with more `MissionPlanner.*` classes.

## Notes / requirements

- **Submodule**: `ExtLibs/mono` (vendored Mono WinForms) must be checked out.
- **Native renderer**: `SkiaSharp.NativeAssets.Linux` supplies `libSkiaSharp.so`
  (creating any WinForms control initialises SkiaSharp).
- **System.Drawing**: the real `System.Drawing.Common` is excluded so the Skia
  `MissionPlanner.Drawing` shim is the sole `System.Drawing` provider.
- **Satellite assemblies**: the app's reference closure pulls in dozens of
  localized `*.resources.dll` satellites (duplicated across reference paths,
  which clash on copy); a build target drops them — tests need none.
- **Scope**: only code reachable without Windows-only native dependencies
  (no DirectInput/DirectShow/Win32) is exercised. Constructing the full
  `MainV2`/GUI is out of scope; targeted logic classes are the focus.
- Not part of `MissionPlanner.sln`, to avoid coupling the Windows build.
