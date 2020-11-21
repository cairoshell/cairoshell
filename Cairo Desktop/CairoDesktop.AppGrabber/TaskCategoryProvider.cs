using System.ComponentModel;
using CairoDesktop.WindowsTasks;

namespace CairoDesktop.AppGrabber
{
    public class TaskCategoryProvider : ITaskCategoryProvider
    {
        private TaskCategoryChangeDelegate categoryChangeDelegate;

        public TaskCategoryProvider()
        {
            foreach (Category category in AppGrabber.Instance.CategoryList)
            {
                category.PropertyChanged += Category_PropertyChanged;
            }
        }

        public void Dispose()
        {
            foreach (Category category in AppGrabber.Instance.CategoryList)
            {
                category.PropertyChanged -= Category_PropertyChanged;
            }
        }

        public string GetCategory(ApplicationWindow window)
        {
            ApplicationInfo applicationInfo = GetTaskApplicationInfo(window);

            if (applicationInfo != null && applicationInfo.Category == null)
            {
                // if app was removed, category is null, so stop using that app
                applicationInfo = null;
            }
            string category = applicationInfo?.Category?.DisplayName;

            if (category == null && window.WinFileName.ToLower().Contains("cairodesktop.exe"))
                category = "Cairo";
            else if (category == null && window.WinFileName.ToLower().Contains("\\windows\\") && !window.IsUWP)
                category = "Windows";
            else if (category == null)
                category = Localization.DisplayString.sAppGrabber_Uncategorized;

            return category;
        }

        public void SetCategoryChangeDelegate(TaskCategoryChangeDelegate changeDelegate)
        {
            categoryChangeDelegate = changeDelegate;
            AppGrabber.Instance.CategoryList.CategoryChanged += (sender, args) => categoryChangeDelegate();
            AppGrabber.Instance.CategoryList.CollectionChanged += CategoryList_CollectionChanged;
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

        private ApplicationInfo GetTaskApplicationInfo(ApplicationWindow window)
        {
            ApplicationInfo appInfo = null;
            foreach (ApplicationInfo ai in AppGrabber.Instance.CategoryList.FlatList)
            {
                if ((ai.Target.ToLower() == window.WinFileName.ToLower() || (window.IsUWP && ai.Target == window.AppUserModelID)) && ai.Category != null)
                {
                    appInfo = ai;
                    break;
                }
                else if (window.Title.ToLower().Contains(ai.Name.ToLower()) && ai.Category != null)
                {
                    appInfo = ai;
                }
            }

            return appInfo;
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
