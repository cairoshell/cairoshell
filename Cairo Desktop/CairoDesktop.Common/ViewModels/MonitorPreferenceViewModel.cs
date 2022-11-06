using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

using CairoDesktop.Common.ExtensionMethods;
using CairoDesktop.Configuration;

using ManagedShell.AppBar;

namespace CairoDesktop.Common.ViewModels;

class MonitorPreferenceViewModel : INotifyPropertyChanged
{
    #region Display-related
    public int DesktopWidth { get; }
    public int DesktopHeight { get; }
    public int PrimaryScreenX { get; }
    public int PrimaryScreenY { get; }
    public ReadOnlyObservableCollection<ScreenViewModel> Screens { get; }
    #endregion

    MonitorPreference _preference = MonitorPreference.Parse("Primary");
    public MonitorPreference Preference
    {
        get => _preference;
        set
        {
            if (value == _preference) return;
            _preference = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UsePrimary));
            OnPropertyChanged(nameof(UseScreen));
            foreach (var screen in Screens)
            {
                screen.OnPropertyChanged(nameof(ScreenViewModel.IsSelected));
            }
        }
    }

    public bool UsePrimary
    {
        get => _preference is PrimaryMonitorPreference;
        set
        {
            if (value)
            {
                Preference = MonitorPreference.Parse("Primary");
            }
        }
    }

    public AppBarScreen UseScreen
    {
        get => _preference is MonitorFromCoordinatesPreference preference
                ? Screens.FirstOrDefault(s => s.Bounds.Contains(preference.Coordinates))?.Screen
                : null;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(UseScreen));

            Preference = new MonitorFromCoordinatesPreference(value.Bounds.Center());
        }
    }

    public MonitorPreferenceViewModel()
    {
        var screens = AppBarScreen.FromAllScreens().Select(s => new ScreenViewModel(this, s));
        Screens = new(new(screens));
        int left = Screens.Min(s => s.Bounds.Left);
        int top = Screens.Min(s => s.Bounds.Top);
        DesktopWidth = Screens.Max(s => s.Bounds.Right) - left;
        DesktopHeight = Screens.Max(s => s.Bounds.Bottom) - top;
        PrimaryScreenX = -left;
        PrimaryScreenY = -top;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public sealed class ScreenViewModel : INotifyPropertyChanged
    {
        readonly MonitorPreferenceViewModel _parent;
        public AppBarScreen Screen { get; }
        public Rectangle Bounds => Screen.Bounds;
        public bool IsSelected
        {
            get => Screen == _parent.UseScreen;
            set
            {
                if (value)
                {
                    _parent.UseScreen = Screen;
                }
                OnPropertyChanged();
            }
        }
        internal ScreenViewModel(MonitorPreferenceViewModel parent, AppBarScreen screen)
        {
            _parent = parent;
            Screen = screen;
        }

        internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
