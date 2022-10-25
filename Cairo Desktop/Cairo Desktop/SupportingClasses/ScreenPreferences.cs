using System;

using CairoDesktop.Configuration;
using CairoDesktop.Interfaces;

using ManagedShell.AppBar;

namespace CairoDesktop.SupportingClasses
{
    static class ScreenPreferences
    {
        static bool Matches(this AppBarScreen screen, MonitorPreference preference)
        {
            if (preference is MonitorFromCoordinatesPreference coordinates)
            {
                return screen.Bounds.Contains(coordinates.Coordinates.X, coordinates.Coordinates.Y);
            }

            if (preference is PrimaryMonitorPreference _ || preference == null)
            {
                return screen.Primary;
            }
            
            throw new NotImplementedException(preference.GetType().FullName);
        }

        public static AppBarScreen GetScreen(this IWindowManager windowManager, MonitorPreference preference)
            => windowManager.ScreenState.Find(s => s.Matches(preference));

        public static bool IsPreferred(this IWindowManager windowManager, AppBarScreen screen, MonitorPreference preference)
            => ScreenEqualityComparer.Instance.Equals(screen, windowManager.GetScreen(preference));
    }
}
