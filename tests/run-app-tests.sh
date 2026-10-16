#!/usr/bin/env bash
#
# Run the cross-platform application-logic tests.
#
# These exercise real Mission Planner main-app code (e.g. the MagCalib
# magnetometer solver, ExtensionsMP layout helpers) on plain .NET 8 by
# referencing MissionPlannerLib - the whole app compiled as a netstandard2.0
# library. No Mono runtime, no display server.
#
# Requirements:
#   - the ExtLibs/mono submodule must be initialised:
#       git submodule update --init --depth 1 ExtLibs/mono
#
# Usage: tests/run-app-tests.sh [extra dotnet test args...]
set -euo pipefail

cd "$(dirname "$0")/.."

if [ ! -f ExtLibs/mono/mcs/class/System.Windows.Forms/System.Windows.Forms-net_4_x.csproj ]; then
  echo "==> Initialising ExtLibs/mono submodule (vendored Mono WinForms)"
  git submodule update --init --depth 1 ExtLibs/mono
fi

dotnet test MissionPlannerTests.App/MissionPlannerTests.App.csproj \
  -f net8.0 \
  -c Debug \
  --filter "TestCategory=App" \
  --logger "trx;LogFileName=app.trx" \
  --results-directory "TestResults" \
  "$@"
