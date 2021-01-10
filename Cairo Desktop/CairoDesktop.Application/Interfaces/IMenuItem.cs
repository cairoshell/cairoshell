using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface IMenuItem
    {
        string Header { get; }

        void MenuItem_Click<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;
    }
}