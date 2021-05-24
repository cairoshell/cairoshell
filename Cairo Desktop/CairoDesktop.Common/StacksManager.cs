using ManagedShell.Common.Common;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;
using ManagedShell.ShellFolders;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace CairoDesktop.Common
{
    public class StacksManager : DependencyObject
    {
        private StackManagerPersistence _stackManagerPersistence;

        private InvokingObservableCollection<StackLocation> _stackLocations = new InvokingObservableCollection<StackLocation>(Application.Current.Dispatcher);

        public InvokingObservableCollection<StackLocation> StackLocations
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
            _stackManagerPersistence = new StackManagerPersistence();
            initialize();
        }

        private void initialize()
        {
            try
            {
                var paths = _stackManagerPersistence.Load();
                foreach (var path in paths)
                {
                    AddLocation(path);
                }
            }
            catch (Exception e)
            {
                ShellLogger.Error($"StacksManager: Unable to deserialize stacks config: {e.Message}");
            }

            StackLocations.CollectionChanged += stackLocations_CollectionChanged;
        }

        private void stackLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _stackManagerPersistence.Save(StackLocations.Select(location => location.Path));
        }

        public bool AddLocation(string path)
        {
            if (Path.GetExtension(path) == ".xml")
            {
                StackLocations.Add(new XmlConfigStackLocation(path));
                return true;
            }
            else if (CanAdd(path))
            {
                ShellFolder dir = new ShellFolder(path, IntPtr.Zero, true);

                if (dir.IsNavigableFolder)
                {
                    StackLocations.Add(new ShellFolderStackLocation(dir));
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

        public void RemoveLocation(StackLocation location)
        {
            StackLocations.Remove(location);
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
    }

    public abstract class StackLocation
    {
        public abstract string Path { get; }
        public abstract string DisplayName { get; }
        public abstract bool IsDesktop { get; }
        public abstract ThreadSafeObservableCollection<ShellFile> Files { get; }
        public abstract bool IsFileSystem { get; }
    }

    public class ShellFolderStackLocation : StackLocation
    {
        private readonly ShellFolder _shellFolder;

        public ShellFolderStackLocation(ShellFolder shellFolder)
        {
            _shellFolder = shellFolder;
        }

        public override string Path { get { return _shellFolder.Path; } }
        public override string DisplayName
        {
            get { return _shellFolder.DisplayName; }
        }
        public override bool IsDesktop { get { return _shellFolder.IsDesktop; } }

        public override ThreadSafeObservableCollection<ShellFile> Files
        {
            get { return _shellFolder.Files; }
        }

        public override bool IsFileSystem
        {
            get { return _shellFolder.IsFileSystem; }
        }
    }

    public class XmlConfigStackLocation : StackLocation
    {
        private readonly XmlSerializer _serializer;
        private readonly string _configFile;
        private readonly string _displayName;
        private readonly ThreadSafeObservableCollection<ShellFile> _files;
        private readonly FileSystemWatcher _watcher;

        public XmlConfigStackLocation(string configFile)
        {
            _serializer = System.Xml.Serialization.XmlSerializer.FromTypes(new[] { typeof(List<string>) })[0];
            _configFile = configFile;
            _displayName = System.IO.Path.GetFileNameWithoutExtension(_configFile);
            _files = new ThreadSafeObservableCollection<ShellFile>();

            LoadConfig();

            _watcher = new FileSystemWatcher();
            _watcher.Path = System.IO.Path.GetDirectoryName(_configFile);
            _watcher.Filter = System.IO.Path.GetFileName(_configFile);
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += _watcher_Changed; ;
        }

        private void LoadConfig()
        {
            _files.Clear();
            var shellFiles = Load();
            foreach (var path in shellFiles)
            {
                ShellFile shellFile = new ShellFile(null, path);

                _files.Add(shellFile);
            }
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadConfig();
        }

        private IEnumerable<string> Load()
        {
            List<string> result = new List<string>();
            if (ShellHelper.Exists(_configFile))
            {
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(_configFile);

                if (_serializer.Deserialize(reader) is List<string> locationPaths)
                {
                    foreach (string path in locationPaths)
                    {
                        result.Add(path);
                    }
                }

                reader.Close();
            }

            return result;
        }

        public override string Path { get { return _configFile; } }

        public override string DisplayName
        {
            get { return _displayName; }
        }
        public override bool IsDesktop { get { return false; } }

        public override ThreadSafeObservableCollection<ShellFile> Files
        {
            get { return _files; }
        }

        public override bool IsFileSystem
        {
            get { return true; }
        }
    }



    internal class StackManagerPersistence
    {
        private readonly string _configFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoStacksConfig.xml";
        private readonly XmlSerializer _serializer;

        public StackManagerPersistence()
        {
            _serializer = System.Xml.Serialization.XmlSerializer.FromTypes(new[] { typeof(List<string>) })[0];
        }

        public void Save(IEnumerable<string> stackLocations)
        {
            List<string> locationPaths = new List<string>();

            foreach (string dir in stackLocations)
            {
                locationPaths.Add(dir);
            }

            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    "
            };

            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(_configFile, settings);
            _serializer.Serialize(writer, locationPaths);

            writer.Close();
        }

        public IEnumerable<string> Load()
        {
            List<string> result = new List<string>();
            if (ShellHelper.Exists(_configFile))
            {
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(_configFile);

                if (_serializer.Deserialize(reader) is List<string> locationPaths)
                {
                    foreach (string path in locationPaths)
                    {
                        result.Add(path);
                    }
                }

                reader.Close();
            }
            else
            {
                // Add some default folders on first run
                result.AddRange(new[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None),
                        KnownFolders.GetPath(KnownFolder.Downloads, NativeMethods.KnownFolderFlags.None)
                    });

                // Save
                Save(result);
            }

            return result;
        }
    }
}