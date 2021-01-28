using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.Services;
using ManagedShell.AppBar;

namespace CairoDesktop.Services
{
    public class TaskbarWindowService : AppBarWindowService
    {
        private readonly AppGrabberService _appGrabber;
        private readonly DesktopManager _desktopManager;

        public TaskbarWindowService(ICairoApplication cairoApplication, ShellManagerService shellManagerService, WindowManager windowManager, DesktopManager desktopManager, AppGrabberService appGrabber) : base(cairoApplication, shellManagerService, windowManager)
        {
            _appGrabber = appGrabber;
            _desktopManager = desktopManager;

            EnableMultiMon = Settings.Instance.EnableTaskbarMultiMon;
            EnableService = Settings.Instance.EnableTaskbar;

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
                    _shellManager.ExplorerHelper.HideExplorerTaskbar = Settings.Instance.EnableTaskbar;
                    
                    HandleEnableServiceChanged(Settings.Instance.EnableTaskbar);
                    break;
                case "EnableTaskbarMultiMon":
                    HandleEnableMultiMonChanged(Settings.Instance.EnableTaskbarMultiMon);
                    break;
            }
        }

        private void AppBarEvent(object sender, AppBarEventArgs e)
        {
            if (Settings.Instance.TaskbarMode == 2)
            {
                if (sender is MenuBar menuBar)
                {
                    var taskbar = (Taskbar) WindowManager.GetScreenWindow(Windows, menuBar.Screen);

                    if (taskbar == null)
                    {
                        return;
                    }

                    if (taskbar.AppBarEdge != menuBar.AppBarEdge)
                    {
                        return;
                    }

                    if (e.Reason == AppBarEventReason.MouseEnter)
                    {
                        taskbar.CanAutoHide = false;
                    }
                    else if (e.Reason == AppBarEventReason.MouseLeave)
                    {
                        taskbar.CanAutoHide = true;
                    }
                }
            }
        }

        protected override void OpenWindow(AppBarScreen screen)
        {
            Taskbar newTaskbar = new Taskbar(_cairoApplication, _shellManager, _windowManager, _desktopManager, _appGrabber, screen, Settings.Instance.TaskbarPosition == 1 ? AppBarEdge.Top : AppBarEdge.Bottom);
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
