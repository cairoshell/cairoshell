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
            string logsDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cairo_Development_Team");
            string logFilePath = Path.Combine(logsDirectoryPath, "CairoDesktop.log");

            var fileLog = new FileLog(logFilePath);
            fileLog.Open();
            
            CairoLogger.Instance.Attach(fileLog);
            CairoLogger.Instance.Attach(new ConsoleLog());
        }

        internal static void SetupPluginSystem()
        {
            var pluginContext = new PluginContext();
            var fileSystemPluginProvider = new FileSystemPluginProvider(nameof(FileSystemPluginProvider));
            var pluginProviders = new PluginProviderCollection { fileSystemPluginProvider };

            pluginContext.Initialize(pluginProviders);
            pluginContext.Start();
            _CairoShell.OnQuit += a => pluginContext.Stop();
        }


        internal static void WriteApplicationDebugInfoToConsole()
        {
            const string @break = @"#############################################";

            CairoLogger.Instance.Info(@break);
            CairoLogger.Instance.Info(string.Format(@"{0}", Application.ProductName));
            CairoLogger.Instance.Info(string.Format(@"Version: {0}", Application.ProductVersion));
            CairoLogger.Instance.Info(string.Format(@"Operating System: {0}", new ComputerInfo().OSFullName)); //outputs the OS type based on version
            CairoLogger.Instance.Info(string.Format(@"OS Build: {0}", new ComputerInfo().OSVersion));
            CairoLogger.Instance.Info(string.Format(@"Processor Type: {0}", string.Format(@"{0}-bit", IntPtr.Size == 8 || InternalCheckIsWow64() ? 64 : 32)));
            CairoLogger.Instance.Info(string.Format(@"Startup Path: {0}", Application.StartupPath));
            CairoLogger.Instance.Info(string.Format(@"Running As: {0}-bit Process", IntPtr.Size * 8));
            CairoLogger.Instance.Info(@break);
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
    }
}