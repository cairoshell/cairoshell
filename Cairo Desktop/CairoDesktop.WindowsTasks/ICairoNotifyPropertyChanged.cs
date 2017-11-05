using System.ComponentModel;

namespace CairoDesktop.WindowsTasks
{
    public interface ICairoNotifyPropertyChanged : INotifyPropertyChanged
    {
        void OnPropertyChanged(string PropertyName);
    }
}
