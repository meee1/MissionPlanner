using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using MissionPlanner.Comms;
using MissionPlanner.Maui.GCSViews;
using Xamarin; // Test facade + service interfaces
using DeviceInfo = MissionPlanner.ArduPilot.DeviceInfo;

namespace MissionPlanner.Maui.MacCatalyst
{
    // Ported from the legacy Xamarin.MacOS AppDelegate service implementations.
    // Speech is intentionally NOT registered: the legacy OSXSpeech used AppKit NSSpeechSynthesizer
    // (not available under Mac Catalyst), so the render loop falls back to the MAUI TextToSpeech-based
    // Speech in WinFormsHostPage.
    internal static class PlatformServices
    {
        public static void Register()
        {
            Test.BlueToothDevice = new BTDevice();
            Test.UsbDevices = new USBDevices();
            Test.Radio = new Radio();
            Test.GPS = new GPS();
            Test.SystemInfo = new SystemInfo();
            WinFormsHostPage.OSX = true;
        }
    }

    public class Radio : IRadio
    {
        public void Toggle() { }
    }

    public class BTDevice : IBlueToothDevice
    {
        public Task<List<DeviceInfo>> GetDeviceInfoList() => Task.FromResult(new List<DeviceInfo>());
        public Task<ICommsSerial> GetBT(DeviceInfo first) => throw new NotImplementedException();
    }

    public class USBDevices : IUSBDevices
    {
        public event EventHandler<DeviceInfo> USBEvent;

        public DeviceInfo GetDeviceInfo(object devicein) => throw new NotImplementedException();

        public void USBEventCallBack(object usbDeviceReceiver, object device) => throw new NotImplementedException();

        public async Task<ICommsSerial> GetUSB(DeviceInfo di)
        {
            var data = GetUSBList();
            var devs = GetDevList().Split(new[] { '\r', '\n' });
            var regex = new Regex(@"-o\s+([\w\s]+@.{4}).*\n.*idProduct""\s+=\s+(.*)\n.*\n.*idVendor""\s+=\s+(.*)");

            foreach (Match match in regex.Matches(data))
            {
                var name = match.Groups[1].Value;
                var loc = name.Split("@")[^1];
                foreach (var dev in devs)
                {
                    if (di.board == name && dev.ToLower().Contains(loc.ToLower()))
                        return new SerialPort(dev.TrimEnd());
                }
            }

            return null;
        }

        public async Task<List<DeviceInfo>> GetDeviceInfoList()
        {
            var ans = new List<DeviceInfo>();
            var data = GetUSBList();
            var devs = GetDevList().Split(new[] { '\r', '\n' });
            var regex = new Regex(@"-o\s+([\w\s]+@.{4}).*\n.*idProduct""\s+=\s+(.*)\n.*\n.*idVendor""\s+=\s+(.*)");

            foreach (Match match in regex.Matches(data))
            {
                var name = match.Groups[1].Value;
                var loc = name.Split("@")[^1];
                var pid = int.Parse(match.Groups[2].Value);
                var vid = int.Parse(match.Groups[3].Value);

                var deviceInfo = new DeviceInfo()
                {
                    board = name,
                    description = name,
                    hardwareid = String.Format("USB\\VID_{0:X4}&PID_{1:X4}&", vid, pid),
                    name = name
                };

                foreach (var dev in devs)
                {
                    if (deviceInfo.board == name && dev.ToLower().Contains(loc.ToLower()))
                        ans.Add(deviceInfo);
                }
            }

            return ans;
        }

        private static string GetDevList()
        {
            var proc = System.Diagnostics.Process.Start("bash", @"-c ""ls /dev/tty.* > /tmp/dev.list""");
            proc.WaitForExit();
            return File.ReadAllText("/tmp/dev.list");
        }

        private static string GetUSBList()
        {
            var proc = System.Diagnostics.Process.Start("bash",
                @"-c ""ioreg -p IOUSB -w0 -l | grep -E '@|idVendor|idProduct|bcdDevice' > /tmp/usb.list""");
            proc.WaitForExit();
            return File.ReadAllText("/tmp/usb.list");
        }
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

        public void StartProcess(string[] cmd)
        {
            System.Diagnostics.Process.Start(cmd[0], string.Join(" ", cmd, 1, cmd.Length - 1));
        }
    }
}
