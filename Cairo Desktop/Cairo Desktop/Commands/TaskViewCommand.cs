using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class TaskViewCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly TaskViewCommandInfo _info = new TaskViewCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            ShellHelper.ShowWindowSwitcher();
            return true;
        }
    }

    public class TaskViewCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "TaskView";

        public string Description => "Shows the Windows Task View.";

        public string Label => DisplayString.sTaskbar_TaskView;

        public bool IsAvailable => EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
