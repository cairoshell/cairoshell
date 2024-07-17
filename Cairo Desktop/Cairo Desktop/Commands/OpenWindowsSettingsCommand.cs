using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenWindowsSettingsCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly OpenWindowsSettingsCommandInfo _info = new OpenWindowsSettingsCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            return ShellHelper.StartProcess("ms-settings://");
        }
    }

    public class OpenWindowsSettingsCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenWindowsSettings";

        public string Description => "Opens the Windows Settings app.";

        public string Label => DisplayString.sCairoMenu_WindowsSettings;

        public bool IsAvailable => EnvironmentHelper.IsWindows10OrBetter && !EnvironmentHelper.IsAppRunningAsShell;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
