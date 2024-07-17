using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CairoDesktop.Commands
{
    public class AboutCairoCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly AboutCairoCommandInfo _info = new AboutCairoCommandInfo();

        public bool Execute(params (string name, object value)[] parameters)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            CairoMessage.Show(
                DisplayString.sAbout_Version + " " + version + " - " + DisplayString.sAbout_PreRelease
                + "\n\n" + string.Format(DisplayString.sAbout_Copyright, DateTime.Now.Year.ToString()), "Cairo Desktop Environment", CairoMessageImage.Default);

            return true;
        }
    }

    public class AboutCairoCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "AboutCairo";

        public string Description => "Opens the About Cairo window.";

        public string Label => DisplayString.sCairoMenu_AboutCairo;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
