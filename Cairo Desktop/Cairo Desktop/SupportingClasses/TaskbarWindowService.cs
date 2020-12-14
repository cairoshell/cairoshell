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
            AppBarHelper.HideWindowsTaskbar();
        }

        protected override void OpenWindow(Screen screen)
        {
            Taskbar newTaskbar = new Taskbar(_windowManager, _desktopManager, screen);
            newTaskbar.Show();
            Windows.Add(newTaskbar);
        }

        public override void Dispose()
        {
            AppBarHelper.ShowWindowsTaskbar();
        }
    }
}
