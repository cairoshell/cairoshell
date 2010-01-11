
using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
// Helper code - thanks to Greg Franklin - MSFT
using SHAppBarMessage1.Common;
using CairoDesktop.Interop;
using System.Resources;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Markup;

namespace CairoDesktop
{
    public partial class MenuBar
    {
        private WindowInteropHelper helper;
        private IntPtr handle;
        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;
        private int appbarMessageId = -1;

        private String configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\CairoAppConfig.xml";
        private String fileManger = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.FileManager);

        public MenuBar()
        {
            this.InitializeComponent();
            if (Properties.Settings.Default.MenuBarWhite)
            {
                ResourceDictionary CairoDictionary = (ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "CairoStyles_alt.xaml"));
                this.Resources.MergedDictionaries[0] = CairoDictionary;
                SolidColorBrush borderBrushColor = new SolidColorBrush();
                borderBrushColor.Color = Color.FromArgb(135, 0, 0, 0);
                this.BorderBrush = borderBrushColor;
                this.BorderThickness = new Thickness(0, 0, 0, 0);
                this.Height = 22;
                this.MaxHeight = 22;
                this.Background = Brushes.Transparent;
                BitmapImage CairoMenuIconBlack = new BitmapImage();
                CairoMenuIconBlack.BeginInit();
                CairoMenuIconBlack.UriSource = new Uri("pack://application:,,,/Resources/cairoMenuBlack.png", UriKind.RelativeOrAbsolute);
                CairoMenuIconBlack.EndInit();
                CairoMenuIcon.Source = CairoMenuIconBlack;
                BitmapImage CairoSearchMenuIconBlack = new BitmapImage();
                CairoSearchMenuIconBlack.BeginInit();
                CairoSearchMenuIconBlack.UriSource = new Uri("pack://application:,,,/Resources/searchBlack.png", UriKind.RelativeOrAbsolute);
                CairoSearchMenuIconBlack.EndInit();
                CairoSearchMenuIcon.Source = CairoSearchMenuIconBlack;
            }

            this.CommandBindings.Add(new CommandBinding(CustomCommands.OpenSearchResult, ExecuteOpenSearchResult));

            // Show the search button only if the service is running
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                ObjectDataProvider vistaSearchProvider = new ObjectDataProvider();
                vistaSearchProvider.ObjectType = typeof(VistaSearchProvider.VistaSearchProviderHelper);
                CairoSearchMenu.DataContext = vistaSearchProvider;
            } 
            else
            {
                CairoSearchMenu.Visibility = Visibility.Collapsed;
                DispatcherTimer searchcheck = new DispatcherTimer(new TimeSpan(0, 0, 7), DispatcherPriority.Normal, delegate
                {

                    if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
                    {
                        ObjectDataProvider vistaSearchProvider = new ObjectDataProvider();
                        vistaSearchProvider.ObjectType = typeof(VistaSearchProvider.VistaSearchProviderHelper);
                        CairoSearchMenu.DataContext = vistaSearchProvider;
                        CairoSearchMenu.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CairoSearchMenu.Visibility = Visibility.Collapsed;
                    }

                }, this.Dispatcher);

            }
            if (System.Environment.OSVersion.Version.Major < 6)
            {
                PlacesDownloadsItem.Visibility = Visibility.Collapsed;
            }
            // ---------------------------------------------------------------- //

            InitializeClock();

            //Set Quick Launch category to not show in menu
            AppGrabber.Category ql = appGrabber.CategoryList.GetCategory("Quick Launch");
            if (ql != null)
            {
                ql.ShowInMenu = false;
            }

            //Set Programs Menu to use appGrabber's ProgramList as its source
            categorizedProgramsList.ItemsSource = appGrabber.CategoryList;
        }

        ///
        /// Focuses the specified UI element.
        ///
        /// The UI element.
        public void FocusSearchBox(object sender, RoutedEventArgs e)
        {
            searchStr.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                searchStr.Focusable = true;
                searchStr.Focus();
                Keyboard.Focus(searchStr);
            }),
            DispatcherPriority.Render);
        }

        public void ExecuteOpenSearchResult(object sender, ExecutedRoutedEventArgs e)
        {
            // Get parameter (e.Parameter as T)
            // Try shell execute...
            // TODO: Determine which app to start the file as and boom!
            var searchObj = (VistaSearchProvider.SearchResult)e.Parameter;

            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = searchObj.Path; // e.Parameter as T.x
            p.StartInfo.Verb = "Open";

            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                CairoMessage.Show("Woops, it seems we had some trouble opening the search result you chose.\n\n The error we received was: " + ex.Message, "Uh Oh!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (appbarMessageId == -1)
            {
                return IntPtr.Zero;
            }

            if (msg == appbarMessageId)
            {
                System.Diagnostics.Trace.WriteLine("Callback on AppBarMessage: " + wparam.ToString());
                switch (wparam.ToInt32())
                {
                    case 1:
                        // Reposition to the top of the screen.
                        if (this.Top != 0)
                        {
                            System.Diagnostics.Trace.WriteLine("Repositioning menu bar to top of screen.");
                            this.Top = 0;
                        }
                        /*SHAppBarMessageHelper.QuerySetPosition(hwnd, 
                            new System.Drawing.Size() { Height = (int)this.ActualWidth, Width = (int)this.ActualWidth }, 
                            SHAppBarMessage1.Win32.NativeMethods.ABEdge.ABE_TOP);*/
                        break;
                }
                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Initializes the dispatcher timers to updates the time and date bindings
        /// </summary>
        private void InitializeClock()
        {
            // Create our timer for clock
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                string timeFormat = Properties.Settings.Default.TimeFormat;
                if (string.IsNullOrEmpty(timeFormat))
                {
                    timeFormat = "T"; /// culturally safe long time pattern
                }

                dateText.Text = DateTime.Now.ToString(timeFormat);
            }, this.Dispatcher);

            DispatcherTimer fulldatetimer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                string dateFormat = Properties.Settings.Default.DateFormat;
                if (string.IsNullOrEmpty(dateFormat))
                {
                    dateFormat = "D"; // Culturally safe Long Date Pattern
                }

                dateText.ToolTip = DateTime.Now.ToString(dateFormat);
            }, this.Dispatcher);
        }

        private void OnWindowInitialized(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            handle = helper.Handle;
            System.Drawing.Size size = new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight);

            appbarMessageId = SHAppBarMessageHelper.RegisterBar(handle, size);
            //SHAppBarMessageHelper.QuerySetPosition(handle, size, SHAppBarMessage1.Win32.NativeMethods.ABEdge.ABE_TOP);

            if (Properties.Settings.Default.EnableSysTray == true)
            {
                SysTray.InitializeSystemTray();
            }
            else
            {
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (CairoMessage.ShowOkCancel("You will need to reboot or use the start menu shortcut in order to run Cairo again.", "Are you sure you want to exit Cairo?", "Resources/cairoIcon.png", "Exit Cairo", "Cancel") == true)
            {
                //SHAppBarMessageHelper.DeRegisterBar(handle);
                System.Drawing.Size size = new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight);
                SHAppBarMessageHelper.RegisterBar(handle, size);
                SysTray.DestroySystemTray();
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void OnWindowResize(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("OnWindowResize raised...");
            System.Drawing.Size size = new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight);
            //SHAppBarMessageHelper.QuerySetPosition(handle, size, SHAppBarMessage1.Win32.NativeMethods.ABEdge.ABE_TOP);
            SHAppBarMessageHelper.ABSetPos(handle, size);
        }

        private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            try {
                System.Diagnostics.Process.Start(item.CommandParameter.ToString());
            } catch {
                CairoMessage.Show("The file could not be found.  If you just removed this program, try removing it from the App Grabber to make the icon go away.", "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AboutCairo(object sender, RoutedEventArgs e)
        {
            CairoMessage.Show(
                // Replace next line with the Version
                "Version 0.0.1.9 - Milestone 2 Preview 1"
                +"\nCopyright © 2007-2009 Cairo Development Team.  All rights reserved.\n" +
                // Replace next line with the ID Key
"Not for redistribution."
                , "Cairo Desktop Environment", MessageBoxButton.OK, MessageBoxImage.None);
        } 
        private void OpenLogoffBox(object sender, RoutedEventArgs e)
        {
            bool? LogoffChoice = CairoMessage.ShowOkCancel("You will lose all unsaved documents and be logged off.", "Are you sure you want to log off now?", "Resources/logoffIcon.png", "Log Off", "Cancel");
            if (LogoffChoice.HasValue && LogoffChoice.Value)
            {
                NativeMethods.Logoff();
            }
            else
            {
            }
        }
        private void OpenRebootBox(object sender, RoutedEventArgs e)
        {
            bool? RebootChoice = CairoMessage.ShowOkCancel("You will lose all unsaved documents and your computer will restart.", "Are you sure you want to restart now?", "Resources/restartIcon.png", "Restart", "Cancel");
            if (RebootChoice.HasValue && RebootChoice.Value)
            {
                NativeMethods.Reboot();
            }
            else
            {
            }
        }
        private void OpenShutDownBox(object sender, RoutedEventArgs e)
        {
            bool? ShutdownChoice = CairoMessage.ShowOkCancel("You will lose all unsaved documents and your computer will turn off.", "Are you sure you want to shut down now?", "Resources/shutdownIcon.png", "Shut Down", "Cancel");
            if (ShutdownChoice.HasValue && ShutdownChoice.Value)
            {
                NativeMethods.Shutdown();
            }
            else
            {
            }
        }
        private void OpenCloseCairoBox(object sender, RoutedEventArgs e)
        {
            bool? CloseCairoChoice = CairoMessage.ShowOkCancel("You will need to reboot or use the start menu shortcut in order to run Cairo again.", "Are you sure you want to exit Cairo?", "Resources/cairoIcon.png", "Exit Cairo", "Cancel");
            if (CloseCairoChoice.HasValue && CloseCairoChoice.Value)
            {
                //SHAppBarMessageHelper.DeRegisterBar(handle);
                System.Drawing.Size size = new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight);
                SHAppBarMessageHelper.RegisterBar(handle, size);
                SysTray.DestroySystemTray();
                Application.Current.Shutdown();
                // TODO: Will want to relaunch explorer.exe when we start disabling it
            }
            else
            {
            }
        }
        private void OpenMyDocs(object sender, RoutedEventArgs e)
        {
            string myDocspath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = myDocspath;
            prc.Start();
        }
        private void OpenMyPics(object sender, RoutedEventArgs e)
        {
            string myDocspath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = myDocspath;
            prc.Start();
        }
        private void OpenMyMusic(object sender, RoutedEventArgs e)
        {
            string myDocspath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = myDocspath;
            prc.Start();
        }
        private void OpenDownloads(object sender, RoutedEventArgs e)
        {
            string userprofile = System.Environment.GetEnvironmentVariable("USERPROFILE");
            string myDocspath = userprofile + @"\Downloads\";
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = myDocspath;
            prc.Start();
        }
        private void OpenMyComputer(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = @"::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
            prc.Start();

        }
        private void OpenUserFolder(object sender, RoutedEventArgs e)
        {
            string userprofile = System.Environment.GetEnvironmentVariable("USERPROFILE");
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = userprofile;
            prc.Start();
        }
        private void OpenProgramFiles(object sender, RoutedEventArgs e)
        {
            string progfiles = System.Environment.GetEnvironmentVariable("ProgramFiles");
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = progfiles;
            prc.Start();
        }
        private void OpenRecycleBin(object sender, RoutedEventArgs e)
        {
            string bin = @"::{645FF040-5081-101B-9F08-00AA002F954E}";
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = fileManger;
            prc.StartInfo.Arguments = bin;
            prc.Start();
        }
        private void OpenControlPanel(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("control.exe");
        }
        private void OpenTaskManager(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("taskmgr.exe");
        }
        private void OpenTimeDateCPL(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("timedate.cpl");
        }
        private void SysSleep(object sender, RoutedEventArgs e)
        {
            NativeMethods.Sleep();
        }
        private void InitCairoSettingsWindow(object sender, RoutedEventArgs e)
        {
            CairoSettingsWindow window = new CairoSettingsWindow();
            window.Show();
        }
        private void InitAppGrabberWindow(object sender, RoutedEventArgs e)
        {
            appGrabber.ShowDialog();
        }
        private void LaunchShortcut(object sender, RoutedEventArgs e)
        {
            Button item = (Button)sender;
            string ItemLoc = item.CommandParameter.ToString();
            System.Diagnostics.Process.Start(ItemLoc);
        }
        
        /// <summary>
        /// Retrieves the users ID key from a compiled resources file.
        /// </summary>
        /// <returns>The users ID key.</returns>
        private string GetUsersIdKey()
        {
            string idKey = "For internal use only.";
            string resKey = null;

            try
            {
                var mgr = ResourceManager.CreateFileBasedResourceManager("cairo", Environment.CurrentDirectory, null);
                resKey = mgr.GetString("ID-Key");
            }
            catch (Exception)
            {
                resKey = null;
            }

            return resKey ?? idKey;
        }
   }
}
