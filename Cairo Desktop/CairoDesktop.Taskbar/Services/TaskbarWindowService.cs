using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.Infrastructure.Services;
using CairoDesktop.Taskbar.SupportingClasses;
using ManagedShell.AppBar;
using ManagedShell.WindowsTasks;

namespace CairoDesktop.Taskbar.Services
{
    public class TaskbarWindowService : AppBarWindowService
    {
        private readonly IAppGrabber _appGrabber;
        private readonly IDesktopManager _desktopManager;
        private readonly ICommandService _commandService;

        private bool tasksInitialized;

        public TaskbarWindowService(ICairoApplication cairoApplication,
            AppBarEventService appBarEventService,
            ShellManagerService shellManagerService,
            IWindowManager windowManager,
            IDesktopManager desktopManager, 
            IAppGrabber appGrabber,
            ICommandService commandService,
            Settings settings) 
            : base(cairoApplication, appBarEventService, shellManagerService, windowManager, settings)
        {
            _appGrabber = appGrabber;
            _desktopManager = desktopManager;
            _commandService = commandService;

            EnableMultiMon = _settings.EnableTaskbarMultiMon;
            EnableService = _settings.EnableTaskbar;

            _appBarEventService.AppBarEvent += AppBarEvent;

            if (EnableService)
            {
                initTasks();
                _shellManager.ExplorerHelper.HideExplorerTaskbar = true;
            }
        }

        protected override void HandleSettingChange(string setting)
        {
            switch (setting)
            {
                case "EnableTaskbar":
                    if (_settings.EnableTaskbar)
                    {
                        initTasks();
                    }
                    _shellManager.ExplorerHelper.HideExplorerTaskbar = _settings.EnableTaskbar;

                    HandleEnableServiceChanged(_settings.EnableTaskbar);
                    break;
                case "EnableTaskbarMultiMon":
                    HandleEnableMultiMonChanged(_settings.EnableTaskbarMultiMon);
                    break;
                case "TaskbarGroupingStyle":
                    _shellManager.Tasks.SetTaskCategoryProvider(getTaskCategoryProvider());
                    break;
            }
        }

        private void AppBarEvent(object sender, AppBarEventArgs e)
        {
            if (EnableService && _settings.TaskbarMode == 2)
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

            _appBarEventService.AppBarEvent -= AppBarEvent;

            if (EnableService)
            {
                _shellManager.ExplorerHelper.HideExplorerTaskbar = false;
            }
        }

        private void initTasks()
        {
            if (tasksInitialized)
            {
                return;
            }

            _shellManager.Tasks.Initialize(getTaskCategoryProvider(), true);
            tasksInitialized = true;
        }

        private ITaskCategoryProvider getTaskCategoryProvider()
        {
            if (_settings.TaskbarGroupingStyle == 0)
            {
                return new AppGrabberTaskCategoryProvider(_appGrabber, _shellManager);
            }
            else
            {
                return new ApplicationTaskCategoryProvider();
            }
        }
    }
}
