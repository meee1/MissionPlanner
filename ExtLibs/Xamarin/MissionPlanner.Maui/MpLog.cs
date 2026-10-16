using System;

namespace MissionPlanner.Maui
{
    /// <summary>
    /// Minimal logging shim replacing the legacy Acr.UserDialogs.Infrastructure.Log
    /// (category, message) static API used by the ported render loop. Aliased as <c>Log</c>
    /// where needed (the unqualified name otherwise resolves to the MissionPlanner.Log namespace).
    /// </summary>
    internal static class MpLog
    {
        public static void Info(string category, string message) => Console.WriteLine($"{category}: {message}");
        public static void Warn(string category, string message) => Console.WriteLine($"WARN {category}: {message}");
        public static void Error(string category, string message) => Console.WriteLine($"ERROR {category}: {message}");
        public static void Verbose(string category, string message) => Console.WriteLine($"{category}: {message}");
    }
}
