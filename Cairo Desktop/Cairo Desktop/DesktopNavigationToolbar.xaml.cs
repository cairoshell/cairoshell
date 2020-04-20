using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using CairoDesktop.Interop;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using CairoDesktop.Common;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CairoDesktop.Common.Logging;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopNavigationToolbar.xaml
    /// </summary>
    public partial class DesktopNavigationToolbar : Window, INotifyPropertyChanged
    {
        private WindowInteropHelper helper;
        private System.Windows.Controls.ContextMenu browseContextMenu;
        private System.Windows.Controls.ContextMenu homeContextMenu;
        private LowLevelKeyboardListener lowLevelKeyboardListener;

        private DependencyProperty navigationManagerProperty = DependencyProperty.Register("NavigationManager", typeof(NavigationManager), typeof(DesktopNavigationToolbar));
        private DependencyProperty isShiftKeyHeldProperty = DependencyProperty.Register("isShiftKeyHeld", typeof(bool), typeof(DesktopNavigationToolbar), new PropertyMetadata(new bool()));

        public Desktop ToolbarOwner
        {
            get
            {
                return Owner as Desktop;
            }
        }

        internal NavigationManager NavigationManager
        {
            get
            {
                return (NavigationManager)GetValue(navigationManagerProperty);
            }
            set
            {
                SetValue(navigationManagerProperty, value);
            }
        }

        private bool isShiftKeyHeld
        {
            get
            {
                return (bool)GetValue(isShiftKeyHeldProperty);
            }
            set
            {
                SetValue(isShiftKeyHeldProperty, value);
                OnPropertyChanged();
            }
        }

        public DesktopNavigationToolbar()
        {
            InitializeComponent();

            SetPosition();

            // set up browse context menu (is dynamically constructed)
            browseContextMenu = new System.Windows.Controls.ContextMenu();
            browseContextMenu.Closed += contextMenu_Closed;

            // set up home context menu
            homeContextMenu = new System.Windows.Controls.ContextMenu();
            homeContextMenu.Closed += contextMenu_Closed;
            System.Windows.Controls.MenuItem setHomeMenuItem = new System.Windows.Controls.MenuItem();
            setHomeMenuItem.Header = Localization.DisplayString.sDesktop_SetHome;
            setHomeMenuItem.Click += SetHomeMenuItem_Click; ;
            homeContextMenu.Items.Add(setHomeMenuItem);

            // set up keyboard listener for shift key
            lowLevelKeyboardListener = new LowLevelKeyboardListener();
            lowLevelKeyboardListener.HookKeyboard();
            lowLevelKeyboardListener.OnKeyDown += LowLevelKeyboardListener_OnKeyDown;
            lowLevelKeyboardListener.OnKeyUp += LowLevelKeyboardListener_OnKeyUp;
        }

        #region Keyboard listener
        private void LowLevelKeyboardListener_OnKeyDown(object sender, Common.KeyEventArgs e)
        {
            if (IsMouseOver)
            {
                if (e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift)
                {
                    isShiftKeyHeld = true;
                }
            }
        }

        private void LowLevelKeyboardListener_OnKeyUp(object sender, Common.KeyEventArgs e)
        {
            if (isShiftKeyHeld)
            {
                if (e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift)
                {
                    isShiftKeyHeld = false;
                }
            }
        }
        #endregion

        #region Positioning
        private void SetPosition()
        {
            if (Settings.Instance.DesktopNavigationToolbarLocation != default &&
                PointExistsOnScreen(Settings.Instance.DesktopNavigationToolbarLocation))
            {
                Top = Settings.Instance.DesktopNavigationToolbarLocation.Y / Shell.DpiScale;
                Left = Settings.Instance.DesktopNavigationToolbarLocation.X / Shell.DpiScale;
            }
            else
            {
                Top = WindowManager.PrimaryMonitorSize.Height - Height - 150;
                Left = (WindowManager.PrimaryMonitorSize.Width / 2) - (Width / 2);
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
        #endregion

        #region Button clicks
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateBackward();
        }

        private void btnBack_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationManager.NavigateToParent();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateToParent();
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateHome();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                Description = Localization.DisplayString.sDesktop_BrowseTitle,
                ShowNewFolderButton = false,
                SelectedPath = NavigationManager.CurrentPath
            })
            {
                NativeMethods.SetForegroundWindow(helper.Handle); // bring browse window to front
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    if (Directory.Exists(fbd.SelectedPath))
                        NavigationManager.NavigateTo(fbd.SelectedPath);
            }
        }
        private void btnHome_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            homeContextMenu.IsOpen = true;

            // this value is set so that if we click away from the context menu, the desktop overlay does not get toggled
            ToolbarOwner.IsToolbarContextMenuOpen = true;

            e.Handled = true;
        }

        private void btnBrowse_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NavigationManager.PathHistory.Count > 0)
            {
                browseContextMenu.Items.Clear();

                for (int i = 0; i < NavigationManager.PathHistory.Count; i++)
                {
                    System.Windows.Controls.MenuItem locationMenuItem = new System.Windows.Controls.MenuItem();
                    locationMenuItem.Header = GetCleanFolderName(NavigationManager.PathHistory[i]);
                    locationMenuItem.Tag = i;
                    locationMenuItem.Click += LocationMenuItem_Click;

                    browseContextMenu.Items.Add(locationMenuItem);
                }

                browseContextMenu.Items.Add(new System.Windows.Controls.Separator());

                System.Windows.Controls.MenuItem clearHistoryMenuItem = new System.Windows.Controls.MenuItem { Header = Localization.DisplayString.sDesktop_ClearHistory };
                clearHistoryMenuItem.Click += ClearHistoryMenuItem_Click;

                browseContextMenu.Items.Add(clearHistoryMenuItem);

                browseContextMenu.IsOpen = true;

                // this value is set so that if we click away from the context menu, the desktop overlay does not get toggled
                ToolbarOwner.IsToolbarContextMenuOpen = true;

                e.Handled = true;
            }
        }

        private void btnFwd_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateForward();
        }
        #endregion

        #region Home menu
        private void SetHomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.DesktopDirectory = NavigationManager.CurrentPath;
        }
        #endregion

        #region Path history menu
        private string GetCleanFolderName(string path)
        {
            if (Directory.GetDirectoryRoot(path) == path)
                return path;
            else
                return Path.GetFileName(path);
        }

        private void ClearHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Clear();
        }

        private void LocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem)
                if (menuItem.Tag is int index)
                    NavigationManager.NavigateToIndex(index);
        }
        #endregion

        #region Window events
        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)NativeMethods.WM.MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }
            else if (msg == (int)NativeMethods.WM.WINDOWPOSCHANGING)
            {
                // WM_WINDOWPOSCHANGING arrives here before the desktop window.
                if (!ToolbarOwner.IsOverlayOpen)
                {
                    // if the overlay isn't open, we always want to be on the bottom. modify the WINDOWPOS structure so that we always go to right above the desktop.
                    // the desktop will do the work of bringing us to the bottom.

                    // Extract the WINDOWPOS structure corresponding to this message
                    NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                    // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                    if ((wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                    {
                        // be right above the desktop.
                        wndPos.hwndInsertAfter = NativeMethods.GetWindow(ToolbarOwner.Handle, NativeMethods.GetWindow_Cmd.GW_HWNDPREV);
                        wndPos.flags = wndPos.flags | NativeMethods.SetWindowPosFlags.SWP_NOOWNERZORDER;
                        wndPos.UpdateMessage(lParam);
                    }
                }
            }
            else if (msg == (int)NativeMethods.WM.DISPLAYCHANGE)
            {
                SetPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void DesktopToolbar_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(new HwndSourceHook(WndProc));
            Shell.HideWindowFromTasks(helper.Handle);
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
                    NavigationManager.NavigateBackward();
                    break;
                case System.Windows.Input.MouseButton.XButton2:
                    NavigationManager.NavigateForward();
                    break;
            }
        }

        private void DesktopToolbar_Closing(object sender, CancelEventArgs e)
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

        private void contextMenu_Closed(object sender, RoutedEventArgs e)
        {
            if (!browseContextMenu.IsOpen && !homeContextMenu.IsOpen) ToolbarOwner.IsToolbarContextMenuOpen = false;
        }
        #endregion

        #region Drag
        private void DesktopToolbar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void DesktopToolbar_LocationChanged(object sender, EventArgs e)
        {
            Settings.Instance.DesktopNavigationToolbarLocation = new Point(Left * Shell.DpiScale, Top * Shell.DpiScale);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}