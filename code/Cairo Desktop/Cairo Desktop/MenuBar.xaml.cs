using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CairoDesktop.Interop;
using System.Windows.Input;
using System.Windows.Markup;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;

namespace CairoDesktop
{
    public partial class MenuBar
    {
        // AppBar properties
        private WindowInteropHelper helper;
        private IntPtr handle;
        private int appbarMessageId = -1;

        // AppGrabber instance
        public AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

        private String configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\CairoAppConfig.xml";
        private String fileManger = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.FileManager);

        public MenuBar()
        {
            this.InitializeComponent();

            this.Width = AppBarHelper.PrimaryMonitorSize.Width;

            this.CommandBindings.Add(new CommandBinding(CustomCommands.OpenSearchResult, ExecuteOpenSearchResult));

            // Set username
            miUserName.Header = Environment.UserName.Replace("_", "__");

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

            // Only show Downloads folder on Vista or greater
            if (System.Environment.OSVersion.Version.Major < 6)
            {
                PlacesDownloadsItem.Visibility = Visibility.Collapsed;
            }

            InitializeClock();

            // Set Quick Launch and Uncategorized categories to not show in menu
            AppGrabber.Category ql = appGrabber.CategoryList.GetCategory("Quick Launch");
            if (ql != null)
            {
                ql.ShowInMenu = false;
            }
            AppGrabber.Category uncat = appGrabber.CategoryList.GetCategory("Uncategorized");
            if (uncat != null)
            {
                uncat.ShowInMenu = false;
            }

            // Set Programs Menu to use appGrabber's ProgramList as its source
            categorizedProgramsList.ItemsSource = appGrabber.CategoryList;
        }

        private void shutdown()
        {
            AppBarHelper.RegisterBar(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight));
            AppBarHelper.ResetWorkArea();
            SysTray.DestroySystemTray();
            Application.Current.Shutdown();
        }

        private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            try
            {
                Shell.StartProcess(item.CommandParameter.ToString());
            }
            catch
            {
                CairoMessage.Show("The file could not be found.  If you just removed this program, try removing it from the App Grabber to make the icon go away.", "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Date/time
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

        private void OpenTimeDateCPL(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("timedate.cpl");
        }
        #endregion

        #region Events
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (appbarMessageId == -1)
            {
                return IntPtr.Zero;
            }

            if (msg == appbarMessageId)
            {
                switch (wparam.ToInt32())
                {
                    case 1:
                        // Reposition to the top of the screen.
                        AppBarHelper.ABSetPos(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_TOP);
                        break;
                }
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void OnWindowInitialized(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            handle = helper.Handle;

            appbarMessageId = AppBarHelper.RegisterBar(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_TOP);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableSysTray == true)
            {
                SysTray.InitializeSystemTray();
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            shutdown();
        }

        private void OnWindowResize(object sender, RoutedEventArgs e)
        {
            // making sure this isn't necessary
            //AppBarHelper.ABSetPos(handle, new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), AppBarHelper.ABEdge.ABE_TOP);
        }
        #endregion

        #region Cairo menu items
        private void AboutCairo(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            CairoMessage.Show(
                "Version " + version + " - Pre-release"
                +"\n\nCopyright © 2007-" + DateTime.Now.Year.ToString() + " Cairo Development Team and community contributors.  All rights reserved.", "Cairo Desktop Environment", MessageBoxButton.OK, MessageBoxImage.None);
        }

        private void OpenLogoffBox(object sender, RoutedEventArgs e)
        {
            bool? LogoffChoice = CairoMessage.ShowOkCancel("You will lose all unsaved documents and be logged off.", "Are you sure you want to log off now?", "Resources/logoffIcon.png", "Log Off", "Cancel");
            if (LogoffChoice.HasValue && LogoffChoice.Value)
            {
                NativeMethods.Logoff();
            }
        }

        private void OpenRebootBox(object sender, RoutedEventArgs e)
        {
            bool? RebootChoice = CairoMessage.ShowOkCancel("You will lose all unsaved documents and your computer will restart.", "Are you sure you want to restart now?", "Resources/restartIcon.png", "Restart", "Cancel");
            if (RebootChoice.HasValue && RebootChoice.Value)
            {
                NativeMethods.Reboot();
            }
        }

        private void OpenShutDownBox(object sender, RoutedEventArgs e)
        {
            bool? ShutdownChoice = CairoMessage.ShowOkCancel("You will lose all unsaved documents and your computer will turn off.", "Are you sure you want to shut down now?", "Resources/shutdownIcon.png", "Shut Down", "Cancel");
            if (ShutdownChoice.HasValue && ShutdownChoice.Value)
            {
                NativeMethods.Shutdown();
            }
        }

        private void OpenCloseCairoBox(object sender, RoutedEventArgs e)
        {
            bool? CloseCairoChoice = CairoMessage.ShowOkCancel("You will need to reboot or use the start menu shortcut in order to run Cairo again.", "Are you sure you want to exit Cairo?", "Resources/exitIcon.png", "Exit Cairo", "Cancel");
            if (CloseCairoChoice.HasValue && CloseCairoChoice.Value)
            {
                shutdown();
            }
        }

        private void OpenControlPanel(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("control.exe");
        }

        private void OpenTaskManager(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("taskmgr.exe");
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
        #endregion

        #region Places menu items
        private void OpenMyDocs(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }

        private void OpenMyPics(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        }

        private void OpenMyMusic(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
        }

        private void OpenDownloads(object sender, RoutedEventArgs e)
        {
            string userprofile = System.Environment.GetEnvironmentVariable("USERPROFILE");
            string downloadsPath = userprofile + @"\Downloads\";
            Shell.StartProcess(fileManger, downloadsPath);
        }

        private void OpenMyComputer(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
        }

        private void OpenUserFolder(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, System.Environment.GetEnvironmentVariable("USERPROFILE"));
        }

        private void OpenProgramFiles(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, System.Environment.GetEnvironmentVariable("ProgramFiles"));
        }

        private void OpenRecycleBin(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess(fileManger, "::{645FF040-5081-101B-9F08-00AA002F954E}");
        }
        #endregion

        #region Search menu
        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("search:query=" + searchStr.Text);
        }

        private void searchStr_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchStr.Text.Length > 0)
                btnViewResults.Visibility = Visibility.Visible;
            else
                btnViewResults.Visibility = Visibility.Collapsed;
        }

        private void searchStr_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                Shell.StartProcess("search:query=" + searchStr.Text);
            }
        }

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
            var searchObj = (VistaSearchProvider.SearchResult)e.Parameter;

            try
            {
                Shell.StartProcess(searchObj.Path);
            }
            catch (Exception ex)
            {
                CairoMessage.Show("Sorry, this search result was unable to be opened, because: " + ex.Message, "Uh Oh!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
