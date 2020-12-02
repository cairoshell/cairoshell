using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface IMenuItem<in TEventArgs> where TEventArgs : EventArgs
    {
        string Header { get; }

        void MenuItem_Click(object sender, TEventArgs e);
    }
}