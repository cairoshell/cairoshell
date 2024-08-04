using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using ManagedShell.ShellFolders;
using System;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenLocationInWindowCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly IDesktopManager _desktopManager;
        private readonly Settings _settings;
        private readonly OpenLocationInWindowCommandInfo _info = new OpenLocationInWindowCommandInfo();

        public OpenLocationInWindowCommand(IDesktopManager desktopManager, Settings settings)
        {
            _desktopManager = desktopManager;
            _settings = settings;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            string path = null;

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
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            _desktopManager.IsOverlayOpen = false;

            var args = Environment.ExpandEnvironmentVariables(path);
            var filename = Environment.ExpandEnvironmentVariables(_settings.FileManager);

            return ShellHelper.StartProcess(filename, $@"""{args}""");
        }
    }

    public class OpenLocationInWindowCommandInfo : ICairoShellFolderCommandInfo
    {
        public string Identifier => "OpenLocationInWindow";

        public string Description => "Opens the specified location in a new window.";

        public string Label => DisplayString.sStacks_OpenInNewWindow;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Path to open",
                IsRequired = true
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;

        public string LabelForShellFolder(ShellFolder folder)
        {
            return Label;
        }

        public bool IsAvailableForShellFolder(ShellFolder folder)
        {
            return IsAvailable;
        }
    }
}
