using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MissionPlanner;
using MissionPlanner.Comms;

namespace MissionPlanner.Tests.Sitl
{
    /// <summary>
    /// Launches an ArduPilot SITL binary headless and connects a
    /// <see cref="MAVLinkInterface"/> to it over TCP — the same transport and
    /// connect sequence Mission Planner's SITL screen uses (GCSViews/SITL.cs),
    /// minus the WinForms UI. Dispose stops the connection and the process.
    ///
    /// The SITL binaries are located via the <c>SITL_BIN_DIR</c> environment
    /// variable (see tests/build-sitl.sh). When that is not set the fixture is
    /// unavailable and the tests mark themselves inconclusive rather than fail,
    /// so the project still builds and runs everywhere.
    /// </summary>
    public sealed class SitlFixture : IDisposable
    {
        // ArduPilot autotest's canonical Canberra home: lat,lng,alt,heading.
        public const string DefaultHome = "-35.363261,149.165230,584,353";

        private const string Host = "127.0.0.1";
        private const int BasePort = 5760;

        private readonly Process _proc;
        private readonly string _workDir;

        public MAVLinkInterface Mav { get; }
        public byte Sysid => Mav.MAV.sysid;
        public byte Compid => Mav.MAV.compid;

        private SitlFixture(Process proc, string workDir, MAVLinkInterface mav)
        {
            _proc = proc;
            _workDir = workDir;
            Mav = mav;
        }

        /// <summary>Directory containing the SITL binaries, or null if unset.</summary>
        public static string BinDir => Environment.GetEnvironmentVariable("SITL_BIN_DIR");

        public static int Speedup =>
            int.TryParse(Environment.GetEnvironmentVariable("SITL_SPEEDUP"), out var s) && s > 0 ? s : 1;

        /// <summary>True when a usable SITL binary can be found for the given vehicle.</summary>
        public static bool IsAvailable => ResolveBinary("arducopter") != null;

        public static bool VehicleAvailable(string vehicle) => ResolveBinary(vehicle) != null;

        /// <summary>Start a multirotor SITL and connect to it.</summary>
        public static SitlFixture StartCopter(string home = DefaultHome) => Start("arducopter", "+", home);

        /// <summary>Start a fixed-wing SITL and connect to it.</summary>
        public static SitlFixture StartPlane(string home = DefaultHome) => Start("arduplane", "plane", home);

        /// <summary>
        /// Start the named vehicle binary with the given frame model, then open a
        /// headless MAVLink connection and wait for the first heartbeat.
        /// </summary>
        public static SitlFixture Start(string vehicle, string model, string home = DefaultHome)
        {
            string bin = ResolveBinary(vehicle)
                         ?? throw new InvalidOperationException(
                             $"SITL binary '{vehicle}' not found. Set SITL_BIN_DIR (see tests/build-sitl.sh).");

            // SITL needs a writable working directory for its eeprom.bin etc.
            string workDir = Path.Combine(Path.GetTempPath(), "mp_sitl_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workDir);

            // Mirror GCSViews/SITL.cs:664 argument layout.
            var psi = new ProcessStartInfo
            {
                FileName = bin,
                Arguments = $"-M{model} -O{home} -s{Speedup} --serial0 tcp:0 -w",
                WorkingDirectory = workDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            var proc = Process.Start(psi)
                       ?? throw new InvalidOperationException("Failed to start SITL process.");
            proc.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine("[SITL] " + e.Data); };
            proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine("[SITL!] " + e.Data); };
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            try
            {
                var mav = Connect(BasePort, TimeSpan.FromSeconds(60));
                return new SitlFixture(proc, workDir, mav);
            }
            catch
            {
                TryKill(proc);
                TryDeleteDir(workDir);
                throw;
            }
        }

        private static MAVLinkInterface Connect(int port, TimeSpan timeout)
        {
            var mav = new MAVLinkInterface();
            var deadline = DateTime.UtcNow + timeout;
            Exception last = null;

            // SITL takes a moment to open its TCP server; retry until it accepts.
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var client = new TcpSerial
                    {
                        Host = Host,          // setting Host skips the interactive host/port prompt
                        Port = port.ToString(),
                    };
                    mav.BaseStream = client;
                    // getparams:false, skipconnectedcheck:true, showui:false -> NoUIReporter path
                    mav.Open(false, true, false);

                    var hb = mav.getHeartBeatAsync().GetAwaiter().GetResult();
                    if (hb != null)
                        return mav;
                }
                catch (Exception ex)
                {
                    last = ex;
                    try { mav.Close(); } catch { /* ignore */ }
                    Thread.Sleep(500);
                }
            }

            throw new TimeoutException(
                $"Timed out connecting to SITL on {Host}:{port} within {timeout.TotalSeconds:0}s.", last);
        }

        /// <summary>Resolve a vehicle binary name to a full path under SITL_BIN_DIR.</summary>
        private static string ResolveBinary(string vehicle)
        {
            string dir = BinDir;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return null;

            foreach (var candidate in new[] { vehicle, vehicle + ".exe", vehicle + ".elf" })
            {
                string full = Path.Combine(dir, candidate);
                if (File.Exists(full))
                    return full;
            }
            return null;
        }

        public void Dispose()
        {
            try { Mav?.Close(); } catch { /* ignore */ }
            TryKill(_proc);
            TryDeleteDir(_workDir);
        }

        private static void TryKill(Process p)
        {
            try
            {
                if (p != null && !p.HasExited)
                {
                    p.Kill();
                    p.WaitForExit(5000);
                }
            }
            catch { /* ignore */ }
        }

        private static void TryDeleteDir(string dir)
        {
            try { if (dir != null && Directory.Exists(dir)) Directory.Delete(dir, true); }
            catch { /* ignore */ }
        }
    }
}
