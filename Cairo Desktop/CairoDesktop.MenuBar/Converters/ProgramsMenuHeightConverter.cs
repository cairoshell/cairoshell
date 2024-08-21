using CairoDesktop.Common;
using System;
using System.Windows.Data;

namespace CairoDesktop.MenuBar.Converters
{
    [ValueConversion(typeof(void), typeof(double))]
    public class ProgramsMenuHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Settings.Instance.MenuBarEdge == ManagedShell.AppBar.AppBarEdge.Bottom)
            {
                // To prevent the menu from moving from under the cursor while on bottom, set a constant height
                return 400;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
