using CatalystContainer.CoreModContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Catalyst_Containers
{
    public static class Constants
    {
        public static string ApplicationFolder()
        {
            return Path.GetDirectoryName(Application.ExecutablePath);
        }
        public static string CMFolderName()
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + @"\catcontainers\coremodules";
        }
        public static string ContainerPath(string id)
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + @"\catcontainers\containers\" + id;
        }
        public static string ContainerPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + @"\catcontainers\containers";
        }
    }
    public class PluginLoader
    {
        public static Dictionary<string,ICoreModule> Plugins { get; set; }
        public static Dictionary<string, string> FriendlyNames { get; set; }

        public void LoadPlugins()
        {
            Plugins = new Dictionary<string, ICoreModule>();
            FriendlyNames = new Dictionary<string, string>();

            //Load the DLLs from the Plugins directory
            if (Directory.Exists(Constants.CMFolderName()))
            {
                string[] files = Directory.GetFiles(Constants.CMFolderName());
                foreach (string file in files)
                {
                    if (file.EndsWith(".dll"))
                    {
                        Assembly.LoadFile(Path.GetFullPath(file));
                    }
                }
            }

            Type interfaceType = typeof(ICoreModule);
            //Fetch all types that implement the interface IPlugin and are a class
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass)
                .ToArray();
            foreach (Type type in types)
            {

                    var cm = (ICoreModule)Activator.CreateInstance(type);
                    //Create a new instance of all found types
                    Plugins.Add(cm.UniqueID, cm);
                    FriendlyNames.Add("(" + cm.UniqueID + ") - " + cm.DisplayName, cm.UniqueID);

            }
        }
    }
}
