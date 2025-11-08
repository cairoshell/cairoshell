using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common;
using CairoDesktop.Common.Localization;
using System.Collections.Generic;

namespace CairoDesktop.MenuBarExtensions.Commands
{
    public class ToggleCalendarCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly Settings _settings;
        private readonly MenuBarExtensionsShellExtension _menuExtrasExtension;
        private readonly ToggleCalendarCommandInfo _info = new ToggleCalendarCommandInfo();

        public ToggleCalendarCommand(IEnumerable<IShellExtension> shellExtensions, Settings settings)
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
            _info.IsAvailable = _settings.EnableMenuExtraClock;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(_settings.EnableMenuExtraClock))
            {
                SetAvailable();
            }
        }

        public bool Execute(params (string name, object value)[] parameters)
        {
            if (_menuExtrasExtension == null || !_settings.EnableMenuExtraClock)
            {
                return false;
            }

            foreach (IMenuBarExtension menuExtra in _menuExtrasExtension.MenuExtras)
            {
                if (menuExtra is ClockMenuBarExtension clockMenuExtra)
                {
                    clockMenuExtra.OnShowClock();
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

    public class ToggleCalendarCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "ToggleCalendar";

        public string Description => "Toggles the calendar.";

        public string Label => DisplayString.sCommand_ToggleCalendar;

        public bool IsAvailable { get; internal set; }

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
