# CLAUDE.md — MissionPlanner

## Project Overview

MissionPlanner is a full-featured ground control station (GCS) for ArduPilot unmanned vehicles. It is a Windows Forms desktop application written in C# that supports mission planning, real-time flight data monitoring, vehicle configuration, firmware updates, and log analysis for drones and other autonomous vehicles.

- **Author**: Michael Oborne
- **License**: GPL v3 (`COPYING.txt`)
- **Website**: http://ardupilot.org/planner/
- **Forum**: http://discuss.ardupilot.org/c/ground-control-software/mission-planner

## Build System

### Prerequisites

- **Visual Studio 2022** (recommended) — use `vs2022.vsconfig` to install required components
- **MSBuild** (comes with Visual Studio)
- **NuGet** (included in repo at `.nuget/NuGet.exe`)
- Git submodules must be initialized: `git submodule update --init`

### Solution Files

| Solution | Purpose |
|----------|---------|
| `MissionPlanner.sln` | Primary solution — full desktop application (80+ projects) |
| `MissionPlannerLib.sln` | Library-only build (netstandard2.0 target) |
| `Updater/Updater.sln` | Self-update utility |

### Target Frameworks

| Project | Framework | Output |
|---------|-----------|--------|
| `MissionPlanner.csproj` | net472 | `bin/{Config}/net461/` |
| `MissionPlannerLib.csproj` | netstandard2.0 | Library DLL |
| `MissionPlannerTests.csproj` | net472 | Test assembly |

### Build Commands

```bash
# Windows — using build script
build.bat

# Windows — using MSBuild directly
msbuild -v:m -restore -t:Build -p:Configuration=Release MissionPlanner.sln

# Debug build
msbuild -v:m -restore -t:Build -p:Configuration=Debug MissionPlanner.sln
```

**Important**: VSCode with C# plugin can parse the code but cannot build. Use Visual Studio for building.

### CI/CD Pipelines

Located in `.github/workflows/`:

| Workflow | File | Runner | Purpose |
|----------|------|--------|---------|
| DotNet Build | `main.yml` | windows-latest | Release + Debug builds, beta releases |
| Android Build | `android.yml` | windows-latest | AAB/APK builds, Play Store deploy |
| OSX Build | `mac.yml` | macos-14 | iOS + macOS builds |

All workflows trigger on push, pull_request, and workflow_dispatch. The build uses MSBuild found via `vswhere.exe`.

## Repository Structure

```
MissionPlanner/
├── Program.cs                 # Entry point (MissionPlanner.Program.Main)
├── MainV2.cs                  # Main application form (~4800 lines)
├── Splash.cs                  # Splash screen
├── Common.cs                  # Shared constants and utilities
│
├── GCSViews/                  # Main application views (tabs)
│   ├── FlightData.cs          # Real-time flight telemetry (~6700 lines)
│   ├── FlightPlanner.cs       # Mission/waypoint planning (~8500 lines)
│   ├── InitialSetup.cs        # Vehicle initial setup wizard
│   ├── SoftwareConfig.cs      # Software configuration
│   ├── SITL.cs                # Software-in-the-loop simulation
│   ├── Help.cs                # Help view
│   └── ConfigurationView/     # Configuration sub-views (30+ files)
│       ├── ConfigFlightModes.cs
│       ├── ConfigFailSafe.cs
│       ├── ConfigBatteryMonitoring2.cs
│       └── ...
│
├── Controls/                  # Custom WinForms UI controls
│   ├── ConnectionControl.cs   # Serial/network connection UI
│   ├── EKFStatus.cs           # EKF status display
│   ├── DroneCANInspector.cs   # DroneCAN message inspector
│   └── ...
│
├── Utilities/                 # Application-level utilities
│   ├── Firmware.cs            # Firmware upload/management
│   ├── ThemeManager.cs        # UI theming system
│   ├── Update.cs              # Application self-update
│   ├── BoardDetect.cs         # Autopilot board detection
│   └── ...
│
├── Plugin/                    # Plugin system infrastructure
│   ├── Plugin.cs              # Abstract plugin base class
│   ├── PluginLoader.cs        # Plugin discovery and loading
│   └── PluginUI.cs            # Plugin UI integration
│
├── Plugins/                   # Example/bundled plugins
│   ├── example10-canlogfile.cs
│   ├── example14-mass.cs
│   └── ...
│
├── Log/                       # Log viewing and download
├── Grid/                      # Survey grid planning
├── Swarm/                     # Multi-vehicle swarm control
├── Joystick/                  # Joystick/gamepad input
├── SikRadio/                  # SiK radio configuration
├── Antenna/                   # Antenna tracker
├── NoFly/                     # No-fly zone management
├── GeoRef/                    # Photo geotagging
├── Warnings/                  # Alert/warning system
├── Updater/                   # Self-update component
│
├── ExtLibs/                   # External libraries (90+ subprojects)
│   ├── ArduPilot/             # ArduPilot protocol helpers
│   ├── Mavlink/               # MAVLink protocol (git submodule-sourced)
│   ├── DroneCAN/              # DroneCAN/UAVCAN protocol
│   ├── Comms/                 # Serial/TCP/UDP communication
│   ├── Controls/              # Shared UI controls (HUD, gauges, etc.)
│   ├── Utilities/             # Core utilities (Settings, Download, etc.)
│   ├── Maps/                  # Custom map providers
│   ├── GMap.NET.Core/         # GMap.NET mapping library
│   ├── GMap.NET.WindowsForms/ # GMap.NET WinForms integration
│   ├── Xamarin/               # Mobile platform projects
│   ├── Core/                  # Core interfaces and base classes
│   └── ...
│
├── MissionPlannerTests/       # Test project (MSTest)
│   ├── GCSViews/
│   ├── Utilities/
│   └── Linked/
│
├── Properties/                # Assembly info, resources, app manifest
├── Drivers/                   # Windows driver files (amd64, x86)
├── wix/                       # WiX installer definitions
├── Msi/                       # MSI installer resources
├── L10N.cs                    # Localization support
└── .github/workflows/         # CI/CD pipeline definitions
```

## Key Architecture Concepts

### Application Flow

1. `Program.Main()` → `Program.Start()` initializes logging (log4net), maps (GMap.NET), themes, and proxy settings
2. `Application.Run(new MainV2())` launches the main WinForms application
3. `MainV2` is the top-level form containing navigation tabs and the connection bar
4. Each tab (FlightData, FlightPlanner, InitialSetup, etc.) is a `GCSView` loaded via `MainSwitcher`

### Communication Layer

- **MAVLink** protocol (`ExtLibs/Mavlink/`) — primary drone communication protocol
- **MAVLinkInterface** — manages MAVLink connections over serial, TCP, UDP
- **DroneCAN** (`ExtLibs/DroneCAN/`) — CAN bus peripheral communication
- **Comms** (`ExtLibs/Comms/`) — abstractions for serial ports, TCP, UDP connections

### Mapping System

- Built on **GMap.NET** (`ExtLibs/GMap.NET.*/`)
- Custom map providers in `ExtLibs/Maps/` (WMS, WMTS, Mapbox, Japanese maps, etc.)
- Map cache stored at `C:\ProgramData\Mission Planner\gmapcache`

### Plugin System

- Plugins extend `MissionPlanner.Plugin.Plugin` abstract class
- Must implement: `Name`, `Version`, `Author`, `Init()`, `Loaded()`
- Optional overrides: `Loop()`, `SetupUI()`, event handlers for map/waypoint interactions
- Plugins are loaded from DLL or compiled from .cs files at runtime
- Place plugins in the `plugins/` directory (lowercase)

### Settings and Configuration

- `Settings` class (`ExtLibs/Utilities/`) manages application settings
- Settings stored as XML in user data directory
- `app.config` / `missionplanner.exe.config` for application-level configuration
- Data directory: `C:\ProgramData\Mission Planner` (Windows) or `~/.local/share/Mission Planner/` (Linux)

### Theming

- `ThemeManager` (`Utilities/ThemeManager.cs`) applies themes across all forms and controls
- Theme files: `BurntKermit.mpsystheme`, `HighContrast.mpsystheme`
- `ApplyThemeTo` callbacks registered in `Program.Start()`

### Logging

- Uses **log4net** configured via XML (`XmlConfigurator.Configure()`)
- Standard pattern: `private static readonly ILog log = LogManager.GetLogger(typeof(ClassName));`
- Trace log at `{DataDirectory}/trace.log`

## Coding Conventions

### Naming

- **Namespaces**: `MissionPlanner`, `MissionPlanner.GCSViews`, `MissionPlanner.Utilities`, `MissionPlanner.Controls`, `MissionPlanner.ArduPilot`, etc.
- **Classes**: PascalCase (e.g., `FlightData`, `MAVLinkInterface`, `ThemeManager`)
- **Methods**: PascalCase (e.g., `DoUpdate()`, `GetDataDirectory()`)
- **Fields**: Often camelCase or lowercase for public fields (legacy codebase — `public static menuicons displayicons`)
- **Local variables**: camelCase
- **Constants/Statics**: Mixed conventions — follow existing patterns in the file you're editing

### Code Style

- WinForms designer files (`*.Designer.cs`) are auto-generated — never edit manually
- Resource files (`*.resx`) accompany forms for localization
- Localization supported for 10+ languages (ar, az, id, ja, ko, pt, ru, tr, uk, zh-Hans, zh-Hant, zh-TW)
- Conditional compilation with `#if !LIB` for desktop-only code paths
- Empty catch blocks are common in the codebase (legacy pattern)

### Analyzer Rules (from .editorconfig)

- Async void methods must catch exceptions (`AsyncVoidAnalyzer` — severity: error)
- Fire-and-forget async void disallowed (`AsyncFixer03` — severity: error)
- Public field visibility allowed (`CA1051` — severity: silent)
- General exception catching permitted (`CA1031` — severity: suggestion)
- Underscore identifiers permitted (`CA1707` — severity: suggestion)

### File Organization

- Each WinForms form has three files: `Name.cs` (logic), `Name.Designer.cs` (auto-generated layout), `Name.resx` (resources)
- ExtLibs projects are self-contained with their own `.csproj` files
- Conditional compilation constants: `DEBUG`, `TRACE`, `LIB` (for library mode)

## Testing

### Framework

- **MSTest** (Microsoft.TestPlatform v17.3.2, MSTest.TestFramework v2.2.10)
- Test project: `MissionPlannerTests/MissionPlannerTests.csproj` (net472)

### Test Files

```
MissionPlannerTests/
├── GCSViews/FlightPlannerTests.cs
├── Utilities/
│   ├── GitHubContentTests.cs
│   ├── FirmwareTests.cs
│   └── BoardDetectTests.cs
├── Linked/DownloadTests.cs
└── httpclient.cs
```

### Running Tests

```bash
# Via MSBuild/Visual Studio Test Explorer
# Or via dotnet test (requires .NET SDK)
dotnet test MissionPlannerTests/MissionPlannerTests.csproj
```

## Platform Support

| Platform | Status | Build Target |
|----------|--------|-------------|
| Windows | Primary, fully supported | net472 (MSBuild) |
| Linux | Via Mono (`mono MissionPlanner.exe`) | Same binary |
| Android | Play Store release | Xamarin.Android (API 33) |
| macOS | Experimental | Xamarin.Mac |
| iOS | Experimental | Xamarin.iOS |

## Key Dependencies

| Library | Purpose |
|---------|---------|
| GMap.NET | Map display and caching |
| MAVLink | Drone communication protocol |
| DroneCAN | CAN bus communication |
| log4net | Logging framework |
| Newtonsoft.Json | JSON serialization |
| SkiaSharp | 2D rendering |
| OpenTK | OpenGL 3D graphics |
| SharpDX | DirectX input (joysticks) |
| IronPython | Python scripting support |
| Accord.NET | Computer vision (image processing) |
| SSH.NET | SSH connections (log download) |
| ZedGraph | Charting and graphing |
| Flurl | HTTP client helpers |
| BouncyCastle | Cryptography |

## Git Workflow

- **Submodules**: `ExtLibs/mono` (shallow clone from `https://github.com/meee1/mono.git`)
- Initialize submodules after cloning: `git submodule update --init`
- CI builds check out with `submodules: true`
- `.gitignore` excludes: `bin/`, `obj/`, `obj2/`, `obj3/`, `.vs/`, `packages/`, `*.zip`, `*.appx`

## Important Notes for AI Assistants

1. **Never edit `*.Designer.cs` files** — these are auto-generated by the WinForms designer
2. **Never edit `*.resx` files** directly for code changes — these are resource/localization files
3. **The build requires Windows** (MSBuild + .NET Framework 4.7.2) — Linux/macOS cannot build the solution
4. **ExtLibs contains 90+ subprojects** — changes to these libraries affect many consumers
5. **Large files are common** — FlightPlanner.cs (~8500 lines), FlightData.cs (~6700 lines), MainV2.cs (~4800 lines)
6. **Legacy codebase patterns** — empty catch blocks, public fields, mixed naming conventions are intentional and widespread; do not "fix" these unless specifically asked
7. **Conditional compilation** — code paths differ between `LIB` (library/mobile) and desktop builds
8. **Plugin system** — new functionality can often be added as a plugin rather than modifying core code
9. **The `Settings` class** is the central configuration store — prefer it over direct file I/O for user preferences
10. **MAVLink message types** are auto-generated — the protocol definitions come from the MAVLink project upstream
