using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
    public class StacksManager : DependencyObject
    {
        private readonly string stackConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";
        private System.Xml.Serialization.XmlSerializer serializer;

        private InvokingObservableCollection<ShellFolder> _stackLocations = new InvokingObservableCollection<ShellFolder>(Application.Current.Dispatcher);

        public InvokingObservableCollection<ShellFolder> StackLocations
        {
            get => _stackLocations;
            set
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke((Action)(() => StackLocations = value), null);
                    return;
                }

                _stackLocations = value;
            }
        }

        private static readonly StacksManager _instance = new StacksManager();
        public static StacksManager Instance => _instance;

        public StacksManager()
        {
            initialize();
        }

        private void initialize()
        {
            // this causes an exception, thanks MS!
            //serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<String>));

            serializer = System.Xml.Serialization.XmlSerializer.FromTypes(new[] { typeof(List<string>) })[0];

            try
            {
                deserializeStacks();
            }
            catch (Exception e)
            {
                ShellLogger.Error($"StacksManager: Unable to deserialize stacks config: {e.Message}");
            }

            StackLocations.CollectionChanged += stackLocations_CollectionChanged;
        }

        private void stackLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            serializeStacks();
        }

        public bool AddLocation(string path)
        {
            if (CanAdd(path))
            {
                ShellFolder dir = new ShellFolder(path, IntPtr.Zero, true);

                if (dir.IsNavigableFolder)
                {
                    StackLocations.Add(dir);
                    return true;
                }

                dir.Dispose();
            }

            return false;
        }

        public bool CanAdd(string path)
        {
            return !string.IsNullOrEmpty(path) && StackLocations.All(i => i.Path != path);
        }

        public void RemoveLocation(ShellFolder directory)
        {
            directory.Dispose();
            StackLocations.Remove(directory);
        }

        public void RemoveLocation(string path)
        {
            for (int i = 0; i < StackLocations.Count; i++)
            {
                if (StackLocations[i].Path == path)
                {
                    StackLocations.RemoveAt(i);
                    break;
                }
            }
        }

        private void serializeStacks()
        {
            List<string> locationPaths = new List<string>();
            foreach (ShellFolder dir in StackLocations)
            {
                locationPaths.Add(dir.Path);
            }
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    "
            };
            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stackConfigFile, settings);
            serializer.Serialize(writer, locationPaths);
            writer.Close();
        }

        private void deserializeStacks()
        {
            if (ShellHelper.Exists(stackConfigFile))
            {
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stackConfigFile);

                if (serializer.Deserialize(reader) is List<string> locationPaths)
                {
                    foreach (string path in locationPaths)
                    {
                        AddLocation(path);
                    }
                }

                reader.Close();
            }
            else
            {
                // Add some default folders on first run
                AddLocation(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None));
                AddLocation(KnownFolders.GetPath(KnownFolder.Downloads, NativeMethods.KnownFolderFlags.None));

                // Save
                serializeStacks();
            }
        }
    }
}
