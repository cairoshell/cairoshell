using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;

namespace CairoDesktop
{
    public partial class CairoApplication
    {
        private void ProcessCommandLineArgs(string[] args)
        {
            _commandLineParser = new CommandLineParser(args);

            _isRestart = _commandLineParser.ToBoolean("restart");
            _isTour = _commandLineParser.ToBoolean("tour");
            _forceEnableShellMode = _commandLineParser.ToBoolean("shell");
            _forceDisableShellMode = _commandLineParser.ToBoolean("noshell");
        }

        public void SetIsCairoRunningAsShell()
        {
            // check if there is an existing shell window. If not, we will assume the role of shell.
            EnvironmentHelper.IsAppRunningAsShell = (NativeMethods.GetShellWindow() == IntPtr.Zero && !_forceDisableShellMode) || _forceEnableShellMode;
        }

        private bool SingleInstanceCheck()
        {
            _cairoMutex = new System.Threading.Mutex(true, "CairoShell", out bool ok);

            if (!ok && !_isRestart)
            {
                // Another instance is already running.
                return false;
            }
            else if (!ok && _isRestart)
            {
                // this is a restart so let's wait for the old instance to end
                System.Threading.Thread.Sleep(2000);
            }

            return true;
        }

        private void SetShellReadyEvent()
        {
            int hShellReadyEvent;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && EnvironmentHelper.IsWindows2kOrBetter)
            {
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, true, @"Global\msgina: ShellReadyEvent");
            }
            else
            {
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, "msgina: ShellReadyEvent");
            }

            if (hShellReadyEvent != 0)
            {
                NativeMethods.SetEvent(hShellReadyEvent);
                NativeMethods.CloseHandle(hShellReadyEvent);
            }
        }

        private void SetupSettings()
        {
            if (Settings.Instance.IsFirstRun)
            {
                Settings.Instance.Upgrade();
            }
        }

        internal void LoadExtensions()
        {
            var pluginService = Host.Services.GetService<IExtensionService>();
            pluginService?.Start();
        }

        internal void WriteApplicationDebugInfoToConsole()
        {
            _logger.LogInformation($"{ProductName}");
            _logger.LogInformation($"Version: {ProductVersion}");
            _logger.LogInformation($"Operating System: {new ComputerInfo().OSFullName}");
            _logger.LogInformation($"OS Build: {new ComputerInfo().OSVersion}");
            _logger.LogInformation($"Processor Type: {(IntPtr.Size == 8 || InternalCheckIsWow64() ? 64 : 32)}-bit");
            _logger.LogInformation($"Startup Path: {StartupPath}");
            _logger.LogInformation($"Running As: {IntPtr.Size * 8}-bit Process");
            _logger.LogInformation($"Configured as shell: {EnvironmentHelper.IsAppConfiguredAsShell}");
            _logger.LogInformation($"Running as shell: {EnvironmentHelper.IsAppRunningAsShell}");
        }

        internal bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;

                    try
                    {
                        if (!NativeMethods.IsWow64Process(p.Handle, out retVal))
                        {
                            return false;
                        }
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

        internal void SetSystemKeyboardShortcuts()
        {
            if (EnvironmentHelper.IsAppRunningAsShell)
            {
                // Commenting out as per comments on PR #274
                SupportingClasses.SystemHotKeys.RegisterSystemHotkeys();
            }
        }

        private void SetupWindowServices()
        {
            foreach (var service in Host.Services.GetServices<IWindowService>())
            {
                service.Register();
            }

            Host.Services.GetService<WindowManager>()?.InitialSetup();
        }
    }
}