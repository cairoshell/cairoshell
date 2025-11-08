using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.MenuBarExtensions.Commands
{
    public class ToggleSearchCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly Settings _settings;
        private readonly MenuBarExtensionsShellExtension _menuExtrasExtension;
        private readonly ToggleSearchCommandInfo _info = new ToggleSearchCommandInfo();

        public ToggleSearchCommand(IEnumerable<IShellExtension> shellExtensions, Settings settings)
        {
            foreach (var shellExtension in shellExtensions)
            {
                if (shellExtension is MenuBarExtensionsShellExtension menuExtrasExtension)
                {
                    _menuExtrasExtension = menuExtrasExtension;
                }
            }

            _settings = settings;
            _settings.PropertyChanged += Settings_PropertyChanged;
            SetAvailable();
        }

        private void SetAvailable()
        {
            _info.IsAvailable = _settings.EnableMenuExtraSearch;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(_settings.EnableMenuExtraSearch))
            {
                SetAvailable();
            }
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            if (_menuExtrasExtension == null || !_settings.EnableMenuExtraSearch)
            {
                return false;
            }

            foreach (IMenuBarExtension menuExtra in _menuExtrasExtension.MenuExtras)
            {
                if (menuExtra is SearchMenuBarExtension searchMenuExtra)
                {
                    searchMenuExtra.OnShowSearch();
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

    public class ToggleSearchCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ToggleSearch";

        public string Description => "Toggles the search panel.";

        public string Label => DisplayString.sCommand_ToggleSearch;

        public bool IsAvailable { get; internal set; }

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
