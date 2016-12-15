using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using System.Windows.Markup;

namespace CairoDesktop {
    /// <summary>
    /// Interaction logic for StacksContainer.xaml
    /// </summary>
    public partial class StacksContainer : UserControl 
    {
        private static DependencyProperty locationsProperty = DependencyProperty.Register("Locations", typeof(InvokingObservableCollection<SystemDirectory>), typeof(StacksContainer), new PropertyMetadata(new InvokingObservableCollection<SystemDirectory>(Dispatcher.CurrentDispatcher)));
        private string configFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";

        public StacksContainer() 
        {
            InitializeComponent();

            try 
            {
                this.deserialize();
            } 
            catch {}

            Locations.CollectionChanged += new NotifyCollectionChangedEventHandler(locations_CollectionChanged);

            // Add some default folders on FirstRun
            if (Properties.Settings.Default.IsFirstRun == true) 
            {
                // Check for Documents Folder
                String myDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (Directory.Exists(myDocsPath)) {
                    SystemDirectory myDocsSysDir = new SystemDirectory(myDocsPath, Dispatcher.CurrentDispatcher);
                    // Don't duplicate defaults
                    if (!Locations.Contains(myDocsSysDir)) {
                        Locations.Add(myDocsSysDir);
                    }
                }
                // Check for Downloads folder
                String downloadsPath = System.Environment.GetEnvironmentVariable("USERPROFILE") +@"\Downloads";
                if (Directory.Exists(downloadsPath)) {
                    SystemDirectory downloadsSysDir = new SystemDirectory(downloadsPath, Dispatcher.CurrentDispatcher);
                    // Don't duplicate defaults
                    if (!Locations.Contains(downloadsSysDir)) {
                        Locations.Add(downloadsSysDir);
                    }
                }
            }
        }

        public InvokingObservableCollection<SystemDirectory> Locations
        {
            get
            {
                return GetValue(locationsProperty) as InvokingObservableCollection<SystemDirectory>;
            }
            set
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke((Action)(() => this.Locations = value), null);
                    return;
                }

                SetValue(locationsProperty, value);
            }
        }

        void locations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.serialize();
        }
        
        private void serialize() {
            List<String> locationPaths = new List<String>();
            foreach (SystemDirectory dir in Locations) {
                locationPaths.Add(dir.FullName);
            }
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(configFile, settings);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<String>));
            serializer.Serialize(writer, locationPaths);
            writer.Close();
        }
        
        private void deserialize() {
            Locations.Clear();
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<String>));
            System.Xml.XmlReader reader = System.Xml.XmlReader.Create(configFile);
            List<String> locationPaths = serializer.Deserialize(reader) as List<String>;
            foreach (String path in locationPaths) {
                Locations.Add(new SystemDirectory(path, this.Dispatcher));
            }
            reader.Close();
        }

        private void locationDisplay_DragEnter(object sender, DragEventArgs e)
        {
            String[] formats = e.Data.GetFormats(true);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }

            e.Handled = true;
        }

        private void locationDisplay_Drop(object sender, DragEventArgs e)
        {
            String[] fileNames = e.Data.GetData(DataFormats.FileDrop) as String[];
            if (fileNames != null) 
            {
                foreach (String fileName in fileNames)
                {
                    // Only add if the 'file' is a Directory
                    if (System.IO.Directory.Exists(fileName))
                    {
                        Locations.Add(new SystemDirectory(fileName, Dispatcher.CurrentDispatcher));
                    }
                }
            }

            e.Handled = true;
        }

        private void File_Button_Click(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = senderButton.CommandParameter as String;
            try 
            {
                proc.Start();
            } 
            catch 
            {
                // No 'Open' command associated with this filetype in the registry
                Interop.Shell.ShowOpenWithDialog(proc.StartInfo.FileName);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_MenuItem_Click(sender, e);
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_ContextMenu_Loaded(sender, e);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            Locations.Remove((sender as MenuItem).CommandParameter as SystemDirectory);
        }
        
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(sender.GetType().ToString());
            openDir((sender as MenuItem).CommandParameter.ToString());
        }
        
        /// <summary>
        /// Launches the FileManager specified in the application Settings object to the specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory to open.</param>
        private void openDir(String directoryPath) 
        {
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.FileManager);
            prc.StartInfo.Arguments = directoryPath;
            prc.Start();
        }
    }
}
