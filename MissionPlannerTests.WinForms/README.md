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
- **Signing**: built with `-p:SignAssembly=false` because the referenced
  `MissionPlanner.Drawing.Common` signs with `open.snk`, which is not committed
  to the repo. Signing is irrelevant for these tests.
- **Native renderer**: `SkiaSharp.NativeAssets.Linux` supplies `libSkiaSharp.so`
  for the Linux CI runner (the WinForms paint backend).
- **Scope**: this covers the **netstandard2.0** control set only. The net472
  main application (with its Win32 P/Invokes, DirectInput/DirectShow, OpenGL HUD)
  is not loadable this way and remains Windows-only.
- This project is intentionally **not** part of `MissionPlanner.sln`, to avoid
  coupling the main Windows build to the submodule + signing workaround.

## What's covered

`WinFormsControlTests` — the WinForms assembly really is the vendored Mono one,
forms host controls, `MyButton` subclasses `Button` and round-trips its colours,
`HSI` heading properties round-trip, and a control renders to a bitmap through
the Mono-WinForms + SkiaSharp paint path.
