using CairoDesktop.Extensibility.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace CairoDesktop.Extensions.Places.ShellFolders
{
    public class ShellLocationMenuItem : MenuItem
    {
        private string header;
        private string command;

        public ShellLocationMenuItem(string header, string command)
        {
            this.header = header;
            this.command = command;
        }
        public override string Header
        {
            get { return header; }
        }

        public override void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(command);
        }
    }
}