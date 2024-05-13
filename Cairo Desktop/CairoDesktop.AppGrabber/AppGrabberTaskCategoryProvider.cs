using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CairoDesktop.Common.Localization;
using ManagedShell;
using ManagedShell.Common.Helpers;
using ManagedShell.WindowsTasks;

namespace CairoDesktop.AppGrabber
{
    public class AppGrabberTaskCategoryProvider : ITaskCategoryProvider
    {
        private readonly IAppGrabber _appGrabber;
        private readonly ShellManager _shellManager;
        private TaskCategoryChangeDelegate categoryChangeDelegate;

        public AppGrabberTaskCategoryProvider(IAppGrabber appGrabber, ShellManager shellManager)
        {
            _appGrabber = appGrabber;
            _shellManager = shellManager;

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

        public string GetCategory(ApplicationWindow window)
        {
            var category = GetCategoryDisplayName(window);

            switch (category)
            {
                case null when window.WinFileName.ToLower().Contains("cairodesktop.exe"):
                    category = "Cairo";
                    break;
                case null when window.WinFileName.ToLower().Contains("\\windows\\") && !window.IsUWP:
                    category = "Windows";
                    break;
                case null:
                    category = DisplayString.sAppGrabber_Uncategorized;
                    break;
            }

            return category;
        }

        public void SetCategoryChangeDelegate(TaskCategoryChangeDelegate changeDelegate)
        {
            if (changeDelegate == null)
            {
                return;
            }

            categoryChangeDelegate = changeDelegate;
            _appGrabber.CategoryList.CategoryChanged += (sender, args) => categoryChangeDelegate();
            _appGrabber.CategoryList.CollectionChanged += CategoryList_CollectionChanged;

            // request new categories in case of preference change
            // nullify all existing categories so we don't attempt reuse
            foreach (ApplicationWindow window in _shellManager.Tasks.GroupedWindows)
            {
                window.Category = null;
            }
            categoryChangeDelegate?.Invoke();
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

        private string GetCategoryDisplayName(ApplicationWindow window)
        {
            return window.IsUWP ?
                GetCategoryDisplayNameOfUWPApp(window) :
                GetCategoryDisplayNameOfWin32App(window);
        }

        private string GetCategoryDisplayNameOfUWPApp(ApplicationWindow window)
        {
            if (TryGetCategoryDisplayNameByAppUserModelId(window, out var category)) return category;
            if (TryGetCategoryDisplayNameByProcId(window, out category)) return category;
            if (TryGetCategoryDisplayNameByTitle(window, out category)) return category;
            return null;
        }

        private string GetCategoryDisplayNameOfWin32App(ApplicationWindow window)
        {
            if (TryGetCategoryDisplayNameByWinFileName(window, out var category)) return category;
            if (TryGetCategoryDisplayNameByProcId(window, out category)) return category;
            TryGetCategoryDisplayNameByFileEquality(window);
            if (TryGetCategoryDisplayNameByTitle(window, out category)) return category;
            return null;
        }

        private bool TryGetCategoryDisplayNameByAppUserModelId(ApplicationWindow window, out string category)
        {
            category = _appGrabber.CategoryList.FlatList
                .FirstOrDefault(ai =>
                    !string.IsNullOrEmpty(ai.Target) && ai.Target == window.AppUserModelID && ai.Category != null)
                ?.Category.DisplayName;
            return category != null;
        }

        private bool TryGetCategoryDisplayNameByWinFileName(ApplicationWindow window, out string category)
        {
            category = _appGrabber.CategoryList.FlatList
                .FirstOrDefault(ai =>
                    !string.IsNullOrEmpty(ai.Target)
                    && string.Equals(ai.Target, window.WinFileName, StringComparison.CurrentCultureIgnoreCase))
                ?.Category.DisplayName;
            return category != null;
        }

        private bool TryGetCategoryDisplayNameByProcId(ApplicationWindow window, out string category)
        {
            var applicationWindows = _shellManager.Tasks.GroupedWindows.Cast<ApplicationWindow>();
            category = applicationWindows
                .FirstOrDefault(ai => ai.ProcId == window.ProcId && ai.Category != null)
                ?.Category;
            return category != null;
        }

        private void TryGetCategoryDisplayNameByFileEquality(ApplicationWindow window)
        {
            Task.Run(() =>
            {
                if (!File.Exists(window.WinFileName)) return;
                string category = _appGrabber.CategoryList.FlatList
                    .FirstOrDefault(ai =>
                    {
                        if (string.IsNullOrEmpty(ai.Target)) return false;
                        return File.Exists(ai.Target) && ShellHelper.IsSameFile(ai.Target, window.WinFileName);
                    })?.Category.DisplayName;

                if (!string.IsNullOrEmpty(category))
                {
                    window.Category = category;
                }
            });
        }

        private bool TryGetCategoryDisplayNameByTitle(ApplicationWindow window, out string category)
        {
            category = _appGrabber.CategoryList.FlatList
                .FirstOrDefault(ai => window.Title.ToLower().Contains(ai.Name.ToLower()) && ai.Category != null)
                ?.Category.DisplayName;
            return category != null;
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
