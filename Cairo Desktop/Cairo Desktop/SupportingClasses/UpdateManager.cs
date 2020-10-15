using System;
using CairoDesktop.Common.DesignPatterns;
using CairoDesktop.Interop;

namespace CairoDesktop.SupportingClasses
{
    public class UpdateManager : SingletonObject<UpdateManager>, IDisposable
    {
        private const string UpdateUrl = "https://cairoshell.github.io/appdescriptor.rss";

        public bool AutoUpdatesEnabled
        {
            get => Convert.ToBoolean(WinSparkle.win_sparkle_get_automatic_check_for_updates());
            set => WinSparkle.win_sparkle_set_automatic_check_for_updates(Convert.ToInt32(value));
        }

        private bool isInitialized;
        private WinSparkle.win_sparkle_can_shutdown_callback_t canShutdownDelegate;
        private WinSparkle.win_sparkle_shutdown_request_callback_t shutdownDelegate;

        private UpdateManager() { }

        public void Initialize()
        {
            if (!isInitialized)
            {
                WinSparkle.win_sparkle_set_appcast_url(UpdateUrl);
                canShutdownDelegate = canShutdown;
                shutdownDelegate = Startup.Shutdown;
                WinSparkle.win_sparkle_set_can_shutdown_callback(canShutdownDelegate);
                WinSparkle.win_sparkle_set_shutdown_request_callback(shutdownDelegate);
                WinSparkle.win_sparkle_init();
                isInitialized = true;
            }
        }

        public void CheckForUpdates()
        {
            WinSparkle.win_sparkle_check_update_with_ui();
        }

        public void Dispose()
        {
            WinSparkle.win_sparkle_cleanup();
        }

        private int canShutdown()
        {
            return 1;
        }
    }
}
