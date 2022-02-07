using ManagedShell.WindowsTray;
using System;
using System.Windows.Data;

namespace CairoDesktop.MenuBarExtensions
{
    [ValueConversion(typeof(NotificationBalloon), typeof(string))]
    public class NotificationBalloonTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is NotificationBalloon balloon)
            {
                if (balloon.Title != string.Empty) return balloon.Title.Replace('\n', ' ').Replace('\r', ' ');

                return balloon.NotifyIcon.Title.Replace('\n', ' ').Replace('\r', ' ');
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
