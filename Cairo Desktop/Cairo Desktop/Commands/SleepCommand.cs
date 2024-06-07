using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class SleepCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly SleepCommandInfo _info = new SleepCommandInfo();

        public void Setup() { }

        public bool Execute(params object[] parameters)
        {
            PowerHelper.Sleep();

            return true;
        }

        public void Dispose() { }
    }

    public class SleepCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "Sleep";

        public string Description => "Puts the system to sleep.";

        public string Label => DisplayString.sCairoMenu_Sleep;

        public bool IsAvailable => PowerHelper.CanSleep();

        public List<CairoCommandParameter> Parameters => new List<CairoCommandParameter>();
    }
}
