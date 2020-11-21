using System;

namespace CairoDesktop.WindowsTasks
{
    public interface ITaskCategoryProvider : IDisposable
    {
        string GetCategory(ApplicationWindow window);

        void SetCategoryChangeDelegate(TaskCategoryChangeDelegate changeDelegate);
    }
}
