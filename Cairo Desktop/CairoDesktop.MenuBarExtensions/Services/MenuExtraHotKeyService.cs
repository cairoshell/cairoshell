using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.DependencyInjection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CairoDesktop.MenuBarExtensions.Services
{
    public sealed class MenuExtraHotKeyService : CairoBackgroundService
    {
        private HotKey _calendarHotKey;
        private HotKey _searchHotKey;

        private readonly ICairoApplication _cairoApplication;
        private readonly ICommandService _commandService;
        private readonly Settings _settings;

        public MenuExtraHotKeyService(ICairoApplication cairoApplication, ICommandService commandService, Settings settings)
        {
            _cairoApplication = cairoApplication;
            _settings = settings;
            _settings.PropertyChanged += Settings_PropertyChanged;

            ServiceStartTask = new Task(RegisterHotkeys);
            _commandService = commandService;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName)) return;

            switch (e.PropertyName)
            {
                case nameof(_settings.EnableMenuExtraClock):
                    if (_settings.EnableMenuExtraClock)
                    {
                        RegisterCalendarHotKey();
                    }
                    else
                    {
                        UnregisterCalendarHotKey();
                    }

                    break;
                case nameof(_settings.EnableMenuExtraSearch):
                    if (_settings.EnableMenuExtraSearch)
                    {
                        RegisterSearchHotKey();
                    }
                    else
                    {
                        UnregisterSearchHotKey();
                    }

                    break;
            }
        }

        private void RegisterHotkeys()
        {
            RegisterCalendarHotKey();
            RegisterSearchHotKey();
        }

        private void RegisterCalendarHotKey()
        {
            if (!_settings.EnableMenuExtraClock)
            {
                return;
            }

            _cairoApplication.Dispatch(() =>
            {
                _calendarHotKey = new HotKey(Key.D, HotKeyModifier.Win | HotKeyModifier.Alt | HotKeyModifier.NoRepeat, OnToggleCalendar);
            });
        }

        private void UnregisterCalendarHotKey()
        {
            _calendarHotKey?.Dispose();
            _calendarHotKey = null;
        }

        private void RegisterSearchHotKey()
        {
            if (!_settings.EnableMenuExtraSearch)
            {
                return;
            }

            _cairoApplication.Dispatch(() =>
            {
                _searchHotKey = new HotKey(Key.S, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnToggleSearch);
            });
        }

        private void UnregisterSearchHotKey()
        {
            _searchHotKey?.Dispose();
            _searchHotKey = null;
        }

        private void OnToggleCalendar(HotKey obj)
        {
            _commandService.InvokeCommand("ToggleCalendar");
        }

        private void OnToggleSearch(HotKey obj)
        {
            _commandService.InvokeCommand("ToggleSearch");
        }
    }
}
