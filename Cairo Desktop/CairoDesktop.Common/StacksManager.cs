using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace CairoDesktop.Common
{
    public class StacksManager : DependencyObject
    {
        private string stackConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";
        private System.Xml.Serialization.XmlSerializer serializer;

        private InvokingObservableCollection<SystemDirectory> _stackLocations = new InvokingObservableCollection<SystemDirectory>(Application.Current.Dispatcher);

        public InvokingObservableCollection<SystemDirectory> StackLocations
        {
            get
            {
                return _stackLocations;
            }
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

        private static StacksManager _instance = new StacksManager();
        public static StacksManager Instance
        {
            get { return _instance; }
        }

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
            catch { }

            StackLocations.CollectionChanged += new NotifyCollectionChangedEventHandler(stackLocations_CollectionChanged);
        }

        private void stackLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            serializeStacks();
        }

        public bool AddLocation(string path)
        {
            if (CanAdd(path))
            {
                SystemDirectory dir = new SystemDirectory(path, Dispatcher.CurrentDispatcher);
                StackLocations.Add(dir);

                return true;
            }

            return false;
        }

        public bool CanAdd(string path)
        {
            return Directory.Exists(path) && !StackLocations.Any(i => i.Equals(path));
        }

        public void RemoveLocation(SystemDirectory directory)
        {
            directory.Dispose();
            StackLocations.Remove(directory);
        }

        private void serializeStacks()
        {
            List<string> locationPaths = new List<string>();
            foreach (SystemDirectory dir in StackLocations)
            {
                locationPaths.Add(dir.FullName);
            }
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stackConfigFile, settings);
            serializer.Serialize(writer, locationPaths);
            writer.Close();
        }

        private void deserializeStacks()
        {
            if (Interop.Shell.Exists(stackConfigFile))
            {
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stackConfigFile);
                List<string> locationPaths = serializer.Deserialize(reader) as List<string>;
                foreach (string path in locationPaths)
                {
                    AddLocation(path);
                }
                reader.Close();
            }
            else
            {
                // Add some default folders on FirstRun

                // Check for Documents Folder
                string myDocsPath = Interop.KnownFolders.GetPath(Interop.KnownFolder.Documents);
                if (Directory.Exists(myDocsPath))
                {
                    SystemDirectory myDocsSysDir = new SystemDirectory(myDocsPath, Dispatcher.CurrentDispatcher);
                    // Don't duplicate defaults
                    if (!StackLocations.Contains(myDocsSysDir))
                    {
                        StackLocations.Add(myDocsSysDir);
                    }
                }
                // Check for Downloads folder
                string downloadsPath = Interop.KnownFolders.GetPath(Interop.KnownFolder.Downloads);
                if (Directory.Exists(downloadsPath))
                {
                    SystemDirectory downloadsSysDir = new SystemDirectory(downloadsPath, Dispatcher.CurrentDispatcher);
                    // Don't duplicate defaults
                    if (!StackLocations.Contains(downloadsSysDir))
                    {
                        StackLocations.Add(downloadsSysDir);
                    }
                }

                // save
                serializeStacks();
            }
        }
    }
}
