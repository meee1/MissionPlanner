# MissionPlannerTests.WinForms

Cross-platform GUI tests for Mission Planner's **WinForms controls**, running on
plain **.NET 8** — no Mono runtime and no display server (X11/Xvfb) required.

## How this is possible

Modern .NET has no built-in `System.Windows.Forms` (it is Windows-only in
.NET 5+). Mission Planner ships its own cross-platform WinForms via the
**`ExtLibs/mono` git submodule** (`meee1/mono`): Mono's `System.Windows.Forms`
is compiled to **netstandard2.0** and rendered with **SkiaSharp** through
`MissionPlanner.Drawing.Common`. The `netstandard2.0` flavour of
`MissionPlanner.Controls` references that assembly, so a .NET 8 test project can
instantiate and even **render** the real controls (`MyButton`, `HSI`, …) on
Linux/macOS/Windows alike.

## Running

```bash
# one-time: fetch the vendored Mono WinForms (~647 MB, shallow)
git submodule update --init --depth 1 ExtLibs/mono

tests/run-winforms-tests.sh
```

The runner initialises the submodule if needed.

## Notes / requirements

- **Submodule**: `ExtLibs/mono` must be checked out; the runner/CI do this.
- **Native renderer**: `SkiaSharp.NativeAssets.Linux` supplies `libSkiaSharp.so`
  for the Linux CI runner (the WinForms paint backend).
- **System.Drawing**: the real `System.Drawing.Common` is excluded
  (`ExcludeAssets="all"`) so the Skia-based `MissionPlanner.Drawing` is the sole
  provider of `System.Drawing.*`; otherwise two `System.Drawing.Bitmap` types
  collide and break controls that load bitmaps in their constructor.
- **Signing**: `ExtLibs/MissionPlanner.Drawing.Common/open.snk` (an OSS
  strong-name key) is committed so the cross-platform build signs cleanly with
  no command-line flags. The vendored Mono `System.Windows.Forms` public-signs
  with its own `ecma.pub`.
- **Scope**: this covers the **netstandard2.0** control set only. The net472
  main application (with its Win32 P/Invokes, DirectInput/DirectShow, OpenGL HUD)
  is not loadable this way and remains Windows-only.
- This project is intentionally **not** part of `MissionPlanner.sln`, to avoid
  coupling the main Windows build to the submodule.

## What's covered

`WinFormsControlTests`:

- the loaded `System.Windows.Forms` really is the vendored Mono assembly;
- **every** public concrete control in `MissionPlanner.Controls` (~42) is
  instantiated by reflection (zero failures);
- every non-dialog control can be hosted on a `Form`;
- `MyButton` subclasses `Button` and round-trips its colours; `HSI` heading and
  `MyTrackBar`/`HorizontalProgressBar` values round-trip;
- a representative set of controls renders to a bitmap through the
  Mono-WinForms + SkiaSharp paint path.
