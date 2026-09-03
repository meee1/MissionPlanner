// UnitySerial.cs
// Minimal serial / connection implementation for the Unity platform.
//
// On Android, Xamarin.Android provides USB and Bluetooth serial adapters.
// In Unity there is no built-in serial port API, so we expose:
//   • A TCP loopback bridge for SITL / MAVPROXY connections.
//   • Stubs for physical-port paths that the Interfaces layer expects.
//
// Extend this class to add BLE, USB-serial-over-Android-plugin, etc.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MissionPlanner.Comms;

#if UNITY_ENGINE_PRESENT
using UnityEngine;
#endif

namespace MissionPlanner.Unity.Comms
{
    /// <summary>
    /// TCP-based serial shim: connects to a MAVProxy or SITL TCP port.
    /// Drop-in for <c>MissionPlanner.Comms.TcpSerial</c> when running in Unity.
    /// </summary>
    public sealed class UnityTcpSerial : ICommsSerial
    {
        private TcpClient?     _client;
        private NetworkStream? _stream;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public string   PortName        { get; set; } = "tcp";
        public int      BaudRate        { get; set; } = 115200;
        public int      ReadTimeout     { get; set; } = 500;
        public int      WriteTimeout    { get; set; } = 500;
        public int      BytesToRead     => _stream?.DataAvailable == true ? 1 : 0;
        public int      BytesToWrite    => 0;
        public bool     IsOpen          => _client?.Connected ?? false;
        public int      DataBits        { get; set; } = 8;
        public bool     DtrEnable       { get; set; }
        public bool     RtsEnable       { get; set; }
        public int      ReadBufferSize  { get; set; } = 4096;
        public int      WriteBufferSize { get; set; } = 4096;
        public Stream   BaseStream      => _stream ?? Stream.Null;
        public int      ReceivedBytesThreshold { get; set; } = 1;

        public string Host { get; set; } = "127.0.0.1";
        public int    Port { get; set; } = 5760;

        public void Open()
        {
            _client = new TcpClient();
            _client.Connect(Host, Port);
            _client.NoDelay   = true;
            _stream           = _client.GetStream();
            _stream.ReadTimeout  = ReadTimeout;
            _stream.WriteTimeout = WriteTimeout;

#if UNITY_ENGINE_PRESENT
            Debug.Log($"[MissionPlanner] TCP serial connected to {Host}:{Port}");
#endif
        }

        public void Close()
        {
            _cts.Cancel();
            _stream?.Dispose();
            _client?.Dispose();
            _stream = null;
            _client = null;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            return _stream.Read(buffer, offset, count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            _stream.Write(buffer, offset, count);
        }

        public int ReadByte()
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            return _stream.ReadByte();
        }

        public void WriteByte(byte b)
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            _stream.WriteByte(b);
        }

        public int ReadChar()
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            return _stream.ReadByte();
        }

        public string ReadExisting()
        {
            if (_stream == null || !_stream.DataAvailable) return string.Empty;
            var buf = new byte[4096];
            int n = _stream.Read(buf, 0, buf.Length);
            return Encoding.ASCII.GetString(buf, 0, n);
        }

        public string ReadLine()
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            var sb = new StringBuilder();
            int b;
            while ((b = _stream.ReadByte()) != -1)
            {
                if (b == '\n') break;
                if (b != '\r') sb.Append((char)b);
            }
            return sb.ToString();
        }

        public void Write(string text)
        {
            if (_stream == null) throw new InvalidOperationException("Not open");
            var bytes = Encoding.ASCII.GetBytes(text);
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteLine(string text)
        {
            Write(text + "\n");
        }

        public void DiscardInBuffer()
        {
            // No-op for TCP – reading would consume.
        }

        public void toggleDTR() { /* not applicable for TCP */ }

        public event EventHandler? DataReceived;

        public void Dispose()
        {
            Close();
            _cts.Dispose();
        }
    }
}
