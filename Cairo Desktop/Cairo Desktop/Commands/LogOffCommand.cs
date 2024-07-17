using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class LogOffCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly LogOffCommandInfo _info = new LogOffCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            SystemPower.ShowLogOffConfirmation();

            return true;
        }
    }

    public class LogOffCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "LogOff";

        public string Description => "Opens a dialog prompting the user to log off.";

        public string Label => DisplayString.sCairoMenu_LogOff;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
