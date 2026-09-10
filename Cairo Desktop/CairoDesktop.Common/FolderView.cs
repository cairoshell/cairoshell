using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
    public class FolderView
    {
        public ICollectionView DisplayItems;

        private readonly bool isDesktop;
        private readonly string path;

        private readonly string PublicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory,
            Environment.SpecialFolderOption.DoNotVerify);

        public FolderView(ShellFolder location)
        {
            isDesktop = location.IsDesktop;
            path = location.Path;
            
            DisplayItems = GetCollectionView(location);
        }
        
        private ICollectionView GetCollectionView(ShellFolder location)
        {
            if (location == null)
            {
                return CollectionViewSource.GetDefaultView(new List<ShellItem>());
            }

            ICollectionView cvs = CollectionViewSource.GetDefaultView(location.Files);

            // Use DeferRefresh to batch all view changes and prevent intermediate UI updates
            using (cvs.DeferRefresh())
            {
                cvs.Filter += IconsSource_Filter;

                if (location.IsFileSystem)
                {
                    // Use custom sort for date-based sorting (newest first)
                    if (cvs is ListCollectionView lcv)
                    {
                        lcv.CustomSort = new DateModifiedComparer();
                    }
                }
                else
                {
                    // non-filesystem: fall back to name sorting
                    cvs.SortDescriptions.Clear();
                    cvs.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
                }
            } // DeferRefresh is disposed here, triggering a single refresh with all changes applied

            return cvs;
        }

        private bool IconsSource_Filter(object obj)
        {
            if (obj is ShellFile file)
            {
                if (isDesktop)
                {
                    // the desktop folder includes some links to other folders which are also in the Places menu
                    if (!file.Path.StartsWith(path) && !file.Path.StartsWith(PublicDesktopPath))
                    {
                        return false;
                    }

                    // in newer Windows 11 builds, the Desktop also lists itself
                    if (file.Path == path)
                    {
                        return false;
                    }
                }

                if (file.Path == @"::{21EC2020-3AEA-1069-A2DD-08002B30309D}\::{98F2AB62-0E29-4E4C-8EE7-B542E66740B1}")
                {
                    // this is a mystery item in Control Panel
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Custom comparer that sorts by DateModified property (newest first)
        /// Caches file dates to improve performance
        /// </summary>
        private class DateModifiedComparer : IComparer
        {
            private readonly Dictionary<string, DateTime> _dateCache = new Dictionary<string, DateTime>();

            public int Compare(object x, object y)
            {
                if (x is ShellFile fileX && y is ShellFile fileY)
                {
                    // Get cached dates or fetch from filesystem
                    DateTime dateX = GetDateModifiedCached(fileX);
                    DateTime dateY = GetDateModifiedCached(fileY);

                    // Compare newest first (descending)
                    int result = dateY.CompareTo(dateX);

                    // If dates are equal, sort by name
                    if (result == 0)
                    {
                        return string.Compare(fileX.DisplayName, fileY.DisplayName, StringComparison.CurrentCultureIgnoreCase);
                    }

                    return result;
                }

                // Fallback to name comparison for non-ShellFile items
                if (x is ShellItem itemX && y is ShellItem itemY)
                {
                    return string.Compare(itemX.DisplayName, itemY.DisplayName, StringComparison.CurrentCultureIgnoreCase);
                }

                return 0;
            }

            private DateTime GetDateModifiedCached(ShellFile file)
            {
                // Check cache first
                if (_dateCache.TryGetValue(file.Path, out DateTime cachedDate))
                {
                    return cachedDate;
                }

                // Not in cache, get from filesystem
                DateTime date = GetDateModified(file);
                _dateCache[file.Path] = date;
                return date;
            }

            private DateTime GetDateModified(ShellFile file)
            {
                try
                {
                    // Get the file modification date from the filesystem using the Path property
                    if (!string.IsNullOrEmpty(file.Path) && System.IO.File.Exists(file.Path))
                    {
                        return System.IO.File.GetLastWriteTime(file.Path);
                    }
                    else if (!string.IsNullOrEmpty(file.Path) && System.IO.Directory.Exists(file.Path))
                    {
                        return System.IO.Directory.GetLastWriteTime(file.Path);
                    }
                }
                catch
                {
                    // Ignore errors and return MinValue
                }

                return DateTime.MinValue;
            }
        }
    }
}
