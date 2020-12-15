using System.Windows.Forms;
using CairoDesktop.Configuration;

namespace CairoDesktop.SupportingClasses
{
    public class TaskbarWindowService : AppBarWindowService
    {
        private readonly DesktopManager _desktopManager;

        public TaskbarWindowService(DesktopManager desktopManager)
        {
            _desktopManager = desktopManager;

            EnableMultiMon = Settings.Instance.EnableTaskbarMultiMon;
            EnableService = Settings.Instance.EnableTaskbar;

            if (EnableService)
            {
                AppBarHelper.HideWindowsTaskbar();
            }
        }

        public override void PostInit()
        {
            if (EnableService)
            {
                _windowManager.AppBarEvent += AppBarEvent;
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

                    if (taskbar.appBarEdge != menuBar.appBarEdge)
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

        protected override void OpenWindow(Screen screen)
        {
            Taskbar newTaskbar = new Taskbar(_windowManager, _desktopManager, screen);
            Windows.Add(newTaskbar);
            newTaskbar.Show();
        }

        public override void Dispose()
        {
            if (EnableService)
            {
                _windowManager.AppBarEvent -= AppBarEvent;
                AppBarHelper.ShowWindowsTaskbar();
            }
        }
    }
}
