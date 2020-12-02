using CairoDesktop.Application.Interfaces;
using System.Diagnostics;
using System.Windows;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    public class ShellLocationMenuItem : IMenuItem<RoutedEventArgs>
    {
        private readonly string _command;

        public ShellLocationMenuItem(string header, string command)
        {
            Header = header;
            _command = command;
        }

        public string Header { get; }

        public void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_command);
        }
    }
}