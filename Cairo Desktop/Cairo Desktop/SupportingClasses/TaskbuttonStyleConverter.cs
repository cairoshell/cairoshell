using System;
using System.Windows;
using System.Windows.Data;
using CairoDesktop.WindowsTasks;

namespace CairoDesktop.SupportingClasses
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
            if (values[1] == null)
            {
                // Default - couldn't get window state.
                return fxStyle;
            }

            EnumUtility.TryCast(values[1], out ApplicationWindow.WindowState winState, ApplicationWindow.WindowState.Inactive);
            switch (winState)
            {
                case ApplicationWindow.WindowState.Active:
                    fxStyle = fxElement.FindResource("CairoTaskbarButtonActiveStyle");
                    break;

                case ApplicationWindow.WindowState.Flashing:
                    fxStyle = fxElement.FindResource("CairoTaskbarButtonFlashingStyle");
                    break;

                    // case ApplicationWindow.WindowState.Hidden:
                    //     fxStyle = fxElement.FindResource("CairoTaskbarButtonHiddenStyle");
                    //     break;
            }

            return fxStyle;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
