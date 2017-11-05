namespace CairoDesktop.Common
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converts the MessageboxButton set when the dialog is shown into a visibility value.
    /// </summary>
    [ValueConversion(typeof(MessageBoxButton), typeof(Visibility))]
    public class MessageBoxButtonConverter : IValueConverter
    {
        /// <summary>
        /// Implements the IValueConverter interface to provide visibility conversion for messagebox buttons.
        /// </summary>
        /// <param name="value">The value of the bound object.</param>
        /// <param name="targetType">The targeted type.</param>
        /// <param name="parameter">The parameter passed to the converter.</param>
        /// <param name="culture">The cultuer to convert against.</param>
        /// <returns>Visibilty of the button calling the converter.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                return Visibility.Collapsed;
            }

            Visibility returnVisibility = Visibility.Collapsed;

            MessageBoxButton setButtons = (MessageBoxButton)value;
            switch ((string)parameter)
            {
                case "Ok":
                    if (setButtons == MessageBoxButton.OK || setButtons == MessageBoxButton.OKCancel)
                    {
                        returnVisibility = Visibility.Visible;
                    }

                    break;

                case "Cancel":
                    if (setButtons == MessageBoxButton.OKCancel || setButtons == MessageBoxButton.YesNoCancel)
                    {
                        returnVisibility = Visibility.Visible;
                    }

                    break;

                case "Yes":
                    if (setButtons == MessageBoxButton.YesNo || setButtons == MessageBoxButton.YesNoCancel)
                    {
                        returnVisibility = Visibility.Visible;
                    }

                    break;

                case "No":
                    if (setButtons == MessageBoxButton.YesNo || setButtons == MessageBoxButton.YesNoCancel)
                    {
                        returnVisibility = Visibility.Visible;
                    }

                    break;

                default:
                    returnVisibility = Visibility.Collapsed;
                    break;
            }

            return returnVisibility;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>Nothing, Not implemented.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
