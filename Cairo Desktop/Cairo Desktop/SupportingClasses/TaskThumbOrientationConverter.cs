using CairoDesktop.Configuration;
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace CairoDesktop.SupportingClasses
{
    [ValueConversion(typeof(bool), typeof(Orientation))]
    public class TaskThumbOrientationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Settings.Instance.EnableTaskbarThumbnails)
            {
                return Orientation.Horizontal;
            }
            else
            {
                return Orientation.Vertical;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
