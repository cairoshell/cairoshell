using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers; // This using is required when running as release
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class ExitCairoCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ICairoApplication _cairoApplication;
        private readonly ExitCairoCommandInfo _info = new ExitCairoCommandInfo();

        public ExitCairoCommand(ICairoApplication cairoApplication) {
            _cairoApplication = cairoApplication;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            CairoMessage.ShowOkCancel(DisplayString.sExitCairo_Info, DisplayString.sExitCairo_Title,
                CairoMessageImage.Default, DisplayString.sExitCairo_ExitCairo, DisplayString.sInterface_Cancel,
                result =>
                {
                    if (result == true)
                    {
                        if (KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                        {
                            _cairoApplication?.RestartCairo();
                        }
                        else
                        {
                            _cairoApplication?.ExitCairo();
                        }
                    }
                });

            return _cairoApplication != null;
        }
    }

    public class ExitCairoCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ExitCairo";

        public string Description => "Opens the Exit Cairo window.";

        public string Label => DisplayString.sCairoMenu_ExitCairo;

#if DEBUG
        public bool IsAvailable => true;
#else
        public bool IsAvailable => !EnvironmentHelper.IsAppRunningAsShell;
#endif

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
