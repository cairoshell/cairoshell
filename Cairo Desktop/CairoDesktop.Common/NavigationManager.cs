using CairoDesktop.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

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
            if (path != CurrentPath && Directory.Exists(path))
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
            if (CanGoForward && Directory.Exists(list[currentIndex + 1]))
            {
                currentIndex += 1;
            }
        }

        public void NavigateBackward()
        {
            if (CanGoBack && Directory.Exists(list[currentIndex - 1]))
            {
                currentIndex -= 1;
            }
        }

        public void NavigateToIndex(int index)
        {
            if (index < list.Count && Directory.Exists(list[index]))
            {
                currentIndex = index;
            }
        }

        public void NavigateToParent()
        {
            DirectoryInfo parentDirectoryInfo = Directory.GetParent(CurrentPath);
            if (parentDirectoryInfo != null && parentDirectoryInfo.Exists)
            {
                string parentPath = parentDirectoryInfo.FullName;
                NavigateTo(parentPath);
            }
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

            if (!Directory.Exists(desktopPath))
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