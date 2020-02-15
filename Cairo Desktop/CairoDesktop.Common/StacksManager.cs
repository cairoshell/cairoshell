using CairoDesktop.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace CairoDesktop.Common
{
    public class StacksManager
    {
        private static bool isInitialized = false;
        private static string stackConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";
        private static System.Xml.Serialization.XmlSerializer serializer;

        private static InvokingObservableCollection<SystemDirectory> _stackLocations = new InvokingObservableCollection<SystemDirectory>(Application.Current.Dispatcher);

        public static InvokingObservableCollection<SystemDirectory> StackLocations
        {
            get
            {
                if (!isInitialized)
                    initialize();

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

        public StacksManager()
        {
            
        }

        private static void initialize()
        {
            isInitialized = true;

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

        static void stackLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            serializeStacks();
        }

        public static bool AddLocation(string path)
        {
            if (CanAdd(path))
            {
                SystemDirectory dir = new SystemDirectory(path, Dispatcher.CurrentDispatcher);
                StackLocations.Add(dir);

                return true;
            }

            return false;
        }

        public static bool CanAdd(string path)
        {
            return Directory.Exists(path) && !StackLocations.Any(i => i.Equals(path));
        }

        private static void serializeStacks()
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

        private static void deserializeStacks()
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
