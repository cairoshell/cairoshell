using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenDisplayControlPanelCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly OpenDisplayControlPanelCommandInfo _info = new OpenDisplayControlPanelCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            return ShellHelper.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,3");
        }
    }

    public class OpenDisplayControlPanelCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenDisplayControlPanel";

        public string Description => "Opens the Display control panel.";

        public string Label => DisplayString.sDesktop_DisplaySettings;

        public bool IsAvailable => !EnvironmentHelper.IsWindows10OrBetter || !EnvironmentHelper.IsAppRunningAsShell;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
