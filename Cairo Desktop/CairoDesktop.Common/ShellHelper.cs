using Microsoft.Win32;
using System;

namespace CairoDesktop.Common
{
    public class ShellHelper
    {
        public static bool IsCairoUserShell
        {
            get
            {
                // check if we are the current user's shell
                object userShell = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\WinLogon", false).GetValue("Shell");

                if (userShell != null)
                    return userShell.ToString().ToLower().Contains("cairodesktop");
                else
                    return false;
            }
            set
            {
                if (value != IsCairoUserShell)
                {
                    RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\WinLogon", true);

                    if (value)
                    {
                        // set Cairo as the user's shell
                        regKey.SetValue("Shell", AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName);
                    }
                    else
                    {
                        // reset user's shell to system default
                        object userShell = regKey.GetValue("Shell");

                        if (userShell != null)
                            regKey.DeleteValue("Shell");
                    }
                }
            }
        }
    }
}
