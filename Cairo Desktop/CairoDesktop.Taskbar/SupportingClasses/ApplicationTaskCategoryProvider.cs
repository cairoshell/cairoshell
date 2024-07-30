using ManagedShell.WindowsTasks;

namespace CairoDesktop.Taskbar.SupportingClasses
{
    public class ApplicationTaskCategoryProvider : ITaskCategoryProvider
    {
        private TaskCategoryChangeDelegate categoryChangeDelegate;

        public ApplicationTaskCategoryProvider()
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
            if (changeDelegate == null)
            {
                return;
            }

            categoryChangeDelegate = changeDelegate;

            // request new categories in case of preference change
            categoryChangeDelegate?.Invoke();
        }
    }
}
