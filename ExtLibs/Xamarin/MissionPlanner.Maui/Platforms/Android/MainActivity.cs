using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.Devices.Sensors;
using MissionPlanner;
using MissionPlanner.Comms;       // DeviceDiscoveredReceiver
using MissionPlanner.Maui.GCSViews;
using MissionPlanner.Utilities;
using System.Net.Sockets;
using Xamarin;          // Test facade
using Xamarin.Droid;    // BTDevice / USBDevices / Radio / receivers (linked native services)
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
using Settings = MissionPlanner.Utilities.Settings;
using Thread = System.Threading.Thread;

// Permissions / features previously declared in the legacy MainActivity.cs.
// (Assembly attributes must precede the file-scoped namespace declaration — CS1730.)
[assembly: UsesFeature("android.hardware.usb.host", Required = false)]
[assembly: UsesFeature("android.hardware.bluetooth", Required = false)]
[assembly: UsesFeature("android.hardware.bluetooth_le", Required = false)]
[assembly: UsesPermission("android.permission.BLUETOOTH")]
[assembly: UsesPermission("android.permission.BLUETOOTH_CONNECT")]
[assembly: UsesPermission("android.permission.BLUETOOTH_ADMIN")]
[assembly: UsesPermission("android.permission.BLUETOOTH_SCAN")]
[assembly: UsesPermission("android.permission.ACCESS_FINE_LOCATION")]
[assembly: UsesPermission("android.permission.ACCESS_COARSE_LOCATION")]
[assembly: UsesPermission("android.permission.INTERNET")]
[assembly: UsesPermission("android.permission.WAKE_LOCK")]
[assembly: UsesPermission("android.permission.ACCESS_NETWORK_STATE")]

namespace MissionPlanner.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, Exported = true,
    ScreenOrientation = ScreenOrientation.SensorLandscape, HardwareAccelerated = true,
    LaunchMode = LaunchMode.SingleInstance,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { Intent.ActionMain, UsbManager.ActionUsbDeviceAttached, UsbManager.ActionUsbDeviceDetached },
    Categories = new[] { Intent.CategoryLauncher })]
[MetaData("android.hardware.usb.action.USB_DEVICE_ATTACHED", Resource = "@xml/device_filter")]
public class MainActivity : MauiAppCompatActivity
{
    readonly string TAG = "MP";
    public static MainActivity Current { private set; get; }

    public UsbDeviceReceiver UsbBroadcastReceiver;
    private DeviceDiscoveredReceiver BTBroadcastReceiver;
    private Socket server;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        Current = this;

        Window?.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn |
                         WindowManagerFlags.HardwareAccelerated);

        // MAUI bootstraps the app (MauiProgram.CreateMauiApp) and Essentials internally;
        // no Xamarin.Forms.Forms.Init / Xamarin.Essentials.Platform.Init / LoadApplication needed.
        base.OnCreate(savedInstanceState);

        Settings.CustomUserDataDirectory = Application.Context.GetExternalFilesDir(null).ToString();
        Log.Info(TAG, "Settings.CustomUserDataDirectory " + Settings.CustomUserDataDirectory);

        try
        {
            WinFormsHostPage.Android = true;
            WinFormsHostPage.BundledPath = Application.Context.ApplicationInfo.NativeLibraryDir;
        }
        catch { }
        Log.Info(TAG, "WinFormsHostPage.BundledPath " + WinFormsHostPage.BundledPath);

        // Register the platform service implementations onto the Test facade.
        Test.BlueToothDevice = new BTDevice();
        Test.UsbDevices = new USBDevices();
        Test.Radio = new Radio();
        Test.GPS = new GPS();
        Test.SystemInfo = new SystemInfo();

        Vario.Beep = (i, i1) => { playSound(i, i1); };

        // Optional GDAL map overlays — guarded; requires GDALForAndroid (see PHASE3-NOTES.md).
        try
        {
            Java.Lang.JavaSystem.LoadLibrary("gdal");
            Java.Lang.JavaSystem.LoadLibrary("gdalalljni");
            Java.Lang.JavaSystem.LoadLibrary("gdalwrap");

            System.Threading.Tasks.Task.Run(() =>
            {
                var gdaldir = Settings.GetRunningDirectory() + "gdalimages";
                System.IO.Directory.CreateDirectory(gdaldir);
                MissionPlanner.Utilities.GDAL.GDALBase = new GDAL.GDAL();
                GDAL.GDAL.ScanDirectory(gdaldir);
                GMap.NET.MapProviders.GMapProviders.List.Add(GDAL.GDALProvider.Instance);
            });
        }
        catch (System.Exception ex) { Log.Error("GDAL", ex.ToString()); }

        // Permissions
        if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) != (int)Permission.Granted ||
            ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Bluetooth) != (int)Permission.Granted ||
            ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothConnect) != (int)Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new[]
            {
                Android.Manifest.Permission.AccessFineLocation,
                Android.Manifest.Permission.Bluetooth,
                Android.Manifest.Permission.BluetoothConnect,
                Android.Manifest.Permission.BluetoothScan,
            }, 1);
        }

        // Handle a USB-attach launch intent.
        proxyIfUsbAttached(this.Intent);
    }

    public static void ShowKeyboard(Android.Views.View pView)
    {
        pView.RequestFocus();
        var imm = Current.GetSystemService(Context.InputMethodService) as InputMethodManager;
        imm.ShowSoftInput(pView, ShowFlags.Forced);
        imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
    }

    public static void HideKeyboard(Android.Views.View pView)
    {
        var imm = Current.GetSystemService(Context.InputMethodService) as InputMethodManager;
        imm.HideSoftInputFromWindow(pView.WindowToken, HideSoftInputFlags.None);
    }

    private void proxyIfUsbAttached(Intent intent)
    {
        if (intent == null) return;
        if (!UsbManager.ActionUsbDeviceAttached.Equals(intent.Action)) return;

        Log.Verbose(TAG, "usb device attached");
        WinFormsHostPage.InitDevice = () =>
        {
            Log.Info(TAG, "WinFormsHostPage.InitDevice");
            UsbBroadcastReceiver.OnReceive(this.ApplicationContext, intent);
        };
    }

    protected override void OnResume()
    {
        base.OnResume();

        Window.DecorView.SystemUiVisibility =
            (StatusBarVisibility)(SystemUiFlags.LowProfile | SystemUiFlags.Fullscreen |
                                  SystemUiFlags.HideNavigation | SystemUiFlags.Immersive |
                                  SystemUiFlags.ImmersiveSticky);

        StartD2DInfo();

        UsbBroadcastReceiver = new UsbDeviceReceiver();
        RegisterReceiver(UsbBroadcastReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
        RegisterReceiver(UsbBroadcastReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));

        BTBroadcastReceiver = new DeviceDiscoveredReceiver();
        RegisterReceiver(BTBroadcastReceiver, new IntentFilter(Android.Bluetooth.BluetoothDevice.ActionFound));
        RegisterReceiver(BTBroadcastReceiver, new IntentFilter(Android.Bluetooth.BluetoothAdapter.ActionDiscoveryFinished));
    }

    protected override void OnPause()
    {
        base.OnPause();
        StopD2DInfo();
        if (UsbBroadcastReceiver != null) UnregisterReceiver(UsbBroadcastReceiver);
        if (BTBroadcastReceiver != null) UnregisterReceiver(BTBroadcastReceiver);
    }

    public void StopD2DInfo()
    {
        server?.Close();
        server = null;
    }

    public void StartD2DInfo()
    {
        try
        {
            var d2dinfo = "linkstate";
            server = new Socket(AddressFamily.Unix, SocketType.Stream, 0);
            server.Bind(new AbstractUnixEndPoint(d2dinfo));
            server.Listen(50);

            System.Threading.Tasks.Task.Run(() =>
            {
                while (server != null)
                {
                    try
                    {
                        var socket = server.Accept();
                        Thread.Sleep(1);
                        byte[] buffer = new byte[100];
                        int readlen;
                        do
                        {
                            readlen = socket.Receive(buffer);
                        } while (readlen > 0);
                        socket.Close();
                    }
                    catch (System.Exception ex) { Log.Warn(TAG, ex.ToString()); Thread.Sleep(1000); }
                }
            });
        }
        catch (System.Exception ex) { Log.Warn(TAG, ex.ToString()); }
    }

    private byte[] genTone(int sampleRate, int freqOfTone, int numSamples)
    {
        byte[] generatedSnd = new byte[2 * numSamples];
        double[] sample = new double[numSamples];
        for (int i = 0; i < numSamples; ++i)
            sample[i] = System.Math.Sin(2 * System.Math.PI * i / (sampleRate / freqOfTone));

        int idx = 0;
        foreach (double dVal in sample)
        {
            short val = (short)(dVal * 32767);
            generatedSnd[idx++] = (byte)(val & 0x00ff);
            generatedSnd[idx++] = (byte)((val & 0xff00) >> 8);
        }
        return generatedSnd;
    }

    private void playSound(int freq, int duration)
    {
        var sampleRate = 8000;
        var generatedSnd = genTone(sampleRate, freq, (duration * sampleRate) / 1000);
        var audioTrack = new Android.Media.AudioTrack(Android.Media.Stream.Music,
            sampleRate, Android.Media.ChannelConfiguration.Mono, Android.Media.Encoding.Pcm16bit,
            generatedSnd.Length, Android.Media.AudioTrackMode.Stream);
        audioTrack.Play();
        audioTrack.Write(generatedSnd, 0, generatedSnd.Length);
        Thread.Sleep(duration + 40);
        audioTrack.Stop();
    }
}

// GPS / SystemInfo were inline in the legacy MainActivity.cs; ported to MAUI Essentials here.
public class GPS : IGPS
{
    public System.Threading.Tasks.Task<(double lat, double lng, double alt)> GetPosition()
    {
        return Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best))
            .ContinueWith<(double, double, double)>(location =>
                (location.Result.Latitude, location.Result.Longitude,
                 location.Result.Altitude ?? 0.0));
    }
}

public class SystemInfo : ISystemInfo
{
    public string GetSystemTag()
    {
        try { return SysProp.GetProp("ro.build.fingerprint"); }
        catch { return ""; }
    }

    public void StartProcess(string[] cmd)
    {
        Java.Lang.Runtime.GetRuntime().Exec(cmd);
    }
}
