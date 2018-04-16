using System.Diagnostics;
using System.Windows;
using CairoDesktop.Extensibility.ObjectModel;

namespace CairoDesktop.Plugins.Places1
{
    public class GodModeMenuItem : MenuItem
    {
        public override string Header => "GodMode";

        public override void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // https://winaero.com/blog/clsid-guid-shell-list-windows-10/
            Process.Start("shell:::{ED7BA470-8E54-465E-825C-99712043E01C}");
        }
    }
}
