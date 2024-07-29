using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.Infrastructure.Services;
using ManagedShell.AppBar;

namespace CairoDesktop.Taskbar.Services
{
    public class TaskbarWindowService : AppBarWindowService
    {
        private readonly IAppGrabber _appGrabber;
        private readonly IDesktopManager _desktopManager;
        private readonly ICommandService _commandService;

        public TaskbarWindowService(ICairoApplication cairoApplication,
            ShellManagerService shellManagerService,
            IWindowManager windowManager,
            IDesktopManager desktopManager, 
            IAppGrabber appGrabber,
            ICommandService commandService,
            Settings settings) 
            : base(cairoApplication, shellManagerService, windowManager, settings)
        {
            _appGrabber = appGrabber;
            _desktopManager = desktopManager;
            _commandService = commandService;

            EnableMultiMon = _settings.EnableTaskbarMultiMon;
            EnableService = _settings.EnableTaskbar;

            if (EnableService)
            {
                _shellManager.ExplorerHelper.HideExplorerTaskbar = true;
                _shellManager.AppBarManager.AppBarEvent += AppBarEvent;
            }
        }

        protected override void HandleSettingChange(string setting)
        {
            switch (setting)
            {
                case "EnableTaskbar":
                    _shellManager.ExplorerHelper.HideExplorerTaskbar = _settings.EnableTaskbar;

                    HandleEnableServiceChanged(_settings.EnableTaskbar);
                    break;
                case "EnableTaskbarMultiMon":
                    HandleEnableMultiMonChanged(_settings.EnableTaskbarMultiMon);
                    break;
            }
        }

        private void AppBarEvent(object sender, AppBarEventArgs e)
        {
            if (_settings.TaskbarMode == 2)
            {
                if (sender is Taskbar)
                {
                    return;
                }

                if (sender is CairoAppBarWindow otherBar)
                {
                    var taskbar = (Taskbar)_windowManager.GetScreenWindow(Windows, otherBar.Screen);

                    if (taskbar == null)
                    {
                        return;
                    }

                    if (taskbar.AppBarEdge != otherBar.AppBarEdge)
                    {
                        return;
                    }

                    if (e.Reason == AppBarEventReason.MouseEnter)
                    {
                        taskbar.DisableAutoHide = true;
                    }
                    else if (e.Reason == AppBarEventReason.MouseLeave)
                    {
                        taskbar.DisableAutoHide = false;
                    }
                }
            }
        }

        protected override void OpenWindow(AppBarScreen screen)
        {
            Taskbar newTaskbar = new Taskbar(_cairoApplication, _shellManager, _windowManager, _desktopManager, _appGrabber, _commandService, _settings, screen, _settings.TaskbarEdge, _settings.TaskbarMode);
            Windows.Add(newTaskbar);
            newTaskbar.Show();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (EnableService)
            {
                _shellManager.AppBarManager.AppBarEvent -= AppBarEvent;
                _shellManager.ExplorerHelper.HideExplorerTaskbar = false;
            }
        }
    }
}
