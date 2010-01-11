using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace CairoDesktop.WindowsTasks
{
    public interface ICairoNotifyPropertyChanged : INotifyPropertyChanged
    {
        void OnPropertyChanged(string PropertyName);
    }
}
