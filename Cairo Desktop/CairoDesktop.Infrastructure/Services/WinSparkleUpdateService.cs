using CairoDesktop.Application.Interfaces;
using CairoDesktop.Interop;
using Microsoft.Extensions.Logging;
using System;
using ManagedShell.Common;

namespace CairoDesktop.Infrastructure.Services
{
    public class WinSparkleApplicationUpdateService : DisposableObject, IApplicationUpdateService
    {
        private const string UpdateUrl = "https://cairoshell.github.io/appdescriptor.rss";
        private const string AppName = "Cairo Desktop Environment";
        private const string CompanyName = "Cairo Development Team";

        private readonly ILogger<WinSparkleApplicationUpdateService> _logger;
        private readonly ICairoApplication _app;

        private WinSparkle.win_sparkle_can_shutdown_callback_t _canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t _shutdownDelegate;

        public WinSparkleApplicationUpdateService(ILogger<WinSparkleApplicationUpdateService> logger, ICairoApplication app)
        {
            _logger = logger;
            _app = app;

            try
            {
                WinSparkle.win_sparkle_set_appcast_url(UpdateUrl);
                WinSparkle.win_sparkle_set_app_details(CompanyName, AppName, _app.AppVersion);

                _canShutdownDelegate = canShutdown;
                _shutdownDelegate = doShutdown;

                WinSparkle.win_sparkle_set_can_shutdown_callback(_canShutdownDelegate);
                WinSparkle.win_sparkle_set_shutdown_request_callback(_shutdownDelegate);
                WinSparkle.win_sparkle_init();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to initialize WinSparkle: {e.Message}");
            }
        }

        public bool AutomaticUpdatesEnabled
        {
            get
            {
                int result = 0;
                try
                {
                    result = WinSparkle.win_sparkle_get_automatic_check_for_updates();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to get WinSparkle settings: {e.Message}");
                }
                
                return Convert.ToBoolean(result);
            }

            set
            {
                int intValue = Convert.ToInt32(value);

                try
                {
                    WinSparkle.win_sparkle_set_automatic_check_for_updates(intValue);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to set WinSparkle settings: {e.Message}");
                }
            }
        }

        public void CheckForUpdates()
        {
            try
            {
                WinSparkle.win_sparkle_check_update_with_ui();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to check for updates: {e.Message}");
            }
        }

        private int canShutdown()
        {
            return 1;
        }

        private void doShutdown()
        {
            _app.ExitCairo();
        }

        protected override void DisposeOfUnManagedResources()
        {
            try
            {
                WinSparkle.win_sparkle_cleanup();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to clean up WinSparkle: {e.Message}");
            }
        }
    }
}