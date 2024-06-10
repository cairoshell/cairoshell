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

        public void Setup() { }

        public bool Execute(List<CairoCommandParameter> parameters)
        {
            SystemPower.ShowShutdownConfirmation();

            return true;
        }

        public void Dispose() { }
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
