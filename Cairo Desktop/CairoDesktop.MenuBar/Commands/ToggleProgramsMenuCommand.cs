using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.MenuBar.Services;
using System;
using System.Collections.Generic;
using CairoDesktop.Common.Helpers;

namespace CairoDesktop.MenuBar.Commands
{
    public class ToggleProgramsMenuCommand : ICairoCommand, IDisposable
    {
        public ICairoCommandInfo Info => _info;

        private readonly Settings _settings;
        private readonly MenuBarWindowService _windowService;
        private readonly ToggleProgramsMenuCommandInfo _info = new ToggleProgramsMenuCommandInfo();

        public ToggleProgramsMenuCommand(IEnumerable<IWindowService> windowServices, Settings settings)
        {
            foreach (var windowService in windowServices)
            {
                if (windowService is MenuBarWindowService menuBarWindowService)
                {
                    _windowService = menuBarWindowService;
                }
            }

            _settings = settings;
            _settings.PropertyChanged += Settings_PropertyChanged;
            SetAvailable();
        }

        private void SetAvailable()
        {
            _info.IsAvailable = _settings.EnableProgramsMenu;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(_settings.EnableProgramsMenu))
            {
                SetAvailable();
            }
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            if (_windowService == null || !_settings.EnableProgramsMenu)
            {
                return false;
            }

            foreach (MenuBar menuBar in _windowService.Windows)
            {
                if (!_settings.EnableMenuBarMultiMon || CursorHelper.IsCursorOnScreen(menuBar.Screen))
                {
                    menuBar.ToggleProgramsMenu();
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            _settings.PropertyChanged -= Settings_PropertyChanged;
        }
    }

    public class ToggleProgramsMenuCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ToggleProgramsMenu";

        public string Description => "Toggles the programs menu.";

        public string Label => DisplayString.sCommand_ToggleProgramsMenu;

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            internal set
            {
                _isAvailable = value;
            }
        }

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
