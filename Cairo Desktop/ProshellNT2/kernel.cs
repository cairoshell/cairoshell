using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalystContainer.CoreModContracts;
using ProshellNT2.TargetingPack;

namespace ProshellNT2
{
    public class NT_CM : ICoreModule
    {
        public string DisplayName
        {
            get
            {
                return "Proshell NT 2";
            }
        }

        public string UniqueID
        {
            get
            {
                return "NT2.2020";
            }
        }

        public void Boot(string id, Form player, Exposure exp)
        {
            MainForm mainForm = new MainForm();
            mainForm.exp = exp;
            mainForm.MdiParent = player;
            mainForm.Show();
        }
    }
    public static class PluginLoader
    {
        public static ICollection<I_NT2Addon> LoadPlugins(string path)
        {
            string[] dllFileNames = null;

            if (Directory.Exists(path))
            {
                dllFileNames = Directory.GetFiles(path, "*.pcx");

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                Type pluginType = typeof(I_NT2Addon);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }
                            else
                            {
                                if (type.GetInterface(pluginType.FullName) != null)
                                {
                                    pluginTypes.Add(type);
                                }
                            }
                        }
                    }
                }

                ICollection<I_NT2Addon> plugins = new List<I_NT2Addon>(pluginTypes.Count);
                foreach (Type type in pluginTypes)
                {
                    I_NT2Addon plugin = (I_NT2Addon)Activator.CreateInstance(type);
                    plugins.Add(plugin);
                }

                return plugins;
            } else
            {
                try
                {
                    Directory.CreateDirectory(path);
                    MessageBox.Show("You can drop in NT Programs (.pcx) in " + path, "Proshell NT");
                } catch
                {
                    MessageBox.Show("Container Failed to Initalize");
                    
                }
            }
            return null;
        }
    }
}
