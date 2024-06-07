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

        public void Setup() { }

        public bool Execute(params object[] parameters)
        {
            SystemPower.ShowLogOffConfirmation();

            return true;
        }

        public void Dispose() { }
    }

    public class LogOffCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "LogOff";

        public string Description => "Opens a dialog prompting the user to log off.";

        public string Label => DisplayString.sCairoMenu_LogOff;

        public bool IsAvailable => true;

        public List<CairoCommandParameter> Parameters => new List<CairoCommandParameter>();
    }
}
