using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class ShowRunDialogCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ShowRunDialogCommandInfo _info = new ShowRunDialogCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            ShellHelper.ShowRunDialog(DisplayString.sRun_Title, DisplayString.sRun_Info);
            return true;
        }
    }

    public class ShowRunDialogCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ShowRunDialog";

        public string Description => "Opens the Run dialog window.";

        public string Label => DisplayString.sCairoMenu_Run;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
