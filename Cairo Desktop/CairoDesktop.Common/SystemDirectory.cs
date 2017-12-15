using System;
using System.IO;
using System.Windows.Threading;
using CairoDesktop.Configuration;

namespace CairoDesktop.Common {

    /// <summary>
    /// A wrapper for System.IO.DirectoryInfo which exposes File and Directory list calls as properties
    /// </summary>
    public class SystemDirectory : IEquatable<SystemDirectory> {

        private Dispatcher dispatcher;

        FileSystemWatcher fileWatcher = new FileSystemWatcher();
        private DirectoryInfo dir;
        /// <summary>
        /// Gets or sets the DirectoryInfo object being wrapped.
        /// </summary>
        public DirectoryInfo DirectoryInfo {
            get { return dir; }
            set {
                dir = value;
                fileWatcher.Path = dir.FullName;
                initialize();
            }
        }

        private InvokingObservableCollection<SystemFile> files;
        /// <summary>
        /// Gets an ObservableCollection of files contained in this folder as FileSystemInfo objects.
        /// </summary>
        public InvokingObservableCollection<SystemFile> Files {
            get { return files; }
        }

        private string name;

        public string Name {
            get
            {
                if (name == null)
                    name = Interop.Shell.GetDisplayName(FullName);
                return name;
            }
        }

        public string NameLabel
        {
            get { return Name.Replace("_", "__"); }
        }

        public string FullName {
            get { return dir.FullName; }
        }

        /// <summary>
        /// Creates a new SystemDirectory object for the given directory path.
        /// </summary>
        public SystemDirectory(string pathToDirectory, Dispatcher dispatcher) {
            this.dispatcher = dispatcher;
            files = new InvokingObservableCollection<SystemFile>(this.dispatcher);
            this.DirectoryInfo = new DirectoryInfo(pathToDirectory);
            fileWatcher.IncludeSubdirectories = false;
            fileWatcher.Filter = "";
            fileWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
            fileWatcher.Created += new FileSystemEventHandler(fileWatcher_Created);
            fileWatcher.Deleted += new FileSystemEventHandler(fileWatcher_Deleted);
            fileWatcher.Renamed += new RenamedEventHandler(fileWatcher_Renamed);
            fileWatcher.EnableRaisingEvents = true;
        }

        void fileWatcher_Renamed(object sender, RenamedEventArgs e) {
            try
            {
                this.changeFile(e.OldFullPath, e.FullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void fileWatcher_Deleted(object sender, FileSystemEventArgs e) {
            try
            {
                this.removeFile(e.FullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void fileWatcher_Created(object sender, FileSystemEventArgs e) {
            this.addFile(e.FullPath);
        }

        private void addFile(String filePath) {
            if (Interop.Shell.Exists(filePath) && isFileVisible(filePath))
            {
                SystemFile newFile = new SystemFile(filePath, dispatcher);
                if (newFile.Name != null)
                    files.Add(newFile);
            }
        }

        private void removeFile(String filePath) {
            int removalIndex = -1;
            foreach (SystemFile file in files) {
                if (file.FullName == filePath) {
                    removalIndex = files.IndexOf(file);
                    break;
                }
            }

            if (removalIndex > -1)
                files.RemoveAt(removalIndex);
        }

        private void changeFile(string oldPath, string newPath)
        {
            bool newExists = false;
            foreach (SystemFile file in files)
            {
                if (file.FullName == newPath)
                {
                    newExists = true;
                    break;
                }
            }

            if (!newExists)
            {
                foreach (SystemFile file in files)
                {
                    if (file.FullName == oldPath)
                    {
                        file.SetFilePath(newPath);
                        break;
                    }
                }
            }
            else
                removeFile(oldPath);
        }

        private bool isFileVisible(string fileName)
        {
            if (Interop.Shell.Exists(fileName))
            {
                try
                {
                    FileAttributes attributes = File.GetAttributes(fileName);
                    return (((attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && ((attributes & FileAttributes.System) != FileAttributes.System) && ((attributes & FileAttributes.Temporary) != FileAttributes.Temporary));
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private void initialize()
        {
            files.Clear();
            bool showSubs = false;
            if (Settings.EnableSubDirs)
                showSubs = true;

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                if (showSubs && isFileVisible(subDir.FullName))
                {
                    files.Add(new SystemFile(subDir.FullName, dispatcher));
                }
            }

            foreach (String file in Directory.GetFiles(this.DirectoryInfo.FullName))
            {
                if (isFileVisible(file))
                {
                    files.Add(new SystemFile(file, dispatcher));
                }
            }

        }

        public override bool Equals(object other) {
            if (!(other is SystemDirectory)) return false;
            return this.FullName.Equals((other as SystemDirectory).FullName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.FullName.GetHashCode();
        }

        #region IEquatable<T> Members

        bool IEquatable<SystemDirectory>.Equals(SystemDirectory other) {
            return this.FullName.Equals((other as SystemDirectory).FullName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
