using System;
using System.Windows.Data;

namespace CairoDesktop.MenuBarExtensions
{
    [ValueConversion(typeof(double[]), typeof(double))]
    public class NotificationBalloonWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double result = 1.0;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double item)
                    result *= item;
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
