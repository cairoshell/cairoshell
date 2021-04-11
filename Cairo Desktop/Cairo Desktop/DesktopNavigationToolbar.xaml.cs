using System;
using System.Windows;
using System.IO;
using System.Windows.Interop;
using ManagedShell.Interop;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Controls;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Services;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopNavigationToolbar.xaml
    /// </summary>
    public partial class DesktopNavigationToolbar : Window, INotifyPropertyChanged
    {
        private const int DEFAULT_Y = 150;
        private WindowInteropHelper helper;
        private readonly ContextMenu browseContextMenu;
        private readonly ContextMenu homeContextMenu;
        private readonly ContextMenu toolbarContextMenu;
        private LowLevelKeyboardListener lowLevelKeyboardListener;
        private readonly ICairoApplication _cairoApplication;
        private readonly DesktopManager _desktopManager;

        private static DependencyProperty navigationManagerProperty = DependencyProperty.Register("NavigationManager", typeof(NavigationManager), typeof(DesktopNavigationToolbar));
        private static DependencyProperty isShiftKeyHeldProperty = DependencyProperty.Register("isShiftKeyHeld", typeof(bool), typeof(DesktopNavigationToolbar), new PropertyMetadata(new bool()));

        public bool AllowClose;

        public bool IsContextMenuOpen
        {
            get
            {
                // this value is checked so that if we click away from the context menu, the desktop overlay does not get toggled
                return browseContextMenu.IsOpen || homeContextMenu.IsOpen;
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

        public DesktopNavigationToolbar(ICairoApplication cairoApplication, DesktopManager manager)
        {
            InitializeComponent();

            _cairoApplication = cairoApplication;
            _desktopManager = manager;

            SetPosition();

            // set up browse context menu (is dynamically constructed)
            browseContextMenu = new ContextMenu();

            // set up toolbar context menu
            toolbarContextMenu = new ContextMenu();
            MenuItem resetPositionMenuItem = new MenuItem
            {
                Header = Localization.DisplayString.sDesktop_ResetPosition
            };
            resetPositionMenuItem.Click += ResetPositionMenuItem_OnClick;
            toolbarContextMenu.Items.Add(resetPositionMenuItem);

            // set up home context menu
            homeContextMenu = new ContextMenu();
            MenuItem setHomeMenuItem = new MenuItem
            {
                Header = Localization.DisplayString.sDesktop_SetHome
            };
            setHomeMenuItem.Click += SetHomeMenuItem_Click;
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
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                {
                    isShiftKeyHeld = true;
                }
            }
        }

        private void LowLevelKeyboardListener_OnKeyUp(object sender, Common.KeyEventArgs e)
        {
            if (isShiftKeyHeld)
            {
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
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
                Top = Settings.Instance.DesktopNavigationToolbarLocation.Y / DpiHelper.DpiScale;
                Left = Settings.Instance.DesktopNavigationToolbarLocation.X / DpiHelper.DpiScale;
            }
            else
            {
                SetPositionDefault(WindowManager.PrimaryMonitorSize.Width, WindowManager.PrimaryMonitorSize.Height);
            }
        }

        private bool PointExistsOnScreen(Point desktopNavigationToolbarLocation)
        {
            bool result = false;
            if (System.Windows.Forms.Screen.AllScreens.Any(s => s.Bounds.Contains((int)desktopNavigationToolbarLocation.X, (int)desktopNavigationToolbarLocation.Y)))
            {
                result = true;
            }

            return result;
        }

        private void SetPosition(uint x, uint y)
        {
            if (Settings.Instance.DesktopNavigationToolbarLocation != default &&
                PointExistsOnScreen(Settings.Instance.DesktopNavigationToolbarLocation))
            {
                // use custom size
                Top = Settings.Instance.DesktopNavigationToolbarLocation.Y / DpiHelper.DpiScale;
                Left = Settings.Instance.DesktopNavigationToolbarLocation.X / DpiHelper.DpiScale;
            }
            else
            {
                // use size from wndproc and adjust for dpi
                DpiHelper.TransformFromPixels(x, y, out int sWidth, out int sHeight);

                SetPositionDefault(sWidth, sHeight);
            }
        }

        private void SetPositionDefault(int screenWidth, int screenHeight)
        {
            Top = screenHeight - Height - DEFAULT_Y;
            Left = (screenWidth / 2) - (Width / 2);
        }

        public void BringToFront()
        {
            NativeMethods.SetWindowPos(helper.Handle, (IntPtr)NativeMethods.WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE);
        }

        public void SendToBottom()
        {
            if (_desktopManager.ShellWindow != null) NativeMethods.SetWindowPos(helper.Handle, NativeMethods.GetWindow(_desktopManager.ShellWindow.Handle, NativeMethods.GetWindow_Cmd.GW_HWNDPREV), 0, 0, 0, 0, (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE);
            else if (_desktopManager.DesktopWindow != null) NativeMethods.SetWindowPos(helper.Handle, NativeMethods.GetWindow(_desktopManager.DesktopWindow.Handle, NativeMethods.GetWindow_Cmd.GW_HWNDPREV), 0, 0, 0, 0, (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE);
        }

        private void ResetPositionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SetPositionDefault(WindowManager.PrimaryMonitorSize.Width, WindowManager.PrimaryMonitorSize.Height);
        }
        #endregion

        #region Button clicks
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateBackward();
        }

        private void btnBack_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            NavigationManager.NavigateToParent(_desktopManager.DesktopLocation);
            e.Handled = true;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateToParent(_desktopManager.DesktopLocation);
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.NavigateHome();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (isShiftKeyHeld)
            {
                ShowGoToFolderDialog();
                return;
            }

            try
            {
                using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = Localization.DisplayString.sDesktop_BrowseTitle,
                    ShowNewFolderButton = false,
                    SelectedPath = NavigationManager.CurrentItem.Path
                })
                {
                    NativeMethods.SetForegroundWindow(helper.Handle); // bring browse window to front
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        if (Directory.Exists(fbd.SelectedPath))
                            NavigationManager.NavigateTo(fbd.SelectedPath);
                }
            }
            catch (Exception exception)
            {
                ShellLogger.Warning($"DesktopNavigationToolbar: Exception in FolderBrowserDialog: {exception.Message}");
            }
        }
        
        private void btnHome_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            homeContextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void btnBrowse_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NavigationManager.PathHistory.Count > 0)
            {
                browseContextMenu.Items.Clear();

                for (int i = 0; i < NavigationManager.PathHistory.Count; i++)
                {
                    string displayName = NavigationManager.PathHistory[i].DisplayName;

                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = NavigationManager.PathHistory[i].Path;
                    }
                    
                    MenuItem locationMenuItem = new MenuItem();
                    locationMenuItem.Header = displayName.Replace("_", "__");
                    locationMenuItem.ToolTip = new ToolTip {Content = NavigationManager.PathHistory[i].Path};
                    locationMenuItem.Tag = i;
                    locationMenuItem.Click += LocationMenuItem_Click;

                    browseContextMenu.Items.Add(locationMenuItem);
                }

                browseContextMenu.Items.Add(new Separator());

                MenuItem clearHistoryMenuItem = new MenuItem { Header = Localization.DisplayString.sDesktop_ClearHistory };
                clearHistoryMenuItem.Click += ClearHistoryMenuItem_Click;

                browseContextMenu.Items.Add(clearHistoryMenuItem);

                MenuItem goToFolderMenuItem = new MenuItem { Header = Localization.DisplayString.sDesktop_GoToFolderMenu };
                goToFolderMenuItem.Click += GoToFolderMenuItem_Click;

                browseContextMenu.Items.Add(goToFolderMenuItem);

                browseContextMenu.IsOpen = true;

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
            Settings.Instance.DesktopDirectory = NavigationManager.CurrentItem.Path;
        }
        #endregion

        #region Path history menu
        private void ClearHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Clear();
        }

        private void LocationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
                if (menuItem.Tag is int index)
                    NavigationManager.NavigateToIndex(index);
        }

        private void GoToFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowGoToFolderDialog();
        }

        private void ShowGoToFolderDialog()
        {
            Common.MessageControls.Input inputControl = new Common.MessageControls.Input();
            inputControl.Initialize(NavigationManager.CurrentItem.Path);

            CairoMessage.ShowControl(Localization.DisplayString.sDesktop_GoToFolderMessage,
                Localization.DisplayString.sDesktop_GoToFolderTitle,
                CairoMessageImage.Default,
                inputControl,
                Localization.DisplayString.sInterface_Go,
                Localization.DisplayString.sInterface_Cancel,
                (bool? result) =>
                {
                    string path = inputControl.InputField.Text;
                    if (result != true || string.IsNullOrEmpty(path))
                    {
                        return;
                    }

                    // Check if this is a valid folder
                    ShellFolder shellFolder = new ShellFolder(path, IntPtr.Zero);
                    bool valid = shellFolder.Loaded;
                    shellFolder.Dispose();

                    if (valid)
                    {
                        NavigationManager.NavigateTo(path);
                    }
                    else
                    {
                        CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo,
                            Localization.DisplayString.sError_OhNo,
                            MessageBoxButton.OK,
                            CairoMessageImage.Error, (bool? errResult) =>
                            {
                                ShowGoToFolderDialog();
                            });
                    }
                });
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
                if (!_desktopManager.IsOverlayOpen)
                {
                    // if the overlay isn't open, we always want to be on the bottom. modify the WINDOWPOS structure so that we always go to right above the desktop.
                    // the desktop will do the work of bringing us to the bottom.

                    // Extract the WINDOWPOS structure corresponding to this message
                    NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                    // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                    if ((wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                    {
                        // if the overlay is not open, we want to be right above the desktop. otherwise, we want to be right above the overlay.
                        IntPtr ownerWnd = IntPtr.Zero;
                        if (_desktopManager.ShellWindow != null)
                        {
                            ownerWnd = _desktopManager.ShellWindow.Handle;
                        }
                        else if (_desktopManager.DesktopWindow != null)
                        {
                            ownerWnd = _desktopManager.AllowProgmanChild ? WindowHelper.GetLowestDesktopParentHwnd() : _desktopManager.DesktopWindow.Handle;
                        }

                        wndPos.hwndInsertAfter = NativeMethods.GetWindow(ownerWnd, NativeMethods.GetWindow_Cmd.GW_HWNDPREV);
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
            else if (msg == (int)NativeMethods.WM.DPICHANGED || msg == (int)NativeMethods.WM.DISPLAYCHANGE)
            {
                SetPosition();
            }

            return IntPtr.Zero;
        }

        private void DesktopToolbar_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(new HwndSourceHook(WndProc));
            WindowHelper.HideWindowFromTasks(helper.Handle);
        }

        private void DesktopToolbar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    break;
                case MouseButton.Middle:
                    // Maybe Navigate Home?
                    break;
                case MouseButton.Right:
                    break;

                case MouseButton.XButton1:
                    NavigationManager.NavigateBackward();
                    break;
                case MouseButton.XButton2:
                    NavigationManager.NavigateForward();
                    break;
            }
        }

        private void DesktopNavigationToolbar_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            toolbarContextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void DesktopToolbar_Closing(object sender, CancelEventArgs e)
        {

            if (!_cairoApplication.IsShuttingDown && !AllowClose)
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
        #endregion

        #region Drag
        private void DesktopToolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void DesktopToolbar_LocationChanged(object sender, EventArgs e)
        {
            Settings.Instance.DesktopNavigationToolbarLocation = new Point(Left * DpiHelper.DpiScale, Top * DpiHelper.DpiScale);
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