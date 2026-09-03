using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using MissionPlanner.Comms;
using MissionPlanner.Maui.GCSViews;
using Xamarin; // Test facade + service interfaces
using DeviceInfo = MissionPlanner.ArduPilot.DeviceInfo;

namespace MissionPlanner.Maui.iOS
{
    // Ported from the legacy Xamarin.iOS Main.cs service registrations.
    // iOS has no general USB serial; BT would use CoreBluetooth (left as a stub for now).
    internal static class PlatformServices
    {
        public static void Register()
        {
            Test.BlueToothDevice = new BTDevice();
            Test.UsbDevices = new USBDevices();
            Test.Radio = new Radio();
            Test.GPS = new GPS();
            Test.SystemInfo = new SystemInfo();
            WinFormsHostPage.IOS = true;
        }
    }

    public class Radio : IRadio
    {
        public void Toggle() { }
    }

    public class BTDevice : IBlueToothDevice
    {
        // TODO: implement via CoreBluetooth.
        public Task<List<DeviceInfo>> GetDeviceInfoList() => Task.FromResult(new List<DeviceInfo>());
        public Task<ICommsSerial> GetBT(DeviceInfo first) => throw new NotImplementedException();
    }

    public class USBDevices : IUSBDevices
    {
        public event EventHandler<DeviceInfo> USBEvent;
        public DeviceInfo GetDeviceInfo(object devicein) => throw new NotImplementedException();
        public Task<ICommsSerial> GetUSB(DeviceInfo di) => throw new NotImplementedException();
        public Task<List<DeviceInfo>> GetDeviceInfoList() => Task.FromResult(new List<DeviceInfo>());
        public void USBEventCallBack(object usbDeviceReceiver, object device) => throw new NotImplementedException();
    }

    public class GPS : IGPS
    {
        public Task<(double lat, double lng, double alt)> GetPosition() =>
            Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best))
                .ContinueWith<(double, double, double)>(t =>
                    (t.Result.Latitude, t.Result.Longitude, t.Result.Altitude ?? 0.0));
    }

    public class SystemInfo : ISystemInfo
    {
        public string GetSystemTag() => "";
        public void StartProcess(string[] cmd) { }
    }
}
