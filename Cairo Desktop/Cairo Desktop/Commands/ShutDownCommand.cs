using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class ShutDownCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ShutDownCommandInfo _info = new ShutDownCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            SystemPower.ShowShutdownConfirmation();

            return true;
        }
    }

    public class ShutDownCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ShutDown";

        public string Description => "Opens a dialog prompting to shut down the system.";

        public string Label => DisplayString.sCairoMenu_ShutDown;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
