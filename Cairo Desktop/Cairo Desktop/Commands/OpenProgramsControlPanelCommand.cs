using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenProgramsControlPanelCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly OpenProgramsControlPanelCommandInfo _info = new OpenProgramsControlPanelCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            return ShellHelper.StartProcess("appwiz.cpl");
        }
    }

    public class OpenProgramsControlPanelCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenProgramsControlPanel";

        public string Description => "Opens the Programs control panel.";

        public string Label => DisplayString.sProgramsMenu_UninstallAProgram;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
