using CairoDesktop.Common.DesignPatterns;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management;

namespace CairoDesktop.Common.Helpers
{
    public sealed class GroupPolicyManager : SingletonObject<GroupPolicyManager>, INotifyPropertyChanged, IDisposable
    {
        private GroupPolicyManager()
        {
            RegistryChangeMonitor registryChangeMonitor = new RegistryChangeMonitor(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            registryChangeMonitor.Changed += RegistryChangeMonitor_Changed;
            registryChangeMonitor.Start();
        }

        private void RegistryChangeMonitor_Changed(object sender, RegistryChangeEventArgs e)
        {
            // Was this one of our keys?
        }
        
        public void Dispose()
        {
            //_watcher.Stop();
            //_watcher.EventArrived -= HandleEvent;
            //_watcher.Dispose();
            //_watcher = null;
        }


        /// <summary>
        /// Removes icons, shortcuts, and other default and user-defined items from the desktop, including 
        /// Briefcase, Recycle Bin, Computer, and Network Locations.
        ///
        /// Removing icons and shortcuts does not prevent the user from using another method to start the 
        /// programs or opening the items they represent.
        ///
        /// Also, see "Items displayed in Places Bar" in User Configuration\Administrative Templates\Windows 
        /// Components\Common Open File Dialog to remove the Desktop icon from the Places Bar.This will help 
        /// prevent users from saving data to the Desktop.
        /// </summary>
        /// <remarks>
        /// Policy: Hide and disable all items on the desktop
        /// Category Path: User Configuration\Administrative Templates\Desktop\
        /// Supported On: At least Microsoft Windows 2000
        /// Registry Key: HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer
        /// Value: NoDesktop
        /// Admx: Desktop.admx
        /// Documentation: https://gpsearch.azurewebsites.net/#146
        /// </remarks>
        public bool NoDesktop
        {
            get
            {
                bool result = false;

                if (Registry.CurrentUser.OpenSubKey(@"\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer")?.GetValue("NoDesktop", 0) is int noDesktopValue)
                    if (noDesktopValue == 1)
                        result = true;

                return result;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}