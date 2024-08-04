using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.ShellFolders;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class OpenLocationCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly ICommandService _commandService;
        private readonly IDesktopManager _desktopManager;
        private readonly Settings _settings;
        private readonly OpenLocationCommandInfo _info;

        public OpenLocationCommand(ICommandService commandService, IDesktopManager desktopManager, Settings settings)
        {
            _commandService = commandService;
            _desktopManager = desktopManager;
            _settings = settings;
            _info = new OpenLocationCommandInfo(desktopManager, settings);
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            string path = null;
            bool noOverlay = false;

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
                    case "NoOverlay":
                        if (parameter.value is bool _noOverlay)
                        {
                            noOverlay = _noOverlay;
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (_settings.EnableDynamicDesktop && _desktopManager.IsEnabled)
            {
                try
                {
                    _desktopManager.SetPath(path);
                    if (!noOverlay)
                    {
                        _desktopManager.IsOverlayOpen = true;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return _commandService.InvokeCommand("OpenLocationInWindow", ("Path", path));
        }
    }

    public class OpenLocationCommandInfo : ICairoShellItemCommandInfo
    {
        public string Identifier => "OpenLocation";

        public string Description => "Opens the specified location on the desktop if possible, otherwise in a new window.";

        public string Label => DisplayString.sCommand_OpenLocation;

        public bool IsAvailable => true;

        private List<CairoCommandParameter> _parameters = new List<CairoCommandParameter>()
        {
            new CairoCommandParameter {
                Name = "Path",
                Description = "Path to open",
                IsRequired = true
            },
            new CairoCommandParameter {
                Name = "NoOverlay",
                Description = "Boolean indicating whether the desktop overlay is suppressed.",
                IsRequired = false
            }
        };

        public IReadOnlyCollection<CairoCommandParameter> Parameters => _parameters;

        private readonly IDesktopManager _desktopManager;
        private readonly Settings _settings;

        public OpenLocationCommandInfo(IDesktopManager desktopManager, Settings settings)
        {
            _desktopManager = desktopManager;
            _settings = settings;
        }

        public string LabelForShellItem(ShellItem item)
        {
            return _settings.EnableDynamicDesktop && _desktopManager.IsEnabled ?
                DisplayString.sStacks_OpenOnDesktop : DisplayString.sStacks_OpenInNewWindow;
        }

        public bool IsAvailableForShellItem(ShellItem item)
        {
            return item.IsNavigableFolder && _settings.EnableDynamicDesktop && _desktopManager.IsEnabled;
        }
    }
}
