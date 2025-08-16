using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Common
{
    public class FolderView
    {
        public ICollectionView DisplayItems;

        public enum SortMode
        {
            NameAsc,
            DateDesc,
            DateAsc
        }

        public SortMode CurrentSort { get; private set; } = SortMode.DateDesc;

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

            if (location.IsFileSystem)
            {
                // default to newest first for filesystem folders
                cvs.SortDescriptions.Add(new SortDescription("DateModified", ListSortDirection.Descending));
                cvs.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            }
            else
            {
                // non-filesystem fall back to name
                cvs.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            }

            // expose DateModified live sorting when available
            if (cvs is ICollectionViewLiveShaping cvls)
            {
                if (location.IsFileSystem)
                {
                    cvls.LiveSortingProperties.Add("DateModified");
                }
                cvls.LiveSortingProperties.Add("DisplayName");
            }

            cvs.Filter += IconsSource_Filter;

            return cvs;
        }

        /// <summary>
        /// Change the current sort mode for the view.
        /// </summary>
        public void ApplySort(SortMode mode)
        {
            if (DisplayItems == null)
            {
                CurrentSort = mode;
                return;
            }

            DisplayItems.SortDescriptions.Clear();

            switch (mode)
            {
                case SortMode.NameAsc:
                    DisplayItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
                    break;
                case SortMode.DateDesc:
                    // DateModified may not exist for non-filesystem items; fallback to name
                    DisplayItems.SortDescriptions.Add(new SortDescription("DateModified", ListSortDirection.Descending));
                    DisplayItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
                    break;
                case SortMode.DateAsc:
                    DisplayItems.SortDescriptions.Add(new SortDescription("DateModified", ListSortDirection.Ascending));
                    DisplayItems.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
                    break;
            }

            if (DisplayItems is ICollectionViewLiveShaping cvls)
            {
                cvls.IsLiveSorting = true;
            }

            CurrentSort = mode;
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
    }
}
