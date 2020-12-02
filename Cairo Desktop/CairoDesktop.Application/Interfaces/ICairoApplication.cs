using System;
using System.Collections.Generic;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoApplication<TMenuIemEventArgs> where TMenuIemEventArgs : EventArgs
    {
        List<IShellExtension> Extensions { get; }

        List<IMenuItem<TMenuIemEventArgs>> Places { get; }
    }
}