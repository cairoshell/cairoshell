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
            return Application.ExecutablePath;
        }
        public static string CMFolderName()
        {
            return Application.ExecutablePath + @"\catcontainers\coremodules";
        }
        public static string ContainerPath(string id)
        {
            return Application.ExecutablePath + @"\catcontainers\containers\" + id;
        }
    }
    public class PluginLoader
    {
        public static List<ICoreModule> Plugins { get; set; }

        public void LoadPlugins()
        {
            Plugins = new List<ICoreModule>();

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
                //Create a new instance of all found types
                Plugins.Add((ICoreModule)Activator.CreateInstance(type));
            }
        }
    }
}
