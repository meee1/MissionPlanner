# MissionPlannerTests.Unit

Fast, **cross-platform** regression unit tests for Mission Planner's core logic.

## Why this project exists

The original `MissionPlannerTests` project references the full WinForms
`MissionPlanner.csproj` (net472) and several of its tests hit live network
endpoints, so it only builds on Windows and is slow/flaky. This project instead
references **only the `netstandard2.0` logic libraries**
(`MissionPlanner.Utilities`, `MissionPlanner.ArduPilot`, `MAVLink`), so it builds
and runs with the .NET 8 SDK on Linux, macOS, and Windows — and runs in CI on
every push (`.github/workflows/tests.yml`).

## Running

```bash
# from the repo root
tests/run-unit-tests.sh           # Linux / macOS
pwsh tests/run-unit-tests.ps1     # Windows / cross-platform
```

Or directly:

```bash
dotnet test MissionPlannerTests.Unit/MissionPlannerTests.Unit.csproj -f net8.0 --filter TestCategory=Unit
```

All tests are tagged `[TestCategory("Unit")]`. They are deterministic and
offline — no network, no GUI, no hardware.

## Layout

| Folder       | Under test                                                        |
|--------------|-------------------------------------------------------------------|
| `Geometry/`  | `PointLatLngAlt`, `Vector3`, `Quaternion`, `Matrix3`, UTM, `Spline2`, math helpers |
| `Mission/`   | `Locationwp` ↔ MAVLink items, `WaypointFile`, QGC `.plan` JSON, geofence |
| `Mavlink/`   | `MavlinkCRC` (wire CRC), `MavlinkParse` (v1/v2 + tlog framing)     |
| `Grid/`      | `Grid.CreateGrid` survey generation                               |
| `Logs/`      | DataFlash text (`DFLog`) and binary (`BinaryLog`) parsing          |
| `Params/`    | `CurrentState` display-unit conversions, `ParamChanges47` renames |
| `TestData/`  | committed fixtures copied to the test output directory            |

## Adding tests

1. Put the test in the folder matching the source area (create one if needed).
2. Tag every `[TestMethod]`'s class or method with `[TestCategory("Unit")]`.
3. Keep it deterministic and offline. Generate fixtures programmatically where
   cheap; otherwise add a small file under `TestData/`.
4. Run `tests/run-unit-tests.sh` and confirm green before pushing.

## Roadmap

This is the Phase 1 foundation. Planned follow-ups (see the project test plan):

- **Phase 2** — `MissionPlannerTests.Sitl`: headless ArduPilot SITL integration
  tests driving `MAVLinkInterface` (connect / param round-trip / mission
  upload-download / arm / guided takeoff), with SITL built from source on a
  Linux CI runner.
- Broaden Phase 1 coverage class by class (geofence, DataFlash/tlog parsing,
  quaternion/matrix math, parameter metadata).
