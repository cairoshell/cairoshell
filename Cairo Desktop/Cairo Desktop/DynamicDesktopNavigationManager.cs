using System;
using System.Collections.Generic;

namespace CairoDesktop
{
    internal class DynamicDesktopNavigationManager
    {
        private List<string> list;
        private int currentIndex;
        private IReadOnlyList<string> readOnlyList;

        internal Action<string> Navigating;

        public DynamicDesktopNavigationManager()
        {
            list = new List<string>();
            currentIndex = 0;
        }

        public void Clear()
        {
            list.Clear();
            currentIndex = 0;
        }

        public string CurrentItem
        {
            get { return list[currentIndex - 1]; }
        }

        public IReadOnlyList<string> ReadOnlyHistory
        {
            get
            {
                if (readOnlyList == null)
                    readOnlyList = list.AsReadOnly();

                return readOnlyList;
            }
        }

        public bool CanGoBack()
        {
            return list != null &&
                   list.Count > 1 &&
                   currentIndex > 1;
        }

        public bool CanGoForward()
        {
            return list != null &&
                   list.Count > 0 &&
                   currentIndex < list.Count;
        }

        private void NavigateToCurrentLocation()
        {
            int ind = (currentIndex == 0) ? 0 : currentIndex - 1;
            Navigating?.Invoke(list[ind]);
        }
        
        public void NavigateTo(string path)
        {
            if (currentIndex < list.Count)
            {
                list.RemoveRange(currentIndex, list.Count - currentIndex);
            }

            list.Add(path);
            currentIndex = list.Count;

            NavigateToCurrentLocation();
        }

        internal void NavigateForward()
        {
            if (CanGoForward())
            {
                currentIndex += 1;
                NavigateToCurrentLocation();
            }
        }

        internal void NavigateBackward()
        {
            if (CanGoBack())
            {
                currentIndex -= 1;
                NavigateToCurrentLocation();
            }
        }
    }
}