# MissionPlannerTests.Sitl

Headless **integration** tests that drive a real ArduPilot SITL instance through
Mission Planner's own `MAVLinkInterface` — the same connect/command path the GUI
uses (see `GCSViews/SITL.cs`), minus WinForms.

## How it works

`SitlFixture` launches a SITL binary (`-M<model> -O<home> -s<speedup>
--serial0 tcp:0`), connects a `TcpSerial` to `127.0.0.1:5760`, and opens a
headless `MAVLinkInterface` (`Open(getparams:false, skipconnectedcheck:true,
showui:false)` → the `NoUIReporter` path). Tests then exercise heartbeat,
parameter download / set-get round-trip, mission upload-download round-trip, and
home position.

## Running

```bash
# 1. Build SITL once (clones ArduPilot, builds copter+plane):
tests/build-sitl.sh
export SITL_BIN_DIR="$PWD/.sitl/ardupilot/build/sitl/bin"

# 2. Run the integration tests:
tests/run-sitl-tests.sh
```

Environment variables:

| Var            | Meaning                                            | Default |
|----------------|----------------------------------------------------|---------|
| `SITL_BIN_DIR` | directory containing `arducopter` / `arduplane`    | (unset) |
| `SITL_SPEEDUP` | SITL sim speed multiplier                          | `1`     |

**If `SITL_BIN_DIR` is unset or the binary is missing, every test reports as
inconclusive (skipped)** rather than failing — so this project builds and runs
everywhere, and only does real work where SITL is available. In CI it runs in the
gated `sitl-tests` job (`.github/workflows/tests.yml`).

## Adding scenarios

Add `[TestMethod] [TestCategory("Sitl")]` methods to `SitlConnectionTests`
(shared instance) or a new class with its own `[ClassInitialize]` fixture. Drive
the vehicle via the async `MAVLinkInterface` API (`doARMAsync`, `doCommandAsync`
for `NAV_TAKEOFF`, mode changes, etc.). Keep assertions tolerant of simulation
timing and prefer high `SITL_SPEEDUP` to keep runs fast.
