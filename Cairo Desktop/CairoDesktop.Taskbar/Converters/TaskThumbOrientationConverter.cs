using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace CairoDesktop.Taskbar.Converters
{
    [ValueConversion(typeof(bool), typeof(Orientation))]
    public class TaskThumbOrientationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool showThumbnails)
            {
                return showThumbnails ? Orientation.Horizontal : Orientation.Vertical;
            }

            return Orientation.Horizontal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
