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
                cvs.SortDescriptions.Add(new SortDescription("IsFolder", ListSortDirection.Descending));
            }
            cvs.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

            cvs.Filter += IconsSource_Filter;

            if (cvs is ICollectionViewLiveShaping cvls)
            {
                cvls.IsLiveSorting = true;
                if (location.IsFileSystem)
                {
                    cvls.LiveSortingProperties.Add("IsFolder");
                }
                cvls.LiveSortingProperties.Add("DisplayName");
            }

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
