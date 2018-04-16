using System.Windows;

namespace CairoDesktop.Extensibility.ObjectModel
{
    public abstract class MenuItem
    {
        public abstract string Header { get; }
        public abstract void MenuItem_Click(object sender, RoutedEventArgs e);
    }
}