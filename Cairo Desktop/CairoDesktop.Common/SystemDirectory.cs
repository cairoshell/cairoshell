using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using Microsoft.VisualBasic.FileIO;

namespace CairoDesktop.Common {

    /// <summary>
    /// A wrapper for System.IO.DirectoryInfo which exposes File and Directory list calls as properties
    /// </summary>
    public class SystemDirectory : IEquatable<SystemDirectory>, INotifyPropertyChanged, IDisposable {

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
                    name = Shell.GetDisplayName(FullName);
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

        private FileSystemEventHandler createdHandler;
        private FileSystemEventHandler deletedHandler;
        private RenamedEventHandler renamedHandler;

        /// <summary>
        /// Creates a new SystemDirectory object for the given directory path.
        /// </summary>
        public SystemDirectory(string pathToDirectory, Dispatcher dispatcher)
        {
            try
            {
                this.dispatcher = dispatcher;
                files = new InvokingObservableCollection<SystemFile>(this.dispatcher);
                DirectoryInfo = new DirectoryInfo(pathToDirectory);
                fileWatcher.IncludeSubdirectories = false;
                fileWatcher.Filter = "";
                fileWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;

                createdHandler = new FileSystemEventHandler(fileWatcher_Created);
                deletedHandler = new FileSystemEventHandler(fileWatcher_Deleted);
                renamedHandler = new RenamedEventHandler(fileWatcher_Renamed);

                fileWatcher.Created += createdHandler;
                fileWatcher.Deleted += deletedHandler;
                fileWatcher.Renamed += renamedHandler;
                fileWatcher.EnableRaisingEvents = true;
            }
            catch (UnauthorizedAccessException)
            {
                CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void fileWatcher_Renamed(object sender, RenamedEventArgs e) {
            try
            {
                changeFile(e.OldFullPath, e.FullPath);
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Error("Error in fileWatcher_Renamed.", ex);
            }
        }

        void fileWatcher_Deleted(object sender, FileSystemEventArgs e) {
            try
            {
                removeFile(e.FullPath);
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Error("Error in fileWatcher_Deleted.", ex);
            }
        }

        void fileWatcher_Created(object sender, FileSystemEventArgs e) {
            addFile(e.FullPath);
        }

        private void addFile(string filePath) {
            if (Shell.Exists(filePath) && isFileVisible(filePath))
            {
                SystemFile newFile = new SystemFile(filePath);
                if (newFile.Name != null)
                {
                    files.Add(newFile);
                    OnPropertyChanged("Files");
                }
            }
        }

        private void removeFile(string filePath) {
            int removalIndex = -1;
            foreach (SystemFile file in files) {
                if (file.FullName == filePath) {
                    removalIndex = files.IndexOf(file);
                    break;
                }
            }

            if (removalIndex > -1)
            {
                files.RemoveAt(removalIndex);
                OnPropertyChanged("Files");
            }
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
            if (Shell.Exists(fileName))
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
            
            if (Settings.EnableSubDirs)
            {
                IEnumerable<string> dirs = Directory.EnumerateDirectories(DirectoryInfo.FullName);
                foreach (string subDir in dirs)
                {
                    if (isFileVisible(subDir))
                    {
                        files.Add(new SystemFile(subDir));
                    }
                }
            }

            IEnumerable<string> dirFiles = Directory.EnumerateFiles(DirectoryInfo.FullName, "*");
            foreach (String file in dirFiles)
            {
                if (isFileVisible(file))
                {
                    files.Add(new SystemFile(file));
                }
            }

            OnPropertyChanged("Files");
        }

        public void Dispose()
        {
            fileWatcher.Created -= createdHandler;
            fileWatcher.Deleted -= deletedHandler;
            fileWatcher.Renamed -= renamedHandler;
        }

        public override bool Equals(object other) {
            if (other is string)
            {
                return FullName.Equals(other as string, StringComparison.OrdinalIgnoreCase);
            }
            if (!(other is SystemDirectory)) return false;
            return FullName.Equals((other as SystemDirectory).FullName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }



        public void PasteFromClipboard()
        {
            IDataObject clipFiles = Clipboard.GetDataObject();
            if (clipFiles.GetDataPresent(DataFormats.FileDrop))
            {
                if (clipFiles.GetData(DataFormats.FileDrop) is string[] files)
                {
                    CopyInto(files);
                }
            }
        }

        public void CopyInto(string[] files)
        {
            CopyInto(files, FullName);
        }

        public void MoveInto(string[] files)
        {
            MoveInto(files, FullName);
        }

        public static void CopyInto(string[] files, string directory)
        {
            foreach (string file in files)
            {
                if (Shell.Exists(file))
                {
                    try
                    {
                        FileAttributes attr = File.GetAttributes(file);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            string futureName = directory + "\\" + new DirectoryInfo(file).Name;
                            if (!(futureName == file)) FileSystem.CopyDirectory(file, futureName, UIOption.AllDialogs);
                        }
                        else
                        {
                            string futureName = directory + "\\" + Path.GetFileName(file);
                            if (!(futureName == file)) FileSystem.CopyFile(file, futureName, UIOption.AllDialogs);
                        }
                    }
                    catch { }
                }
            }
        }

        public static void MoveInto(string[] files, string directory)
        {
            foreach (string file in files)
            {
                if (Shell.Exists(file))
                {
                    try
                    {
                        FileAttributes attr = File.GetAttributes(file);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            string futureName = directory + "\\" + new DirectoryInfo(file).Name;
                            if (!(futureName == file)) FileSystem.MoveDirectory(file, futureName, UIOption.AllDialogs);
                        }
                        else
                        {
                            string futureName = directory + "\\" + Path.GetFileName(file);
                            if (!(futureName == file)) FileSystem.MoveFile(file, futureName, UIOption.AllDialogs);
                        }
                    }
                    catch { }
                }
            }
        }

        #region IEquatable<T> Members

        bool IEquatable<SystemDirectory>.Equals(SystemDirectory other) {
            return FullName.Equals((other as SystemDirectory).FullName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// This Event is raised whenever a property of this object has changed. Necesary to sync state when binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [DebuggerNonUserCode]
        private void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion
    }
}
