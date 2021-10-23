using ManagedShell.WindowsTasks;

namespace CairoDesktop.SupportingClasses
{
    public class TaskCategoryProvider : ITaskCategoryProvider
    {
        private TaskCategoryChangeDelegate categoryChangeDelegate;

        public TaskCategoryProvider()
        {
        }

        public void Dispose()
        {
        }

        public string GetCategory(ApplicationWindow window)
        {
            if (window.IsUWP)
            {
                return window.AppUserModelID.ToLower();
            }

            return window.WinFileName.ToLower();
        }

        public void SetCategoryChangeDelegate(TaskCategoryChangeDelegate changeDelegate)
        {
            categoryChangeDelegate = changeDelegate;

            // request new categories in case of preference change
            categoryChangeDelegate();
        }
    }
}
