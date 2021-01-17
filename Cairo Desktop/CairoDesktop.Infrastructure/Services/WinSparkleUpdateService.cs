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

        private readonly ILogger<WinSparkleApplicationUpdateService> _logger;
        private readonly ICairoApplication _app;

        private WinSparkle.win_sparkle_can_shutdown_callback_t _canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t _shutdownDelegate;

        public WinSparkleApplicationUpdateService(ILogger<WinSparkleApplicationUpdateService> logger, ICairoApplication app)
        {
            _logger = logger;
            _app = app;

            WinSparkle.win_sparkle_set_appcast_url(UpdateUrl);

            _canShutdownDelegate = canShutdown;
            _shutdownDelegate = doShutdown;

            WinSparkle.win_sparkle_set_can_shutdown_callback(_canShutdownDelegate);
            WinSparkle.win_sparkle_set_shutdown_request_callback(_shutdownDelegate);
            WinSparkle.win_sparkle_init();
        }

        public bool AutomaticUpdatesEnabled
        {
            get
            {
                int result = WinSparkle.win_sparkle_get_automatic_check_for_updates();
                return Convert.ToBoolean(result);
            }

            set
            {
                int intValue = Convert.ToInt32(value);
                WinSparkle.win_sparkle_set_automatic_check_for_updates(intValue);
            }
        }

        public void CheckForUpdates()
        {
            WinSparkle.win_sparkle_check_update_with_ui();
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
            WinSparkle.win_sparkle_cleanup();
        }
    }
}