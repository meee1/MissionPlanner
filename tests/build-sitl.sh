#!/usr/bin/env bash
#
# Build ArduPilot SITL binaries from source for the integration tests.
#
# Produces arducopter / arduplane (etc.) under a directory you then point
# SITL_BIN_DIR at when running MissionPlannerTests.Sitl:
#
#   tests/build-sitl.sh
#   export SITL_BIN_DIR="$PWD/.sitl/ardupilot/build/sitl/bin"
#   tests/run-sitl-tests.sh
#
# Environment:
#   ARDUPILOT_REF   git ref to build (default: master)
#   SITL_SRC_DIR    where to clone ardupilot (default: ./.sitl/ardupilot)
#   SITL_VEHICLES   waf targets to build (default: "copter plane")
#
# Intended for Linux (CI: ubuntu-latest). Requires git and a build toolchain;
# the prereq installer below covers a clean Ubuntu runner.
set -euo pipefail

ARDUPILOT_REF="${ARDUPILOT_REF:-master}"
SITL_SRC_DIR="${SITL_SRC_DIR:-$PWD/.sitl/ardupilot}"
SITL_VEHICLES="${SITL_VEHICLES:-copter plane}"

if [ ! -d "$SITL_SRC_DIR/.git" ]; then
  echo "==> Cloning ArduPilot ($ARDUPILOT_REF) into $SITL_SRC_DIR"
  mkdir -p "$(dirname "$SITL_SRC_DIR")"
  git clone --recurse-submodules https://github.com/ArduPilot/ardupilot.git "$SITL_SRC_DIR"
fi

cd "$SITL_SRC_DIR"
git fetch --all --tags --quiet
git checkout "$ARDUPILOT_REF"
git submodule update --init --recursive

# Install build prerequisites (no-op if already satisfied). Safe to skip with
# SKIP_PREREQS=1 on a machine that is already set up.
if [ "${SKIP_PREREQS:-0}" != "1" ]; then
  echo "==> Installing prerequisites"
  Tools/environment_install/install-prereqs-ubuntu.sh -y
  # shellcheck disable=SC1090
  source ~/.profile || true
fi

echo "==> Configuring SITL board"
./waf configure --board sitl

echo "==> Building: $SITL_VEHICLES"
# shellcheck disable=SC2086
./waf $SITL_VEHICLES

echo
echo "SITL binaries built under: $SITL_SRC_DIR/build/sitl/bin"
echo "Set: export SITL_BIN_DIR=\"$SITL_SRC_DIR/build/sitl/bin\""
