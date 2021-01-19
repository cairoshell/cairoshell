using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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

        internal void LoadExtensions()
        {
            var pluginService = Host.Services.GetService<IExtensionService>();
            pluginService?.Start();
        }

        internal void WriteApplicationDebugInfoToConsole()
        {
            _logger.LogInformation($"Version: {ProductVersion}");
            _logger.LogInformation($"Operating System: {EnvironmentHelper.WindowsProductName} {Environment.OSVersion.Version}");
            _logger.LogInformation($"Processor Type: {(IntPtr.Size == 8 || EnvironmentHelper.IsWow64 ? 64 : 32)}-bit");
            _logger.LogInformation($"Running As: {IntPtr.Size * 8}-bit Process");
            _logger.LogInformation($"Startup Path: {StartupPath}");
            _logger.LogInformation($"Configured as shell: {EnvironmentHelper.IsAppConfiguredAsShell}");
            _logger.LogInformation($"Running as shell: {EnvironmentHelper.IsAppRunningAsShell}");
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