using CairoDesktop.Common;
using ManagedShell.WindowsTasks;
using System;
using System.Windows.Data;

namespace CairoDesktop.Taskbar.Converters
{
    [ValueConversion(typeof(CollectionViewGroup), typeof(string))]
    public class TaskGroupNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is CollectionViewGroup group && group.ItemCount > 0)
            {
                if (Settings.Instance.TaskbarGroupingStyle == 0)
                {
                    return group.Name;
                }

                if (group.Items[0] is ApplicationWindow window)
                {
                    if (window.IsUWP)
                    {
                        return ManagedShell.UWPInterop.StoreAppHelper.AppList.GetAppByAumid(window.AppUserModelID).DisplayName;
                    }

                    return window.WinFileDescription;
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
