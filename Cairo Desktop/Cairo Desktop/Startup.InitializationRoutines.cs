using CairoDesktop.Common.Logging;
using CairoDesktop.Common.Logging.Observers;
using CairoDesktop.Extensibility.ObjectModel;
using CairoDesktop.Extensibility.Plugins;
using CairoDesktop.Extensibility.Providers;
using CairoDesktop.Extensibility.Providers.Custom;
using CairoDesktop.Interop;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CairoDesktop
{
    public partial class Startup
    {
        private static void SetupLoggingSystem()
        {
            var dateTimeNow = DateTime.Now;
            string logsDirectoryPath = Path.Combine(Application.StartupPath, "Logs");
            string logFilePath = Path.Combine(logsDirectoryPath, string.Format(@"{0:yyyy-MM-dd}_{1}.log", dateTimeNow, dateTimeNow.Ticks));

            var fileLog = new FileLog(logFilePath);
            fileLog.Open();

            SingletonLogger.Instance.Severity = LogSeverity.Debug;
            SingletonLogger.Instance.Attach(fileLog);
            SingletonLogger.Instance.Attach(new ConsoleLog());
        }

        internal static void SetupPluginSystem()
        {
            var pluginContext = new PluginContext();
            var fileSystemPluginProvider = new FileSystemPluginProvider(nameof(FileSystemPluginProvider));
            var pluginProviders = new PluginProviderCollection { fileSystemPluginProvider };

            pluginContext.Initialize(pluginProviders);
            pluginContext.Start();
            _Application.OnQuit += a => pluginContext.Stop();
        }


        internal static void WriteApplicationDebugInfoToConsole()
        {
            const string @break = @"##################################################################################";

            SingletonLogger.Instance.Info(@break);
            SingletonLogger.Instance.Info(string.Format(@"Product: {0}", Application.ProductName));
            SingletonLogger.Instance.Info(string.Format(@"Version: {0}", Application.ProductVersion));
            SingletonLogger.Instance.Info(@break);
            SingletonLogger.Instance.Info(string.Format(@"Operating System: {0}", new ComputerInfo().OSFullName)); //outputs the OS type based on version
            SingletonLogger.Instance.Info(@break);
            SingletonLogger.Instance.Info(string.Format(@"Processor Type: {0}", string.Format(@"{0}-bit", IntPtr.Size == 8 || InternalCheckIsWow64() ? 64 : 32)));
            SingletonLogger.Instance.Info(string.Format(@"Count: {0}", Environment.ProcessorCount));
            SingletonLogger.Instance.Info(@break);
            SingletonLogger.Instance.Info(string.Format(@"Process: {0}", Process.GetCurrentProcess().ProcessName));
            SingletonLogger.Instance.Info(string.Format(@"Startup Path: {0}", Application.StartupPath));
            SingletonLogger.Instance.Info(string.Format(@"Running As: {0}-bit Process", IntPtr.Size * 8));
            SingletonLogger.Instance.Info(@break);
            SingletonLogger.Instance.Info(string.Format(@"Client User Agent: {0}", GetWebBrowserUserAgent()));
            SingletonLogger.Instance.Info(@break);
        }

        internal static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;

                    try
                    {
                        if (!NativeMethods.IsWow64Process(p.Handle, out retVal))
                            return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    return retVal;
                }
            }
            return false;
        }

        private static string GetWebBrowserUserAgent()
        {
            var wb = new WebBrowser { Url = new Uri("about:blank") };
            wb.Document.Write("<script type='text/javascript'>function getUserAgent(){return navigator.userAgent}</script>");
            return wb.Document.InvokeScript("getUserAgent") as string;
        }
    }
}