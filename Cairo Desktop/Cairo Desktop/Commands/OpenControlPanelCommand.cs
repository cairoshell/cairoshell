using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using CairoDesktop.SupportingClasses;
using ManagedShell.Common.Enums;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenControlPanelCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly OpenControlPanelCommandInfo _info = new OpenControlPanelCommandInfo();

        public void Setup() { }

        public bool Execute(params (string name, object value)[] parameters)
        {
            // TODO: Call via command
            return FolderHelper.OpenLocation(ShellFolderPath.ControlPanelFolder.Value);
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
