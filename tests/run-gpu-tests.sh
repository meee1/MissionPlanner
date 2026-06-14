#!/usr/bin/env bash
#
# Run the GPU render tests against a headless SOFTWARE GL context
# (Mesa llvmpipe via an EGL surfaceless context) - no display, no real GPU.
# This actually exercises the SkiaSharp GPU path (GRContext + GPU SKSurface),
# unlike the CPU-fallback tests in the normal WinForms tier.
#
# Requirements:
#   - libEGL + a Mesa software driver:
#       sudo apt-get install -y libegl1 libgl1-mesa-dri libgles2
#   - the vendored Mono WinForms submodule (ExtLibs/mono).
#
# Usage: tests/run-gpu-tests.sh [extra dotnet test args...]
set -euo pipefail

cd "$(dirname "$0")/.."

if [ ! -f ExtLibs/mono/mcs/class/System.Windows.Forms/System.Windows.Forms-net_4_x.csproj ]; then
  echo "==> Initialising ExtLibs/mono submodule (vendored Mono WinForms)"
  git submodule update --init --depth 1 ExtLibs/mono
fi

# Force Mesa software rendering through a surfaceless EGL context.
export LIBGL_ALWAYS_SOFTWARE=1
export GALLIUM_DRIVER=llvmpipe
export EGL_PLATFORM=surfaceless
export MESA_GL_VERSION_OVERRIDE=3.3
export MESA_GLES_VERSION_OVERRIDE=3.0

dotnet test MissionPlannerTests.WinForms/MissionPlannerTests.WinForms.csproj \
  -f net8.0 \
  -c Debug \
  --filter "TestCategory=Gpu" \
  --logger "trx;LogFileName=gpu.trx" \
  --results-directory "TestResults" \
  "$@"
