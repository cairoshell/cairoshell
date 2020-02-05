using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using CairoDesktop.Interop;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using System.Windows.Media.Imaging;
using CairoDesktop.Common;
using System.Linq;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopNavigationToolbar.xaml
    /// </summary>
    public partial class DesktopNavigationToolbar : Window
    {
        private static DesktopNavigationToolbar _instance = null;

        private WindowInteropHelper helper;
        private System.Windows.Controls.ContextMenu browseContextMenu;
        private LowLevelKeyboardListener lowLevelKeyboardListener;

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

            lowLevelKeyboardListener = new Common.LowLevelKeyboardListener();
            lowLevelKeyboardListener.HookKeyboard();
            lowLevelKeyboardListener.OnKeyDown += LowLevelKeyboardListener_OnKeyDown;
            lowLevelKeyboardListener.OnKeyUp += LowLevelKeyboardListener_OnKeyUp;
        }

        bool backButtonTypeToggled = false;
        private void LowLevelKeyboardListener_OnKeyDown(object sender, Common.KeyEventArgs e)
        {
            if (IsMouseOver)
            {
                if (e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift)
                {
                    backButtonImg.Source = new BitmapImage(new Uri("/Resources/controlsParentFolder.png", UriKind.RelativeOrAbsolute));
                    backButtonTypeToggled = true;
                    btnBack.ToolTip = "Parent";
                }
            }
        }

        private void LowLevelKeyboardListener_OnKeyUp(object sender, Common.KeyEventArgs e)
        {
            if (backButtonTypeToggled)
            {
                if (e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift)
                {
                    backButtonTypeToggled = false;
                    backButtonImg.Source = new BitmapImage(new Uri("/Resources/controlsBack.png", UriKind.RelativeOrAbsolute));
                    btnBack.ToolTip = "Back";
                }
            }
        }

        private void SetPosition()
        {
            if (Settings.Instance.DesktopNavigationToolbarLocation != default(System.Windows.Point) &&
                PointExistsOnScreen(Settings.Instance.DesktopNavigationToolbarLocation))
            {
                Top = Settings.Instance.DesktopNavigationToolbarLocation.Y;
                Left = Settings.Instance.DesktopNavigationToolbarLocation.X;
            }
            else
            {
                Top = AppBarHelper.PrimaryMonitorSize.Height - Height - 150;
                Left = (AppBarHelper.PrimaryMonitorSize.Width / 2) - (Width / 2);
            }
        }

        private bool PointExistsOnScreen(Point desktopNavigationToolbarLocation)
        {
            bool result = false;
            if (Screen.AllScreens.Any(s => s.Bounds.Contains((int)desktopNavigationToolbarLocation.X, (int)desktopNavigationToolbarLocation.Y)))
            {
                result = true;
            }

            return result;
        }

        private void SetPosition(uint x, uint y)
        {
            // adjust size for dpi
            Shell.TransformFromPixels(x, y, out int sWidth, out int sHeight);

            Top = sHeight - Height - 150;
            Left = (sWidth / 2) - (Width / 2);
        }

        private void HomeButton_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {

                if (Owner is Desktop owningDesktop)
                {
                    string desktopPath = Settings.Instance.DesktopDirectory;

                    if (!Directory.Exists(desktopPath))
                        desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    if (owningDesktop.CurrentLocation != desktopPath)
                        owningDesktop.Navigate(desktopPath);
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
                owningDesktop.ClearNavigation();
        }

        private void LocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem)
                if (menuItem.Tag is string location)
                    if (Owner is Desktop owningDesktop)
                        owningDesktop.Navigate(location);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (backButtonTypeToggled)
            {
                NavigateToParent();
            }
            else
            {
                NavigateBackward();
            }
        }

        private void btnBack_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigateToParent();
        }

        internal void NavigateToParent()
        {
            if (Owner is Desktop owningDesktop)
            {
                DirectoryInfo parentDirectoryInfo = Directory.GetParent(owningDesktop.CurrentLocation);
                if (parentDirectoryInfo != null && parentDirectoryInfo.Exists)
                {
                    string parentPath = parentDirectoryInfo.FullName;
                    owningDesktop.Navigate(parentPath);
                }
            }
        }

        internal void NavigateBackward()
        {
            if (Owner is Desktop owningDesktop)
                owningDesktop.NavigateBackward();
        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            NavigateForward();
        }

        internal void NavigateForward()
        {
            if (Owner is Desktop owningDesktop)
                owningDesktop.NavigateForward();
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
                // WM_WINDOWPOSCHANGING arrives here before the desktop window.
                if (!ToolbarOwner.IsOverlayOpen)
                {
                    // if the overlay isn't open, we always want to be on the bottom. modify the WINDOWPOS structure so that nothing can change our z-order.

                    // Extract the WINDOWPOS structure corresponding to this message
                    NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                    // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                    // If we are intentionally setting our z-order, allow it. The desktop sets this flag whenever it decides to go to the bottom.
                    if (!ToolbarOwner.IsLowering && (wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                    {
                        // add the SWP_NOZORDER flag
                        wndPos.flags = wndPos.flags | NativeMethods.SetWindowPosFlags.SWP_NOZORDER;
                        wndPos.UpdateMessage(lParam);
                    }
                }
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
            {
                e.Cancel = true;
            }
            else
            {
                lowLevelKeyboardListener.OnKeyDown -= LowLevelKeyboardListener_OnKeyDown;
                lowLevelKeyboardListener.OnKeyUp -= LowLevelKeyboardListener_OnKeyUp;
                lowLevelKeyboardListener.UnHookKeyboard();
                lowLevelKeyboardListener = null;
            }
        }

        public static DesktopNavigationToolbar Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DesktopNavigationToolbar();
                }

                return _instance;
            }
        }

        private void DesktopToolbar_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    break;
                case System.Windows.Input.MouseButton.Middle:
                    // Maybe Navigate Home?
                    break;
                case System.Windows.Input.MouseButton.Right:
                    break;

                case System.Windows.Input.MouseButton.XButton1:
                    NavigateBackward();
                    break;
                case System.Windows.Input.MouseButton.XButton2:
                    NavigateForward();
                    break;
            }
        }

        private void DesktopToolbar_LocationChanged(object sender, EventArgs e)
        {
            Settings.Instance.DesktopNavigationToolbarLocation = new Point(Left, Top);
        }
    }
}