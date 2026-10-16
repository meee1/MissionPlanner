#!/usr/bin/env bash
#
# Run the fast, cross-platform Mission Planner regression unit tests.
#
# These tests reference only the netstandard2.0 logic libraries (no WinForms),
# so they build and run on Linux/macOS/Windows with the .NET 8 SDK. They are
# offline: the "Network" category (live downloads in the legacy test project)
# is intentionally excluded.
#
# Usage: tests/run-unit-tests.sh [extra dotnet test args...]
set -euo pipefail

cd "$(dirname "$0")/.."

PROJECT="MissionPlannerTests.Unit/MissionPlannerTests.Unit.csproj"

dotnet test "$PROJECT" \
  -f net8.0 \
  -c Debug \
  --filter "TestCategory=Unit" \
  --logger "trx;LogFileName=unit.trx" \
  --results-directory "TestResults" \
  "$@"
