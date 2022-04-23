extern alias MPDrawing;
extern alias MPCommon;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.IndexedDB.Framework;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Sotsera.Blazor.Toaster.Core.Models;
using Tewr.Blazor.FileReader;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using Blazor.IndexedDB.Framework;
using BlazorWorker.Core;
using GMap.NET.MapProviders;
using MissionPlanner.ArduPilot;
using MissionPlanner.Utilities;
using Sotsera.Blazor.Toaster.Core.Models;
using Tewr.Blazor.FileReader;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Blazor.Extensions.Storage;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using MissionPlanner.Comms;
using MPDrawing::System.Drawing;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace wasm
{
    public class Program
    {
        private static log4net.ILog log;

        private static void AddTypeConverter(Type type, Type type1)
        {
            Attribute[] newAttributes = new Attribute[1];
            newAttributes[0] = new TypeConverterAttribute(type1);

            TypeDescriptor.AddAttributes(type, newAttributes);
        }
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient
                {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

            builder.Services.AddFileReaderService(options => options.UseWasmSharedBuffer = true);

            builder.Services.AddStorage();

            builder.Services.AddSpeechSynthesis();

            builder.Services.AddScoped<IIndexedDbFactory, IndexedDbFactory>();

            builder.Services.AddWorkerFactory();

            builder.Services.AddToaster(config =>
            {
                //example customizations
                config.PositionClass = Defaults.Classes.Position.TopRight;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
            });

            {

                //bool result1 = WebRequest.RegisterPrefix("http://", new Startup.FakeRequestFactory());
                //Console.WriteLine("webrequestmod " + result1);
                //bool result2 = WebRequest.RegisterPrefix("https://", new Startup.FakeRequestFactory());
                //Console.WriteLine("webrequestmod " + result2);



                log4net.Repository.Hierarchy.Hierarchy hierarchy =
                    (Hierarchy) log4net.LogManager.GetRepository(Assembly.GetAssembly(typeof(Startup)));

                PatternLayout patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
                patternLayout.ActivateOptions();

                var cca = new ConsoleAppender();
                cca.Layout = patternLayout;
                cca.ActivateOptions();
                hierarchy.Root.AddAppender(cca);

                hierarchy.Root.Level = Level.Debug;
                hierarchy.Configured = true;

                log =
                    log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


                log.Info("test");

                log.Info("Configure Done");

                new MPCommon::MissionPlanner.Drawing.Common.Common();
                

                AddTypeConverter(typeof(MPDrawing::System.Drawing.Bitmap), typeof(BitmapClassConverter));
                AddTypeConverter(typeof(MPDrawing::System.Drawing.Icon), typeof(IconClassConverter));
                AddTypeConverter(typeof(Font), typeof(FontClassConverter));
                //AddTypeConverter(typeof(Point), typeof(PointClassConverter));
                //AddTypeConverter(typeof(Size), typeof(SizeClassConverter));

                //new MissionPlanner.Drawing.Common.Common();


                //System.Net.WebRequest.get_InternalDefaultWebProxy

                //WebRequest.DefaultWebProxy = GlobalProxySelection.GetEmptyWebProxy();

                Directory.CreateDirectory(@"/home/web_user/Desktop");

                BinaryLog.onFlightMode += (firmware, modeno) =>
                {
                    try
                    {
                        if (firmware == "")
                            return null;

                        var modes = Common.getModesList((Firmwares) Enum.Parse(typeof(Firmwares), firmware));
                        string currentmode = null;

                        foreach (var mode in modes)
                        {
                            if (mode.Key == modeno)
                            {
                                currentmode = mode.Value;
                                break;
                            }
                        }

                        return currentmode;
                    }
                    catch
                    {
                        return null;
                    }
                };

                CustomMessageBox.ShowEvent += (text, caption, buttons, icon, yestext, notext) =>
                {
                    Console.WriteLine("CustomMessageBox " + caption + " " + text);


                    return CustomMessageBox.DialogResult.OK;
                };

                MissionPlanner.Utilities.Download.RequestModification += Download_RequestModification;
            }

            await builder.Build().RunAsync();
        }

        private static void Download_RequestModification(object sender, HttpRequestMessage e)
        {
            Console.WriteLine("Download_RequestModification Set No-Cors");
            e.SetBrowserRequestMode(BrowserRequestMode.NoCors);
        }


    }

    [TypeConverter(typeof(FontClassConverter))]
    public class FontClassConverter : TypeConverter
    { 
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            var info = value.ToString().Split(',');
            return new Font(info[0], float.Parse(info[1].Replace("pt", "")));

            return base.ConvertFrom(context, culture, value);
        }
    }

    [TypeConverter(typeof(ControlClassConverter))]
    public class ControlClassConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            var log =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("ControlClassConverter.ConvertFrom " + value + " " + context);

            if (string.Equals(value, "$this"))
            {
                return null;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

    [TypeConverter(typeof(BitmapClassConverter))]
    public class BitmapClassConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(MPDrawing::System.Drawing.Bitmap))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is byte[])
            {
                return new MPDrawing::System.Drawing.Bitmap(new MemoryStream((byte[])value));
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)) { return "___"; }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    [TypeConverter(typeof(IconClassConverter))]
    public class IconClassConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(MPDrawing::System.Drawing.Icon))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is byte[])
            {
                return new MPDrawing::System.Drawing.Icon(new Bitmap(16, 16));
                return new MPDrawing::System.Drawing.Icon(new MemoryStream((byte[])value));
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string)) { return "___"; }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    

    // Represents the database
    public class ExampleDb : IndexedDb
        {
            public ExampleDb(IJSRuntime jSRuntime, string name, int version) : base(jSRuntime, name, version) { }

            // These are like tables. Declare as many of them as you want.
            public IndexedSet<Person> People { get; set; }
        }

        public class Person
        {
            [Key]
            public long Id { get; set; }

            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }
        }
        public class WASMPort : ICommsSerial
        {
            public void Dispose()
            {
                
            }

            public Stream BaseStream { get; }
            public int BaudRate { get; set; }
            public int BytesToRead { get; }
            public int BytesToWrite { get; }
            public int DataBits { get; set; }
            public bool DtrEnable { get; set; }
            public bool IsOpen { get; }
            public string PortName { get; set; }
            public int ReadBufferSize { get; set; }
            public int ReadTimeout { get; set; }
            public bool RtsEnable { get; set; }
            public int WriteBufferSize { get; set; }
            public int WriteTimeout { get; set; }
            public void Close()
            {
                throw new NotImplementedException();
            }

            public void DiscardInBuffer()
            {
                throw new NotImplementedException();
            }

            public void Open()
            {
                throw new NotImplementedException();
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public int ReadByte()
            {
                throw new NotImplementedException();
            }

            public int ReadChar()
            {
                throw new NotImplementedException();
            }

            public string ReadExisting()
            {
                throw new NotImplementedException();
            }

            public string ReadLine()
            {
                throw new NotImplementedException();
            }

            public void Write(string text)
            {
                throw new NotImplementedException();
            }

            public void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public void WriteLine(string text)
            {
                throw new NotImplementedException();
            }

            public void toggleDTR()
            {
                throw new NotImplementedException();
            }
        }
}
