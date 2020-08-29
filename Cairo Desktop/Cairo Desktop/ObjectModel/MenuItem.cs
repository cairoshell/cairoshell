using System.Windows;

namespace CairoDesktop.ObjectModel
{
    public abstract class MenuItem
    {
        public abstract string Header { get; }

        public abstract void MenuItem_Click(object sender, RoutedEventArgs e);
    }
}