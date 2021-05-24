using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ManagedShell.Common.Common;
using ManagedShell.Common.Helpers;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
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
}