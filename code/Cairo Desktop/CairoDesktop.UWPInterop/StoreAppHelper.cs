using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

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

        // returns [name,icon,color]
        public static string[] GetAppInfo(string id)
        {
            string returnName = "";
            string returnIcon = "";
            string returnColor = "#000000";
            Windows.Management.Deployment.PackageManager pman = new Windows.Management.Deployment.PackageManager();
            Windows.ApplicationModel.Package pkg = pman.FindPackageForUser(System.Security.Principal.WindowsIdentity.GetCurrent().User.ToString(), id);
            
            string path = pkg.InstalledLocation.Path;

            XmlDocument manifest = new XmlDocument();
            manifest.Load(path + "\\AppxManifest.xml");
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(manifest.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            xmlnsManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");

            foreach (XmlNode app in manifest.SelectNodes("/ns:Package/ns:Applications/ns:Application", xmlnsManager))
            {
                XmlNode showEntry = app.SelectSingleNode("uap:VisualElements/@AppListEntry", xmlnsManager);
                if (showEntry == null || showEntry.Value == "true")
                {
                    Uri nameUri;
                    string nameKey = app.SelectSingleNode("uap:VisualElements/@DisplayName", xmlnsManager).Value;

                    if (!Uri.TryCreate(nameKey, UriKind.Absolute, out nameUri))
                        returnName = nameKey;
                    else
                    {
                        var resourceKey = string.Format("ms-resource://{0}/resources/{1}", pkg.Id.Name, nameUri.Segments.Last());
                        string name = ExtractStringFromPRIFile(path + "\\resources.pri", resourceKey);
                        if (!string.IsNullOrEmpty(name))
                            returnName = name;
                        else
                        {
                            resourceKey = string.Format("ms-resource://{0}/{1}", pkg.Id.Name, nameUri.Segments.Last());
                            returnName = ExtractStringFromPRIFile(path + "\\resources.pri", resourceKey);
                        }
                    }

                    XmlNode colorKey = app.SelectSingleNode("uap:VisualElements/@BackgroundColor", xmlnsManager);

                    if (colorKey != null && !string.IsNullOrEmpty(colorKey.Value) && colorKey.Value.ToLower() != "transparent")
                        returnColor = colorKey.Value;

                    string iconPath = path + "\\" + (app.SelectSingleNode("uap:VisualElements/@Square44x44Logo", xmlnsManager).Value).Replace(".png", "");

                    // ugh....
                    if (File.Exists(iconPath + ".targetsize-32.png"))
                        returnIcon = iconPath + ".targetsize-32.png";
                    else if (File.Exists(iconPath + ".targetsize-16.png"))
                        returnIcon = iconPath + ".targetsize-16.png";
                    else if (File.Exists(iconPath + ".targetsize-24.png"))
                        returnIcon = iconPath + ".targetsize-24.png";
                    else if (File.Exists(iconPath + ".scale-100.png"))
                        returnIcon = iconPath + ".scale-100.png";
                    else if (File.Exists(iconPath + ".scale-200.png"))
                        returnIcon = iconPath + ".scale-200.png";
                    else if (File.Exists(iconPath + ".targetsize-256.png"))
                        returnIcon = iconPath + ".targetsize-256.png";
                    else if (File.Exists(iconPath + ".png"))
                        returnIcon = iconPath + ".png";
                }
            }

            return new string[] { returnName, returnIcon, returnColor };
        }

        [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        static internal string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
        {
            string sWin8ManifestString = string.Format("@{{{0}? {1}}}", pathToPRI, resourceKey);
            var outBuff = new StringBuilder(1024);
            int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
            return outBuff.ToString();
        }
    }
}
