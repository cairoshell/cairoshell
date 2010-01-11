using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Windows.Markup;

namespace CairoDesktop {
    /// <summary>
    /// Interaction logic for DesktopIcons.xaml
    /// </summary>
    public partial class DesktopIcons : UserControl 
    {
        private static DependencyProperty locationsProperty = DependencyProperty.Register("Locations", typeof(InvokingObservableCollection<SystemDirectory>), typeof(DesktopIcons), new PropertyMetadata(new InvokingObservableCollection<SystemDirectory>(Dispatcher.CurrentDispatcher)));

        public DesktopIcons() 
        {
            InitializeComponent();
            if (Properties.Settings.Default.MenuBarWhite)
            {
                ResourceDictionary CairoDictionary = (ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "CairoStyles_alt.xaml"));
                this.Resources.MergedDictionaries[0] = CairoDictionary;
            }

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
                proc.Start();
            } 
            catch 
            {
                // No 'Open' command associated with this filetype in the registry
                ShowOpenWithDialog(proc.StartInfo.FileName);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) 
        {
            MenuItem item = sender as MenuItem;
            String fileName = item.CommandParameter as String;
            if (item.Header as String == "Open with...")
            {
                ShowOpenWithDialog(fileName);
                return;
            }

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Verb = item.Header as String;
            try 
            {
                proc.Start();
            } 
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error running the {0} verb on {1}. ({2})",item.Header,fileName,ex.Message));
            }
        }

        /// <summary>
        /// Calls the Windows OpenWith dialog (shell32.dll) to open the file specified.
        /// </summary>
        /// <param name="fileName">Path to the file to open</param>
        private void ShowOpenWithDialog(string fileName) 
        {
            System.Diagnostics.Process owProc = new System.Diagnostics.Process();
            owProc.StartInfo.UseShellExecute = true;
            owProc.StartInfo.FileName = Environment.GetEnvironmentVariable("WINDIR") + @"\system32\rundll32.exe";
            owProc.StartInfo.Arguments =
                @"C:\WINDOWS\system32\shell32.dll,OpenAs_RunDLL " + fileName;
            owProc.Start();
        }
    }
}
