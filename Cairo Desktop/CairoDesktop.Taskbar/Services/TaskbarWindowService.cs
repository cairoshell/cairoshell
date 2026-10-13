using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.Infrastructure.Services;
using CairoDesktop.Taskbar.SupportingClasses;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using ManagedShell.Interop;
using ManagedShell.WindowsTasks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CairoDesktop.Taskbar.Services
{
    public class TaskbarWindowService : AppBarWindowService
    {
        private readonly IAppGrabber _appGrabber;
        private readonly IDesktopManager _desktopManager;
        private readonly ICommandService _commandService;

        private static readonly TimeSpan[] _taskbarButtonResyncDelays = { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) };
        private bool tasksInitialized;
        private CancellationTokenSource _taskbarButtonResyncCancellationTokenSource;

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
            _taskbarButtonResyncCancellationTokenSource?.Cancel();
            _taskbarButtonResyncCancellationTokenSource?.Dispose();

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
            scheduleTaskbarButtonResync();
            tasksInitialized = true;
        }

        private void scheduleTaskbarButtonResync()
        {
            if (EnvironmentHelper.IsServerCore || EnvironmentHelper.IsAppRunningAsShell)
            {
                return;
            }

            _taskbarButtonResyncCancellationTokenSource?.Cancel();
            _taskbarButtonResyncCancellationTokenSource?.Dispose();

            _taskbarButtonResyncCancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _taskbarButtonResyncCancellationTokenSource.Token;

            _ = Task.Run(async () =>
            {
                // Some apps cache taskbar hookup state during startup; replay the message a few times to recover missed badges.
                foreach (TimeSpan delay in _taskbarButtonResyncDelays)
                {
                    try
                    {
                        await Task.Delay(delay, token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    sendTaskbarButtonCreatedMessageToKnownWindows();
                }
            }, token);
        }

        private void sendTaskbarButtonCreatedMessageToKnownWindows()
        {
            int taskbarButtonCreatedMessage = NativeMethods.RegisterWindowMessage("TaskbarButtonCreated");

            if (taskbarButtonCreatedMessage <= 0)
            {
                return;
            }

            void sendMessages()
            {
                ApplicationWindow[] windows = _shellManager.Tasks.GroupedWindows
                    .Where(window => window.ShowInTaskbar)
                    .ToArray();

                foreach (ApplicationWindow window in windows)
                {
                    NativeMethods.SendNotifyMessage(window.Handle, (uint)taskbarButtonCreatedMessage, UIntPtr.Zero, IntPtr.Zero);
                }
            }

            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null)
            {
                return;
            }

            if (dispatcher.CheckAccess())
            {
                sendMessages();
            }
            else
            {
                dispatcher.BeginInvoke((Action)sendMessages);
            }
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
