using System;
using System.ComponentModel;
using CairoDesktop.Configuration;
using ManagedShell;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Common.Logging.Observers;

namespace CairoDesktop.SupportingClasses
{
    public class ShellManagerService : IDisposable
    {
        public ShellManager ShellManager { get; }

        public ShellManagerService()
        {
            ConfigureLogging();

            ShellManager = ConfigureShellManager();
            
            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName))
                return;

            switch (e.PropertyName)
            {
                case "TaskbarIconSize":
                    ShellManager.TasksService.TaskIconSize = (IconSize) Settings.Instance.TaskbarIconSize;
                    break;
            }
        }

        private void ConfigureLogging()
        {
            //ShellLogger.Severity = LogSeverity.Debug;
            //ShellLogger.Attach(new ConsoleLog());
        }

        private ShellManager ConfigureShellManager()
        {
            ShellConfig config = new ShellConfig()
            {
                EnableTasksService = Settings.Instance.EnableTaskbar,
                AutoStartTasksService = false,
                TaskIconSize = (IconSize)Settings.Instance.TaskbarIconSize,

                EnableTrayService = Settings.Instance.EnableSysTray,
                AutoStartTrayService = false,
                PinnedNotifyIcons = Settings.Instance.PinnedNotifyIcons
            };

            return new ShellManager(config);
        }

        public void Dispose()
        {
            ShellManager.Dispose();
        }
    }
}
