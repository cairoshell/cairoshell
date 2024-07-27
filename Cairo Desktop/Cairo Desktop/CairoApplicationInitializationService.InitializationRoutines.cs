using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.Infrastructure.Options;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace CairoDesktop
{
    public class CairoApplicationInitializationService : IInitializationService
    {
        private readonly IHost _host;
        private readonly ILogger<CairoApplicationInitializationService> _logger;
        private readonly IOptionsMonitor<CommandLineOptions> _options;
        private readonly IThemeService _themeService;

        public CairoApplicationInitializationService(
            IHost host,
            ILogger<CairoApplicationInitializationService> logger,
            IOptionsMonitor<CommandLineOptions> options,
            IThemeService themeService)
        {
            _host = host;
            _logger = logger;
            _options = options;
            _themeService = themeService;
        }

        void IInitializationService.SetIsCairoRunningAsShell()
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

        void IInitializationService.LoadCommands()
        {
            try
            {
                var commandService = _host.Services.GetService<ICommandService>();
                commandService?.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to start command service due to exception in {ex.TargetSite?.Module}: {ex.Message}");
            }
        }

        void IInitializationService.LoadExtensions()
        {
            try
            {
                var pluginService = _host.Services.GetService<IExtensionService>();
                pluginService?.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to start extension service due to exception in {ex.TargetSite?.Module}: {ex.Message}");
            }
        }

        void IInitializationService.WriteApplicationDebugInfoToConsole(Version productVersion)
        {
            _logger.LogInformation($"Version: {productVersion}");
            _logger.LogInformation($"Operating System: {EnvironmentHelper.WindowsProductName} {Environment.OSVersion.Version}");
            _logger.LogInformation($"Processor Type: {(IntPtr.Size == 8 || EnvironmentHelper.IsWow64 ? 64 : 32)}-bit");
            _logger.LogInformation($"Running As: {IntPtr.Size * 8}-bit Process");
            _logger.LogInformation($"Configured as shell: {EnvironmentHelper.IsAppConfiguredAsShell}");
            _logger.LogInformation($"Running as shell: {EnvironmentHelper.IsAppRunningAsShell}");
        }

        void IInitializationService.SetTheme()
        {
            _themeService.SetThemeFromSettings();
        }

        void IInitializationService.SetupWindowServices()
        {
            foreach (var service in _host.Services.GetServices<IWindowService>())
            {
                service.Register();
            }

            _host.Services.GetService<IDesktopManager>()?.Initialize();
            _host.Services.GetService<IWindowManager>()?.InitialSetup();
        }
    }
}