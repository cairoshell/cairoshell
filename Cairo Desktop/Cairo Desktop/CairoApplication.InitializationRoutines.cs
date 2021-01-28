using CairoDesktop.Application.Interfaces;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using CairoDesktop.Services;

namespace CairoDesktop
{
    public partial class CairoApplication
    {
        public void SetIsCairoRunningAsShell()
        {
            bool forceNoShell = false;
            bool forceShell = false;

            try
            {
                forceNoShell = _options.CurrentValue.NoShell;
                forceShell = _options.CurrentValue.Shell;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to read shell command line options: {e.Message}");
            }

            // check if there is an existing shell window. If not, we will assume the role of shell.
            EnvironmentHelper.IsAppRunningAsShell = (NativeMethods.GetShellWindow() == IntPtr.Zero && !forceNoShell) || forceShell;
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
            _logger.LogInformation($"Configured as shell: {EnvironmentHelper.IsAppConfiguredAsShell}");
            _logger.LogInformation($"Running as shell: {EnvironmentHelper.IsAppRunningAsShell}");
        }

        private void SetTheme()
        {
            // TODO: Find a cleaner way to do this. We can't inject it to CairoApplication since it has a dependency on ICairoApplication.
            // We could work around this using something similar to how WindowManager receives WindowServices, but that doesn't seem any better.
            Host.Services.GetService<ThemeService>()?.SetThemeFromSettings();
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