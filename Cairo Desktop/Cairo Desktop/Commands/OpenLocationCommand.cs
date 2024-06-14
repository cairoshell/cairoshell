using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Interfaces;
using CairoDesktop.Services;
using ManagedShell.Common.Helpers;
using System;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenLocationCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IDesktopManager _desktopManager;
        private readonly Settings _settings;
        private readonly OpenLocationCommandInfo _info = new OpenLocationCommandInfo();

        public OpenLocationCommand(IDesktopManager desktopManager, Settings settings) {
            _desktopManager = desktopManager;
            _settings = settings;
        }

        public void Setup() { }

        public bool Execute(params (string name, object value)[] parameters)
        {
            string path = null;
            bool openInWindow = false;

            foreach (var parameter in parameters)
            {
                switch (parameter.name)
                {
                    case "Path":
                        if (parameter.value is string _path)
                        {
                            path = _path;
                        }
                        break;
                    case "OpenInWindow":
                        if (parameter.value is bool _openInWindow)
                        {
                            openInWindow = _openInWindow;
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!openInWindow && _settings.EnableDynamicDesktop && _settings.FoldersOpenDesktopOverlay && DesktopManager.IsEnabled)
            {
                try
                {
                    _desktopManager.NavigationManager.NavigateTo(path);
                    _desktopManager.IsOverlayOpen = true;

                    return true;
                }
                catch { }
            }

            _desktopManager.IsOverlayOpen = false;

            var args = Environment.ExpandEnvironmentVariables(path);
            var filename = Environment.ExpandEnvironmentVariables(Settings.Instance.FileManager);

            return ShellHelper.StartProcess(filename, $@"""{args}""");
        }

        public void Dispose() { }
    }

    public class OpenLocationCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "OpenLocation";

        public string Description => "Opens the specified location.";

        public string Label => "Open Location";

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Path to open",
                IsRequired = true
            },
            new CairoCommandParameter {
                Name = "OpenInWindow",
                Description = "Boolean indicating whether the location should be forced to open in a window rather than on the desktop.",
                IsRequired = false
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;
    }
}
