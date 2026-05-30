# MissionPlannerTests.Sitl

Headless **integration** tests that drive a real ArduPilot SITL instance through
Mission Planner's own `MAVLinkInterface` — the same connect/command path the GUI
uses (see `GCSViews/SITL.cs`), minus WinForms.

## How it works

`SitlFixture` launches a SITL binary (`-M<model> -O<home> -s<speedup>
--instance <n> --serial0 tcp:0`), connects a `TcpSerial` to the instance's port
(`5760 + 10*n`), opens a headless `MAVLinkInterface` (`Open(getparams:false,
skipconnectedcheck:true, showui:false)` → the `NoUIReporter` path), and requests
telemetry streams. The copter tests (`SitlTests`) exercise heartbeat, parameter download /
set-get round-trip, mission upload-download round-trip, home position, GPS
3D-fix acquisition, a full GUIDED arm + takeoff that confirms the vehicle climbs
to the commanded altitude via live `GLOBAL_POSITION_INT` telemetry, an AUTO
mission run that confirms the active waypoint (`MISSION_CURRENT`) advances, and a
LAND that confirms the vehicle descends and disarms.

Fixed-wing tests live in a **separate project** (`MissionPlannerTests.SitlPlane`)
so they run in their own process. A single `MAVLinkInterface` per process is the
supported usage; two live interfaces in one process is not. The plane project
shares this `SitlFixture` via a linked source file.

One SITL instance and one `MAVLinkInterface` are shared across the whole test
class (an external sim is expensive to start, and a single interface per process
is the supported usage). The tests are independent — the read-only ones do not
care whether the vehicle is armed or flying.

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

Add `[TestMethod] [TestCategory("Sitl")]` methods to `SitlTests`, which shares
the single SITL connection. Drive the vehicle via the async `MAVLinkInterface`
API (`doARMAsync`, `doCommandAsync` for `NAV_TAKEOFF`, `setMode`, etc.). Note
that headless there is no GUI timer calling `UpdateCurrentSettings`, so read live
state by pumping `readPacketAsync()` and decoding messages (as the takeoff test
does) rather than relying on `MAV.cs`. Set flight modes by numeric `custom_mode`
(name translation needs `cs.firmware`, which the GUI populates). Keep assertions
tolerant of simulation timing and prefer a high `SITL_SPEEDUP` to keep runs fast.
