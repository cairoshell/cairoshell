using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.MenuBar.Services;
using System.Collections.Generic;
using CairoDesktop.Common.Helpers;

namespace CairoDesktop.MenuBar.Commands
{
    public class ToggleCairoMenuCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly Settings _settings;
        private readonly MenuBarWindowService _windowService;
        private readonly ToggleCairoMenuCommandInfo _info = new ToggleCairoMenuCommandInfo();

        public ToggleCairoMenuCommand(IEnumerable<IWindowService> windowServices, Settings settings)
        {
            foreach (var windowService in windowServices)
            {
                if (windowService is MenuBarWindowService menuBarWindowService)
                {
                    _windowService = menuBarWindowService;
                }
            }

            _settings = settings;
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            if (_windowService == null)
            {
                return false;
            }

            foreach (MenuBar menuBar in _windowService.Windows)
            {
                if (!_settings.EnableMenuBarMultiMon || CursorHelper.IsCursorOnScreen(menuBar.Screen))
                {
                    menuBar.ToggleCairoMenu();
                    return true;
                }
            }

            return false;
        }
    }

    public class ToggleCairoMenuCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ToggleCairoMenu";

        public string Description => "Toggles the Cairo menu.";

        public string Label => DisplayString.sCommand_ToggleCairoMenu;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
