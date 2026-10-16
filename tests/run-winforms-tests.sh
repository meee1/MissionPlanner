#!/usr/bin/env bash
#
# Run the cross-platform WinForms control tests.
#
# These instantiate and render Mission Planner's WinForms controls on plain
# .NET 8 using the vendored Mono System.Windows.Forms (the ExtLibs/mono
# submodule), rendered via SkiaSharp - no Mono runtime, no display server.
#
# Requirements:
#   - the ExtLibs/mono submodule must be initialised:
#       git submodule update --init --depth 1 ExtLibs/mono
#
# Usage: tests/run-winforms-tests.sh [extra dotnet test args...]
set -euo pipefail

cd "$(dirname "$0")/.."

if [ ! -f ExtLibs/mono/mcs/class/System.Windows.Forms/System.Windows.Forms-net_4_x.csproj ]; then
  echo "==> Initialising ExtLibs/mono submodule (vendored Mono WinForms)"
  git submodule update --init --depth 1 ExtLibs/mono
fi

PROJECT="MissionPlannerTests.WinForms/MissionPlannerTests.WinForms.csproj"

dotnet test "$PROJECT" \
  -f net8.0 \
  -c Debug \
  --filter "TestCategory=WinForms" \
  --logger "trx;LogFileName=winforms.trx" \
  --results-directory "TestResults" \
  "$@"
