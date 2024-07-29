using System;
using System.Windows;
using System.Windows.Data;
using CairoDesktop.Common.Helpers;
using ManagedShell.WindowsTasks;

namespace CairoDesktop.Taskbar.Converters
{
    [ValueConversion(typeof(bool), typeof(Style))]
    public class TaskButtonStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(values[0] is FrameworkElement fxElement))
            {
                return null;
            }

            // Default style is Inactive...
            var fxStyle = fxElement.FindResource("CairoTaskbarButtonInactiveStyle");
            if (values[1] == DependencyProperty.UnsetValue || values[1] == null)
            {
                // Default - couldn't get window state.
                return fxStyle;
            }

            EnumUtility.TryCast(values[1], out ApplicationWindow.WindowState winState, ApplicationWindow.WindowState.Inactive);

            if (winState == ApplicationWindow.WindowState.Active)
            {
                fxStyle = fxElement.FindResource("CairoTaskbarButtonActiveStyle");
            }

            return fxStyle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
