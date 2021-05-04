using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ManagedShell.Common.Helpers;
using ManagedShell.WindowsTasks;

namespace CairoDesktop.AppGrabber
{
    public class TaskCategoryProvider : ITaskCategoryProvider
    {
        private readonly AppGrabberService _appGrabber;
        private TaskCategoryChangeDelegate categoryChangeDelegate;

        public TaskCategoryProvider(AppGrabberService appGrabber)
        {
            _appGrabber = appGrabber;
            
            foreach (Category category in _appGrabber.CategoryList)
            {
                category.PropertyChanged += Category_PropertyChanged;
            }
        }

        public void Dispose()
        {
            foreach (Category category in _appGrabber.CategoryList)
            {
                category.PropertyChanged -= Category_PropertyChanged;
            }
        }

        public string GetCategory(ApplicationWindow window, ICollection<ApplicationWindow> applicationWindows)
        {
            var category = GetCategoryDisplayName(window, applicationWindows);

            switch (category)
            {
                case null when window.WinFileName.ToLower().Contains("cairodesktop.exe"):
                    category = "Cairo";
                    break;
                case null when window.WinFileName.ToLower().Contains("\\windows\\") && !window.IsUWP:
                    category = "Windows";
                    break;
                case null:
                    category = Localization.DisplayString.sAppGrabber_Uncategorized;
                    break;
            }

            return category;
        }

        public void SetCategoryChangeDelegate(TaskCategoryChangeDelegate changeDelegate)
        {
            categoryChangeDelegate = changeDelegate;
            _appGrabber.CategoryList.CategoryChanged += (sender, args) => categoryChangeDelegate();
            _appGrabber.CategoryList.CollectionChanged += CategoryList_CollectionChanged;
        }

        private void CategoryList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Category item in e.NewItems)
                {
                    item.PropertyChanged += Category_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Category item in e.OldItems)
                {
                    item.PropertyChanged -= Category_PropertyChanged;
                }
            }
        }

        private string GetCategoryDisplayName(ApplicationWindow window,
            IEnumerable<ApplicationWindow> applicationWindows)
        {
            var category = _appGrabber.CategoryList.FlatList
                .FirstOrDefault(ai => !string.IsNullOrEmpty(ai.Target) &&
                                      (!window.IsUWP && string.Equals(ai.Target, window.WinFileName, StringComparison.CurrentCultureIgnoreCase)
                                       || window.IsUWP && ai.Target == window.AppUserModelID) && ai.Category != null)
                ?.Category.DisplayName;
            if (category != null) return category;

            category = applicationWindows
                .FirstOrDefault(ai => ai.ProcId == window.ProcId && ai.Category != null)
                ?.Category;
            if (category != null) return category;

            var windowFileExists = File.Exists(window.WinFileName);
            category = _appGrabber.CategoryList.FlatList
                .FirstOrDefault(ai =>
                {
                    if (string.IsNullOrEmpty(ai.Target) || window.IsUWP) return false;
                    if (!File.Exists(ai.Target) || !windowFileExists) return false;
                    return ShellHelper.IsSameFile(ai.Target, window.WinFileName);
                })?.Category.DisplayName;
            if (category != null) return category;

            category = _appGrabber.CategoryList.FlatList
                .FirstOrDefault(ai => window.Title.ToLower().Contains(ai.Name.ToLower()) && ai.Category != null)
                ?.Category.DisplayName;
            return category;
        }

        private void Category_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == "DisplayName")
            {
                categoryChangeDelegate();
            }
        }
    }
}
