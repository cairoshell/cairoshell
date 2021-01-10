using CairoDesktop.Application.Interfaces;
using System;
using System.Diagnostics;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    public class ShellLocationMenuItem : IMenuItem
    {
        private readonly string _command;

        public ShellLocationMenuItem(string header, string command)
        {
            Header = header;
            _command = command;
        }

        public string Header { get; }

        public void MenuItem_Click<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            Process.Start(_command);
        }
    }
}