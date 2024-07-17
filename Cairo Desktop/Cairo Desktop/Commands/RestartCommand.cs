using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class RestartCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly RestartCommandInfo _info = new RestartCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            SystemPower.ShowRebootConfirmation();

            return true;
        }
    }

    public class RestartCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "Restart";

        public string Description => "Opens a dialog prompting to restart the system.";

        public string Label => DisplayString.sCairoMenu_Restart;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
