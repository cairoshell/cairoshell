using Microsoft.Win32;
using System.Collections.Generic;

namespace CairoDesktop.UWPInterop
{
    public class StoreAppHelper
    {
        public static List<string[]> GetStoreApps()
        {
            List<string[]> ret = new List<string[]>();

            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Classes", false);

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                // start out by enumerating classes that begin with AppX
                if (subKeyName.StartsWith("AppX"))
                {
                    RegistryKey appInfoKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + subKeyName + "\\Application", false);

                    if (appInfoKey != null && appInfoKey.ValueCount > 0)
                    {
                        if (appInfoKey.GetValue("AppUserModelID") != null)
                        {
                            // if we can get an AppUserModelID, we have something legit
                            RegistryKey packageIdKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + subKeyName + "\\Shell\\open", false);

                            if (packageIdKey != null && packageIdKey.ValueCount > 0)
                            {
                                if (packageIdKey.GetValue("PackageId") != null)
                                {
                                    string id = (string)packageIdKey.GetValue("PackageId");
                                    string path = (string)appInfoKey.GetValue("AppUserModelID");
                                    string[] toAdd = new string[2] { id, path };
                                    bool canAdd = true;
                                    foreach(string[] added in ret)
                                    {
                                        if (added[0] == id)
                                        {
                                            canAdd = false;
                                            break;
                                        }
                                    }

                                    if (canAdd)
                                        ret.Add(toAdd);
                                }
                            }
                        }

                        appInfoKey.Close();
                    }
                }
            }

            key.Close();

            return ret;
        }

        public static string GetAppName(string id)
        {
            Windows.Management.Deployment.PackageManager pman = new Windows.Management.Deployment.PackageManager();
            Windows.ApplicationModel.Package pkg = pman.FindPackageForUser(System.Security.Principal.WindowsIdentity.GetCurrent().User.ToString(), id);

            string name = pkg.Id.Name;

            if (name.IndexOf('.') > 0)
                name = name.Substring(name.IndexOf('.') + 1);

            return name;
        }
    }
}
