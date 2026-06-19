# Phase 3 — platform services + dependency swaps

## Platform-service architecture

The cross-platform code talks to devices through the `Test` facade
(`ExtLibs/Xamarin/Xamarin/ITest.cs`, namespace `Xamarin`), which holds one
implementation per capability:

| Property | Interface | Purpose |
| --- | --- | --- |
| `Test.UsbDevices` | `IUSBDevices` | enumerate / open USB serial devices, hot-plug events |
| `Test.BlueToothDevice` | `IBlueToothDevice` | enumerate / open BT + BLE devices |
| `Test.Radio` | `IRadio` | toggle the cellular radio |
| `Test.GPS` | `IGPS` | device location |
| `Test.SystemInfo` | `ISystemInfo` | platform tag / launch a process |
| `Test.Speech` | `ISpeech` | text-to-speech (MAUI `TextToSpeech` is used by the ported render loop) |

Each platform head assigns these at startup. Under MAUI the registration happens
in the platform entry point (`Platforms/<plat>/MainActivity` or `AppDelegate`).

## Android — done

The legacy Android service implementations are **Xamarin.Forms/Essentials-free**, so
they are **linked** into the android target (single source of truth, not copied):

- `BTDevice.cs`, `USBDevices.cs`, `Radio.cs`, `AndroidSerial.cs`,
  `UsbDeviceReceiver.cs`, `DeviceDiscoveredReceiver.cs`,
  `DiscoverableModeReceiver.cs`, `BluetoothDiscoveryModeArgs.cs`, `SysProp.cs`,
  `AbstractUnixEndPoint.cs`.

Only the entry point was rewritten for MAUI in `Platforms/Android/MainActivity.cs`:
`FormsAppCompatActivity` → `MauiAppCompatActivity`, dropped
`Forms.Init` / `Essentials.Platform.Init` / `LoadApplication` (MAUI does these),
`Xamarin.Essentials.Geolocation` → `Microsoft.Maui.Devices.Sensors.Geolocation`,
`WinForms.*` → `WinFormsHostPage.*`, and `Test.* = new <impl>()` registration
(replacing the old `ServiceLocator`). `GPS` and `SystemInfo` (previously inline in
the legacy `MainActivity`) are ported here. Video (`AndroidVideo`,
`FormsVideoLibrary`) and gstreamer were dropped (video page removed).

### Android binding dependencies — migrated

`USBDevices` needs `UsbSerialForAndroid`, and the GDAL map overlays need
`GDALForAndroid`. Both were legacy MonoAndroid `v13.0` binding projects; they have now
been migrated to **`net10.0-android`** SDK-style binding projects
(`<IsBindingProject>true</IsBindingProject>`, `Xamarin.Android.Bindings.targets`
import removed, `<EmbeddedJar>`/`<LibraryProjectZip>` → `<AndroidLibrary Bind="true">`,
`<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to keep the existing
`AssemblyInfo.cs`). The `Transforms/*.xml` metadata is unchanged. `GDALForAndroid`
still references `System.Drawing.android` (a netstandard2.0 facade that type-forwards
to `MissionPlanner.Drawing`).

> Not compile-verified (no .NET SDK / Android workload here). On first build, the most
> likely fixups are binding generator (`class-parse`) differences in the generated API
> surface — adjust `Transforms/Metadata.xml` if names/visibility shifted.

## iOS / Mac Catalyst / Windows — done

Each platform now has a `Platforms/<plat>/PlatformServices.cs` that implements the `Test`
facade interfaces and is wired up from the platform entry point's `CreateMauiApp`
override (mirroring `Platforms/Android/MainActivity.OnCreate`):

- **iOS** (`Platforms/iOS/PlatformServices.cs`, registered in `AppDelegate`): ported from
  the legacy `Xamarin.iOS/Main.cs`. `Radio`/`BTDevice`/`USBDevices` are stubs (iOS has no
  general USB serial; BT would use CoreBluetooth — TODO), `GPS` uses MAUI Geolocation,
  `SystemInfo` is trivial. Sets `WinFormsHostPage.IOS`.
- **Mac Catalyst** (`Platforms/MacCatalyst/PlatformServices.cs`, registered in
  `AppDelegate`): ported from `Xamarin.MacOS/AppDelegate.cs`. `USBDevices` keeps the real
  `ioreg` + `/dev/tty.*` enumeration returning `MissionPlanner.Comms.SerialPort`; `GPS`
  uses MAUI Geolocation; `SystemInfo` ported. **Speech is not registered** — the legacy
  `OSXSpeech` used AppKit `NSSpeechSynthesizer` (unavailable on UIKit-based Catalyst), so
  the render loop falls back to the MAUI `TextToSpeech`-based `Speech` in `WinFormsHostPage`.
  Sets `WinFormsHostPage.OSX`.
- **Windows** (`Platforms/Windows/PlatformServices.cs`, registered in WinUI `App`): ported
  from `Xamarin.UWP/MainPage.xaml.cs`. `Radio`/`USBDevices`/`BlueTooth` are stubs; real
  serial/BT via `Windows.Devices.SerialCommunication` / `Windows.Devices.Bluetooth` on
  WinAppSDK is a TODO.

> Not compile-verified: CI only builds the `net10.0-android` target, so the iOS /
> maccatalyst / windows service files (which compile only under their own TFM) have not
> been exercised by a build yet.

## Dependency swaps

| Legacy package              | MAUI replacement (in `MissionPlanner.Maui.csproj`) |
| --------------------------- | -------------------------------------------------- |
| `SkiaSharp.Views.Forms`     | `SkiaSharp.Views.Maui.Controls`                    |
| `Xamarin.Essentials`        | built-in MAUI Essentials (`Microsoft.Maui.*`)      |
| `Acr.UserDialogs`           | `CommunityToolkit.Maui`                            |
| `Xamarin.Plugin.FilePicker` | built-in MAUI `FilePicker`                          |
| `Xam.Plugin.TabView`        | MAUI `TabbedPage` / Shell tabs                     |
| `Xamarin.Forms.DataGrid`    | **no direct MAUI port** — pick a MAUI DataGrid (CommunityToolkit / 3rd-party) where the param grid is shown |

The Xamarin.Forms / Xamarin.Forms.Platform.WinForms packages are gone — not needed
under MAUI (see PHASE2-NOTES.md).

## Pages

Per direction, the secondary pages (Video, Firmware, FlightData, master/detail menu)
are **not** ported — the app shows the WinForms host (`WinFormsHostPage`) directly via
`AppShell`. The legacy `MainPage`/menu and those pages can be deleted from the old
heads when they are retired.
