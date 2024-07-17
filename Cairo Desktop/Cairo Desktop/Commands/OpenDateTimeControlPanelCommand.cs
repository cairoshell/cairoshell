using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenDateTimeControlPanelCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly OpenDateTimeControlPanelCommandInfo _info = new OpenDateTimeControlPanelCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            return ShellHelper.StartProcess("timedate.cpl");
        }
    }

    public class OpenDateTimeControlPanelCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenDateTimeControlPanel";

        public string Description => "Opens the Date & Time control panel.";

        public string Label => DisplayString.sMenuBar_OpenDateTimeSettings;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
