using System;
using System.Windows.Data;

namespace CairoDesktop.SupportingClasses
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class TaskGroupSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 1;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
