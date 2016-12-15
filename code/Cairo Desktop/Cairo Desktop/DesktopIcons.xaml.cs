using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using System.Windows.Markup;

namespace CairoDesktop {
    /// <summary>
    /// Interaction logic for DesktopIcons.xaml
    /// </summary>
    public partial class DesktopIcons : UserControl 
    {
        public static DependencyProperty locationsProperty = DependencyProperty.Register("Locations", typeof(InvokingObservableCollection<SystemDirectory>), typeof(DesktopIcons), new PropertyMetadata(new InvokingObservableCollection<SystemDirectory>(Dispatcher.CurrentDispatcher)));

        public DesktopIcons() 
        {
            InitializeComponent();
            
            String desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (Directory.Exists(desktopPath)) {
                SystemDirectory desktopSysDir = new SystemDirectory(desktopPath, Dispatcher.CurrentDispatcher);
                Locations.Add(desktopSysDir);
            }
        }

        public InvokingObservableCollection<SystemDirectory> Locations
        {
            get
            {
                return GetValue(locationsProperty) as InvokingObservableCollection<SystemDirectory>;
            }
            set
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke((Action)(() => this.Locations = value), null);
                    return;
                }

                SetValue(locationsProperty, value);
            }
        }

        private void File_Button_Click(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = senderButton.CommandParameter as String;
            try 
            {
                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(senderButton.CommandParameter as String);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory && Properties.Settings.Default.EnableDynamicDesktop)
                {
                    ((this.Parent as Grid).Parent as Desktop).PathHistory.Push(this.Locations[0].DirectoryInfo.FullName);
                    this.Locations[0] = new SystemDirectory((senderButton.CommandParameter as String), Dispatcher.CurrentDispatcher);
                }
                else
                {
                    proc.Start();
                }
            } 
            catch 
            {
                // No 'Open' command associated with this filetype in the registry
                Interop.Shell.ShowOpenWithDialog(proc.StartInfo.FileName);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) 
        {
            CustomCommands.Icon_MenuItem_Click(sender, e);
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_ContextMenu_Loaded(sender, e);
        }
    }
}
