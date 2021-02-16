using CairoDesktop.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ManagedShell.Common.Helpers;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
    public class NavigationManager : INotifyPropertyChanged
    {
        private List<string> list;
        private int _currentIndex;
        private IReadOnlyList<string> readOnlyList;

        private int currentIndex
        {
            get
            {
                return _currentIndex;
            }
            set
            {
                _currentIndex = value;

                OnPropertyChanged("CurrentPath");
                OnPropertyChanged("CanGoBack");
                OnPropertyChanged("CanGoForward");
                OnPropertyChanged("CurrentPathFriendly");
                OnPropertyChanged("HomePathFriendly");
            }
        }

        public NavigationManager()
        {
            list = new List<string>();
            currentIndex = 0;

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;

            NavigateHome();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "DesktopDirectory":
                        OnPropertyChanged("HomePathFriendly");
                        break;
                }
            }
        }

        public string CurrentPath
        {
            get
            {
                if (list.Count > 0)
                    return list[currentIndex];
                else
                    return string.Empty;
            }
        }

        public IReadOnlyList<string> PathHistory
        {
            get
            {
                if (readOnlyList == null)
                    readOnlyList = list.AsReadOnly();

                return readOnlyList;
            }
        }

        public bool CanGoBack
        {
            get
            {
                return list != null &&
                       list.Count > 1 &&
                       currentIndex > 0;
            }
        }

        public bool CanGoForward
        {
            get
            {
                return list != null &&
                       list.Count > 1 &&
                       currentIndex < list.Count - 1;
            }
        }

        public string CurrentPathFriendly
        {
            get
            {
                return Localization.DisplayString.sDesktop_CurrentFolder + " " + CurrentPath;
            }
        }

        public string HomePathFriendly
        {
            get
            {
                return string.Format("{0} ({1})", Localization.DisplayString.sDesktop_Home, Settings.Instance.DesktopDirectory);
            }
        }

        public void Clear()
        {
            string current = CurrentPath;
            list.Clear();
            list.Add(current);
            currentIndex = 0;
        }

        public void NavigateTo(string path)
        {
            if (path != CurrentPath)
            {
                if (CanGoForward)
                {
                    // if we went back, then changed directory, items past the new location are no longer valid
                    int forwardStart = currentIndex + 1;
                    list.RemoveRange(forwardStart, list.Count - forwardStart);
                }

                list.Add(path);
                currentIndex = list.Count - 1;
            }
        }

        public void NavigateForward()
        {
            if (CanGoForward)
            {
                currentIndex += 1;
            }
        }

        public void NavigateBackward()
        {
            if (CanGoBack)
            {
                currentIndex -= 1;
            }
        }

        public void NavigateToIndex(int index)
        {
            if (index < list.Count)
            {
                currentIndex = index;
            }
        }

        public void NavigateToParent()
        {
            string parentPath = string.Empty;
            ShellFolder currentFolder = new ShellFolder(CurrentPath, IntPtr.Zero);
            if (currentFolder.ParentItem != null)
            {
                parentPath = currentFolder.ParentItem.Path;
                currentFolder.ParentItem.Dispose();
            }
            currentFolder.Dispose();
            
            NavigateTo(parentPath);
        }

        public void NavigateHome()
        {
            string defaultDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string desktopPath = Settings.Instance.DesktopDirectory;

            // first run won't have desktop directory set
            if (string.IsNullOrWhiteSpace(desktopPath))
            {
                Settings.Instance.DesktopDirectory = defaultDesktopPath;
                desktopPath = defaultDesktopPath;
            }

            if (!ShellHelper.Exists(desktopPath))
                desktopPath = defaultDesktopPath;

            NavigateTo(desktopPath);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}