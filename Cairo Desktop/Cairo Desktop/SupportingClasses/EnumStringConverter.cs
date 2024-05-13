using System;
using System.Globalization;
using System.Windows.Data;

namespace CairoDesktop.SupportingClasses
{
    public class EnumStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
