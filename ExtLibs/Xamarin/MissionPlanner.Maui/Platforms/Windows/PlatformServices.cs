using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MissionPlanner.Comms;
using Xamarin; // Test facade + service interfaces
using DeviceInfo = MissionPlanner.ArduPilot.DeviceInfo;

namespace MissionPlanner.Maui.WinUI
{
    // Ported from the legacy Xamarin.UWP MainPage.xaml.cs service registrations.
    // The desktop net472 head already provides real serial/USB on Windows; the WinAppSDK head can later
    // implement these via Windows.Devices.SerialCommunication / Windows.Devices.Bluetooth (TODO).
    internal static class PlatformServices
    {
        public static void Register()
        {
            Test.Radio = new Radio();
            Test.UsbDevices = new USBDevices();
            Test.BlueToothDevice = new BlueTooth();
        }
    }

    public class Radio : IRadio
    {
        public void Toggle() { }
    }

    public class USBDevices : IUSBDevices
    {
        public event EventHandler<DeviceInfo> USBEvent;
        public DeviceInfo GetDeviceInfo(object devicein) => throw new NotImplementedException();
        public Task<ICommsSerial> GetUSB(DeviceInfo di) => throw new NotImplementedException();
        public Task<List<DeviceInfo>> GetDeviceInfoList() => Task.FromResult(new List<DeviceInfo>());
        public void USBEventCallBack(object usbDeviceReceiver, object device) => throw new NotImplementedException();
    }

    public class BlueTooth : IBlueToothDevice
    {
        public Task<List<DeviceInfo>> GetDeviceInfoList() => Task.FromResult(new List<DeviceInfo>());
        public Task<ICommsSerial> GetBT(DeviceInfo first) => Task.FromResult<ICommsSerial>(null);
    }
}
