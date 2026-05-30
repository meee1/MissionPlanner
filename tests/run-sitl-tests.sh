#!/usr/bin/env bash
#
# Run the headless ArduPilot SITL integration tests.
#
# Requires SITL binaries (see tests/build-sitl.sh) and SITL_BIN_DIR pointing at
# the directory that contains them. Without SITL_BIN_DIR the tests report as
# inconclusive (skipped) rather than failing.
#
# Usage:
#   export SITL_BIN_DIR=/path/to/ardupilot/build/sitl/bin
#   tests/run-sitl-tests.sh [extra dotnet test args...]
set -euo pipefail

cd "$(dirname "$0")/.."

PROJECT="MissionPlannerTests.Sitl/MissionPlannerTests.Sitl.csproj"

if [ -z "${SITL_BIN_DIR:-}" ]; then
  echo "warning: SITL_BIN_DIR is not set; SITL tests will be skipped (inconclusive)." >&2
fi

dotnet test "$PROJECT" \
  -f net8.0 \
  -c Debug \
  --filter "TestCategory=Sitl" \
  --logger "trx;LogFileName=sitl.trx" \
  --results-directory "TestResults" \
  "$@"
