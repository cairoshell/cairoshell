﻿using CairoDesktop.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace CairoDesktop.SupportingClasses
{
    internal class DynamicDesktopNavigationManager : INotifyPropertyChanged
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
            }
        }

        public DynamicDesktopNavigationManager()
        {
            list = new List<string>();
            currentIndex = 0;
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

        internal void NavigateForward()
        {
            if (CanGoForward)
            {
                currentIndex += 1;
            }
        }

        internal void NavigateBackward()
        {
            if (CanGoBack)
            {
                currentIndex -= 1;
            }
        }

        internal void NavigateToIndex(int index)
        {
            if (index < list.Count)
            {
                currentIndex = index;
            }
        }

        internal void NavigateToParent()
        {
            DirectoryInfo parentDirectoryInfo = Directory.GetParent(CurrentPath);
            if (parentDirectoryInfo != null && parentDirectoryInfo.Exists)
            {
                string parentPath = parentDirectoryInfo.FullName;
                NavigateTo(parentPath);
            }
        }

        internal void NavigateHome()
        {
            string desktopPath = Settings.Instance.DesktopDirectory;

            if (!Directory.Exists(desktopPath))
                desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

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