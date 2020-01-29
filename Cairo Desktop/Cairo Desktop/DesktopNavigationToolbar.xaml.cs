using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using CairoDesktop.Interop;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopNavigationToolbar.xaml
    /// </summary>
    public partial class DesktopNavigationToolbar : Window
    {
        private WindowInteropHelper helper;
        private System.Windows.Controls.ContextMenu browseContextMenu;

        public Desktop ToolbarOwner
        {
            get
            {
                return (Owner as Desktop);
            }
        }

        public DesktopNavigationToolbar()
        {
            InitializeComponent();
            SetPosition();

            browseContextMenu = new System.Windows.Controls.ContextMenu();
        }

        private void SetPosition()
        {
            Top = AppBarHelper.PrimaryMonitorSize.Height - Height - 150;
            Left = (AppBarHelper.PrimaryMonitorSize.Width / 2) - (Width / 2);
        }

        private void SetPosition(uint x, uint y)
        {
            // adjust size for dpi
            Shell.TransformFromPixels(x, y, out int sWidth, out int sHeight);

            Top = sHeight - Height - 150;
            Left = (sWidth / 2) - (Width / 2);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is Desktop owningDesktop)
            {
                DirectoryInfo parent = owningDesktop.Icons.Location.DirectoryInfo.Parent;
                if (parent != null)
                    owningDesktop.Navigate(parent.FullName);
            }
        }

        private void HomeButton_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                string defaultDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string userDesktopPath = Settings.DesktopDirectory;

                if (Owner is Desktop owningDesktop)
                {
                    if (Directory.Exists(userDesktopPath))
                        owningDesktop.Navigate(userDesktopPath);
                    else if (Directory.Exists(defaultDesktopPath))
                        owningDesktop.Navigate(defaultDesktopPath);
                }

                e.Handled = true;
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
            }
        }
        private void BrowseButton_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {

            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (Owner is Desktop owningDesktop)
                {
                    if (owningDesktop.PathHistory.Count > 0)
                    {
                        browseContextMenu.Items.Clear();

                        foreach (string location in owningDesktop.PathHistory)
                        {
                            System.Windows.Controls.MenuItem locationMenuItem = new System.Windows.Controls.MenuItem();
                            locationMenuItem.Header = GetCleanFolderName(location);
                            locationMenuItem.Tag = location;
                            locationMenuItem.Click += LocationMenuItem_Click;

                            browseContextMenu.Items.Add(locationMenuItem);
                        }

                        browseContextMenu.Items.Add(new System.Windows.Controls.Separator());

                        System.Windows.Controls.MenuItem clearHistoryMenuItem = new System.Windows.Controls.MenuItem { Header = "Clear History" };
                        clearHistoryMenuItem.Click += ClearHistoryMenuItem_Click;

                        browseContextMenu.Items.Add(clearHistoryMenuItem);

                        browseContextMenu.IsOpen = true;

                        e.Handled = true;
                    }
                }
            }
        }

        private string GetCleanFolderName(string path)
        {
            if (Directory.GetDirectoryRoot(path) == path)
                return path;
            else
                return Path.GetFileName(path);
        }

        private void ClearHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is Desktop owningDesktop)
                owningDesktop.PathHistory.Clear();
        }

        private void LocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem)
                if (menuItem.Tag is string location)
                    if (Owner is Desktop owningDesktop)
                        owningDesktop.Navigate(location);
        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            // Should this be handled by a new method? owningDesktop.MoveForward() ???
            if (Owner is Desktop owningDesktop && owningDesktop.PathHistory.Count > 0)
                owningDesktop.CurrentLocation = owningDesktop.PathHistory.Pop();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is Desktop owningDesktop)
            {
                ToolbarOwner.IsFbdOpen = true;
                using (FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    Description = Localization.DisplayString.sDesktop_BrowseTitle,
                    ShowNewFolderButton = false,
                    SelectedPath = owningDesktop.CurrentLocation
                })
                {
                    NativeMethods.SetForegroundWindow(helper.Handle); // bring browse window to front
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        if (owningDesktop.CurrentLocation != fbd.SelectedPath) // added to prevent duplicate entries into the PathHistory... Should we reimpliment the DynamicDesktop to handle this on its own???
                            if (Directory.Exists(fbd.SelectedPath))
                                owningDesktop.Navigate(fbd.SelectedPath);
                    ToolbarOwner.IsFbdOpen = false;
                    ToolbarOwner.Activate(); // activate desktop, which should actually put it back to the bottom
                }
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }
            else if (msg == NativeMethods.WM_WINDOWPOSCHANGING)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                SetPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(new HwndSourceHook(WndProc));
            Shell.HideWindowFromTasks(helper.Handle);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, GetWindowStyle() | NativeMethods.WS_EX_NOACTIVATE);
        }

        private int GetWindowStyle()
        {
            return NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Startup.IsShuttingDown)
                e.Cancel = true;
        }
    }
}