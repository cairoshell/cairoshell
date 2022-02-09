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
            string title = string.Empty;

            if (value is NotificationBalloon balloon)
            {
                if (balloon.Info != string.Empty)
                {
                    title = balloon.Info;
                }
                else if (balloon.Title != string.Empty)
                {
                    title = balloon.Title;
                }
                else
                {
                    title = balloon.NotifyIcon.Title;
                }
            }

            return title.Replace('\n', ' ').Replace('\r', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
