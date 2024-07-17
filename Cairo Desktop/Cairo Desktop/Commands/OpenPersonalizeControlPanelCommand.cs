using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenPersonalizeControlPanelCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ICommandService _commandService;
        private readonly OpenPersonalizeControlPanelCommandInfo _info = new OpenPersonalizeControlPanelCommandInfo();

        public OpenPersonalizeControlPanelCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            if (EnvironmentHelper.IsAppRunningAsShell)
            {
                return _commandService.InvokeCommand("OpenCairoSettings", ("TabIdentifier", "desktop"));
            }

            return ShellHelper.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,2");
        }
    }

    public class OpenPersonalizeControlPanelCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenPersonalizeControlPanel";

        public string Description => "Opens the Personalize control panel.";

        public string Label => DisplayString.sDesktop_Personalize;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
