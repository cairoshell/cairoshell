using CairoDesktop.Configuration;
using ManagedShell.WindowsTasks;
using System;
using System.Diagnostics;
using System.Windows.Data;

namespace CairoDesktop.SupportingClasses
{
    [ValueConversion(typeof(CollectionViewGroup), typeof(string))]
    public class TaskGroupNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is CollectionViewGroup group)
            {
                if (Settings.Instance.TaskbarGroupingStyle == 0)
                {
                    return group.Name;
                }
                else
                {
                    if (group.Items[0] is ApplicationWindow window)
                    {
                        if (window.IsUWP)
                        {
                            return ManagedShell.UWPInterop.StoreAppHelper.AppList.GetAppByAumid(window.AppUserModelID).DisplayName;
                        }
                        else
                        {
                            return FileVersionInfo.GetVersionInfo(window.WinFileName).FileDescription;
                        }
                    }
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
