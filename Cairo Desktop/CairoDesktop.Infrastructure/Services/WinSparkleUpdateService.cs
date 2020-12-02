using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Interop;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;

namespace CairoDesktop.Infrastructure.Services
{
    public class WinSparkleApplicationUpdateService : DisposableObject, IApplicationUpdateService
    {
        private const string UpdateUrl = "https://cairoshell.github.io/appdescriptor.rss";

        private readonly ILogger<WinSparkleApplicationUpdateService> _logger;

        private bool _isInitialized;
        private WinSparkle.win_sparkle_can_shutdown_callback_t _canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t _shutdownDelegate;

        public WinSparkleApplicationUpdateService(ILogger<WinSparkleApplicationUpdateService> logger)
        {
            _logger = logger;
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

        public void Initialize(Action shutdownDelegate)
        {
            if (_isInitialized) 
                return;

            if (shutdownDelegate == null)
            {
                _logger.LogError("UpdateManager: Unable to initialize; shutdownDelegate is null");
                return;
            }

            WinSparkle.win_sparkle_set_appcast_url(UpdateUrl);

            _canShutdownDelegate = canShutdown;
            _shutdownDelegate = shutdownDelegate.Invoke;

            WinSparkle.win_sparkle_set_can_shutdown_callback(_canShutdownDelegate);
            WinSparkle.win_sparkle_set_shutdown_request_callback(_shutdownDelegate);
            WinSparkle.win_sparkle_init();

            _isInitialized = true;
        }

        public void CheckForUpdates()
        {
            WinSparkle.win_sparkle_check_update_with_ui();
        }

        private int canShutdown()
        {
            return 1;
        }

        protected override void DisposeOfUnManagedResources()
        {
            WinSparkle.win_sparkle_cleanup();
        }
    }
}