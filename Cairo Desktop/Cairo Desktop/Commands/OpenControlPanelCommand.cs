using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Enums;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenControlPanelCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ICommandService _commandService;
        private readonly OpenControlPanelCommandInfo _info = new OpenControlPanelCommandInfo();

        public OpenControlPanelCommand(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            return _commandService.InvokeCommand("OpenLocation", ("Path", ShellFolderPath.ControlPanelFolder.Value));
        }

        public void Dispose() { }
    }

    public class OpenControlPanelCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenControlPanel";

        public string Description => "Opens the Windows Control Panel.";

        public string Label => DisplayString.sCairoMenu_WindowsControlPanel;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
