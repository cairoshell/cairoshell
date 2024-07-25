using CairoDesktop.Common;
using ManagedShell;
using System;
using System.ComponentModel;

namespace CairoDesktop.Infrastructure.Services
{
    public class ShellManagerService : IDisposable
    {
        private readonly Settings _settings;

        public ShellManager ShellManager { get; }

        public ShellManagerService(Settings settings)
        {
            _settings = settings;

            ShellManager = ConfigureShellManager();

            _settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName))
                return;

            switch (e.PropertyName)
            {
                case "TaskbarIconSize":
                    ShellManager.TasksService.TaskIconSize = _settings.TaskbarIconSize;
                    break;
            }
        }

        private ShellManager ConfigureShellManager()
        {
            ShellConfig config = new ShellConfig()
            {
                EnableTasksService = true,
                AutoStartTasksService = false,
                TaskIconSize = _settings.TaskbarIconSize,

                EnableTrayService = _settings.EnableSysTray,
                AutoStartTrayService = false,
                PinnedNotifyIcons = _settings.PinnedNotifyIcons
            };

            return new ShellManager(config);
        }

        public void Dispose()
        {
            ShellManager.Dispose();
        }
    }
}
