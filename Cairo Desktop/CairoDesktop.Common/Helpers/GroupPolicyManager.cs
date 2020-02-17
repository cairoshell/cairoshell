using CairoDesktop.Common.DesignPatterns;
using Microsoft.Win32;

namespace CairoDesktop.Common.Helpers
{
    public sealed class GroupPolicyManager : SingletonObject<GroupPolicyManager>
    {
        private GroupPolicyManager()
        {
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

                var reg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
                object val = reg?.GetValue("NoDesktop", 0);
                if (val is int noDesktopValue)
                    if (noDesktopValue == 1)
                        result = true;

                return result;
            }
        }
    }
}