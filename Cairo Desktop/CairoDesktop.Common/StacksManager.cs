using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;

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
}