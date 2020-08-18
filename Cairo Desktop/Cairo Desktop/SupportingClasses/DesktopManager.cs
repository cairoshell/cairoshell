using CairoDesktop.Common;
using CairoDesktop.Common.DesignPatterns;
using CairoDesktop.Common.Helpers;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace CairoDesktop.SupportingClasses
{
    public class DesktopManager : SingletonObject<DesktopManager>, INotifyPropertyChanged, IDisposable
    {
        #region Windows
        public Desktop DesktopWindow { get; private set; }
        public DesktopOverlay DesktopOverlayWindow { get; private set; }
        public DesktopNavigationToolbar DesktopToolbar { get; private set; }
        public NativeWindowEx ShellWindow { get; private set; }
        #endregion

        public static bool IsEnabled => Settings.Instance.EnableDesktop && !GroupPolicyManager.Instance.NoDesktop;

        #region Properties
        private bool isOverlayOpen;
        private int renderOverlayFrames;
        private HotKey overlayHotKey;
        private WindowManager windowManager;

        public DesktopIcons DesktopIconsControl { get; private set; }
        public NavigationManager NavigationManager { get; private set; }

        public bool IsOverlayOpen
        {
            get
            {
                return isOverlayOpen;
            }
            set
            {
                if (isOverlayOpen != value)
                {
                    isOverlayOpen = value;

                    if (value)
                    {
                        openOverlay();
                    }
                    else
                    {
                        closeOverlay();
                    }

                    OnPropertyChanged();
                }
            }
        }

        public bool AllowProgmanChild => ShellWindow == null && Shell.IsWindows8OrBetter; // doesn't work in win7 due to layered child window restrictions

        public SystemDirectory DesktopLocation => DesktopIconsControl.Location;

        #endregion

        private DesktopManager()
        {
            // DesktopManager is always created on startup by WindowManager, regardless of desktop preferences
            // this allows for dynamic creation and destruction of the desktop per user preference

            initDesktop();

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        private void initDesktop()
        {
            if (IsEnabled)
            {
                // hide the windows desktop
                Shell.ToggleDesktopIcons(false);

                // create the native shell window
                createShellWindow();

                // create navigation manager
                NavigationManager = new NavigationManager();
                NavigationManager.PropertyChanged += NavigationManager_PropertyChanged;

                // create icons control
                DesktopIconsControl = new DesktopIcons();

                // create desktop window
                createDesktopWindow();

                // register desktop overlay hotkey
                registerHotKey();
            }
        }

        private void createDesktopWindow()
        {
            if (DesktopWindow == null)
            {
                DesktopWindow = new Desktop(this);
                DesktopWindow.Show();
            }
        }

        private void createToolbar()
        {
            if (Settings.Instance.EnableDynamicDesktop && DesktopWindow != null && NavigationManager != null)
            {
                DesktopToolbar = new DesktopNavigationToolbar(this) { Owner = DesktopWindow, NavigationManager = NavigationManager };
                DesktopToolbar.Show();
            }
            else if (Settings.Instance.EnableDynamicDesktop)
            {
                CairoLogger.Instance.Warning("DesktopManager: Attempted to create toolbar with uninitialized properties");
            }
        }

        private void destroyDesktopWindow()
        {
            if (DesktopWindow != null)
            {
                if (DesktopToolbar != null) DesktopToolbar.Owner = null;

                DesktopWindow.grid.Children.Clear();
                DesktopWindow.AllowClose = true;
                DesktopWindow.Close();

                DesktopWindow = null;
            }
        }

        private void destroyToolbar()
        {
            if (DesktopToolbar != null)
            {
                DesktopToolbar.AllowClose = true;
                DesktopToolbar.Close();
                DesktopToolbar = null;
            }
        }

        private void teardownDesktop()
        {
            // close windows
            DesktopOverlayWindow?.Close();
            DesktopOverlayWindow = null;
            destroyToolbar();
            destroyDesktopWindow();

            // show the windows desktop
            Shell.ToggleDesktopIcons(true);

            // destroy the native shell window
            destroyShellWindow();

            // destroy the navigation manager
            NavigationManager = null;

            // destroy icons control
            DesktopIconsControl = null;

            // unregister desktop overlay hotkey
            unregisterHotKey();
        }

        private void registerHotKey()
        {
            if (Settings.Instance.EnableDesktopOverlayHotKey && overlayHotKey == null)
            {
                overlayHotKey = HotKeyManager.RegisterHotKey(Settings.Instance.DesktopOverlayHotKey, OnShowDesktop);
            }
        }

        private void unregisterHotKey()
        {
            overlayHotKey?.Dispose();
            overlayHotKey = null;
        }

        public void ConfigureDesktop()
        {
            if (DesktopWindow != null)
            {
                if (ShellWindow != null || AllowProgmanChild)
                {
                    // set the desktop window as a child of the shell window
                    NativeMethods.SetWindowLong(DesktopWindow.Handle, NativeMethods.GWL_STYLE, (NativeMethods.GetWindowLong(DesktopWindow.Handle, NativeMethods.GWL_STYLE) | (int)NativeMethods.WindowStyles.WS_CHILD) & ~unchecked((int)NativeMethods.WindowStyles.WS_OVERLAPPED));
                    NativeMethods.SetParent(DesktopWindow.Handle, ShellWindow != null ? ShellWindow.Handle : Shell.GetLowestDesktopChildHwnd());
                }

                // add the icons to the desktop grid and set initial directory
                if (DesktopIconsControl != null) DesktopWindow.grid.Children.Add(DesktopIconsControl);
                NavigationManager.NavigateHome();

                // set up the toolbar
                if (DesktopToolbar == null)
                {
                    createToolbar();
                }
                else
                {
                    DesktopToolbar.Owner = DesktopWindow;
                }
            }
            else
            {
                CairoLogger.Instance.Warning("DesktopManager: Attempted to configure desktop with uninitialized values");
            }
        }

        public void ResetPosition(bool displayChanged)
        {
            setShellWindowSize();

            DesktopOverlayWindow?.ResetPosition();

            if (displayChanged && DesktopWindow != null)
            {
                destroyDesktopWindow();
                createDesktopWindow();
            }
            else
            {
                DesktopWindow?.ResetPosition();
            }
        }

        public void SetWindowManager(WindowManager manager)
        {
            windowManager = manager;
            windowManager.ScreensChanged += ScreensChanged;
        }

        #region Shell window
        private void createShellWindow()
        {
            if (Shell.IsCairoRunningAsShell && ShellWindow == null)
            {
                // create native window; we must pass a native window to SetShellWindow
                ShellWindow = new NativeWindowEx();
                CreateParams cp = new CreateParams();
                cp.Style |= (int)NativeMethods.WindowStyles.WS_VISIBLE;
                cp.Style |= unchecked((int)NativeMethods.WindowStyles.WS_POPUP);
                cp.ExStyle |= (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                cp.Height = SystemInformation.VirtualScreen.Height;
                cp.Width = SystemInformation.VirtualScreen.Width;
                cp.X = SystemInformation.VirtualScreen.Left;
                cp.Y = SystemInformation.VirtualScreen.Top;

                ShellWindow.CreateHandle(cp);
                ShellWindow.MessageReceived += WndProc;

                if (NativeMethods.SetShellWindow(ShellWindow.Handle) == 1)
                {
                    // we did it
                    CairoLogger.Instance.Debug("DesktopManager: Successfully set as shell window");
                }
            }
        }

        private void setShellWindowSize()
        {
            if (ShellWindow != null)
            {
                NativeMethods.SetWindowPos(ShellWindow.Handle, IntPtr.Zero, SystemInformation.VirtualScreen.Left, SystemInformation.VirtualScreen.Top, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height, (int)NativeMethods.SetWindowPosFlags.SWP_NOZORDER);
            }
        }

        private void destroyShellWindow()
        {
            if (ShellWindow != null)
            {
                NativeMethods.DestroyWindow(ShellWindow.Handle);
                ShellWindow = null;
            }
        }

        private void WndProc(Message msg)
        {
            // Window procedure for the native window
            // Because other desktop windows are children, we need to pass them some received events.

            if (msg.Msg == (int)NativeMethods.WM.SETTINGCHANGE &&
                    msg.WParam.ToInt32() == (int)NativeMethods.SPI.SETWORKAREA)
            {
                OnSetWorkArea();
            }
            else if (msg.Msg == (int)NativeMethods.WM.SETTINGCHANGE &&
                    msg.WParam.ToInt32() == (int)NativeMethods.SPI.SETDESKWALLPAPER)
            {
                msg.Result = OnSetDeskWallpaper();
            }
        }
        #endregion

        #region Overlay
        private void openOverlay()
        {
            if (DesktopOverlayWindow == null && DesktopWindow != null && DesktopIconsControl != null)
            {
                DesktopOverlayWindow = new DesktopOverlay(this);

                // create mask image to show while the icons control is rendered on the overlay window
                Image maskImage = new Image();
                maskImage.Source = DesktopIconsControl?.GenerateBitmap(DesktopWindow.grid);

                // add the mask image to and show the overlay
                DesktopOverlayWindow.grid.Children.Add(maskImage);
                DesktopOverlayWindow.Show();
                DesktopOverlayWindow.BringToFront();

                // migrate the desktop icons control
                DesktopWindow.grid.Children.Clear();
                DesktopOverlayWindow.grid.Children.Add(DesktopIconsControl);

                // remove the mask image
                DesktopOverlayWindow.grid.Children.Remove(maskImage);

                // change toolbar owner
                if (DesktopToolbar != null)
                {
                    DesktopToolbar.Owner = DesktopOverlayWindow;
                    DesktopToolbar.BringToFront();
                }
            }
            else if (DesktopOverlayWindow != null)
            {
                CairoLogger.Instance.Debug("DesktopManager: Desktop overlay already open, ignoring");
            }
            else
            {
                CairoLogger.Instance.Warning("DesktopManager: Attempted to show desktop overlay using uninitialized properties");
            }
        }

        private void closeOverlay()
        {
            if (DesktopOverlayWindow != null && DesktopWindow != null && DesktopIconsControl != null)
            {
                // create mask image to show while the icons control is rendered on the desktop window
                Image maskImage = new Image();
                maskImage.Source = DesktopIconsControl?.GenerateBitmap(DesktopOverlayWindow.grid);

                // add the mask image to the desktop
                DesktopWindow.grid.Children.Add(maskImage);

                // change toolbar owner
                if (DesktopToolbar != null)
                {
                    DesktopToolbar.Owner = DesktopWindow;
                    DesktopToolbar.SendToBottom();
                }

                // setup render callback to hide overlay and continue once image is rendered
                renderOverlayFrames = 0;
                CompositionTarget.Rendering += CloseOverlay_CompositionTarget_Rendering;
            }
            else if (DesktopOverlayWindow == null)
            {
                CairoLogger.Instance.Debug("DesktopManager: Desktop overlay already closed, ignoring");
            }
            else
            {
                CairoLogger.Instance.Warning("DesktopManager: Attempted to close desktop overlay using uninitialized properties");
            }
        }

        public void ToggleOverlay()
        {
            IsOverlayOpen = !IsOverlayOpen;
        }

        private void CloseOverlay_CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // runs once per frame during overlay close

            // it generally takes 2 frames to render the mask image
            int waitFrames = 2;

            if (renderOverlayFrames == waitFrames)
            {
                // close the overlay window
                DesktopOverlayWindow.Close();

                // migrate the icons control to the desktop window
                DesktopOverlayWindow.grid.Children.Clear();
                DesktopWindow.grid.Children.Add(DesktopIconsControl);

                // once the control has been migrated, remove the mask image
                DesktopWindow.grid.Children.RemoveAt(0);

                // remove reference to the overlay window to allow GC
                DesktopOverlayWindow = null;

                // we're done here, stop this callback from executing again
                CompositionTarget.Rendering -= CloseOverlay_CompositionTarget_Rendering;
            }

            renderOverlayFrames++;
        }
        #endregion

        #region Event handling
        public void OnSetWorkArea()
        {
            ResetPosition(false);
        }

        public IntPtr OnSetDeskWallpaper()
        {
            DesktopWindow?.ReloadBackground();

            return new IntPtr(NativeMethods.MA_NOACTIVATE);
        }

        private void NavigationManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPath")
            {
                if (DesktopIconsControl.Location == null || DesktopIconsControl.Location.FullName != NavigationManager.CurrentPath)
                {
                    if (DesktopIconsControl.Location != null)
                    {
                        // dispose of current directory so that we don't keep a lock on it
                        DesktopIconsControl.Location.Dispose();
                    }

                    DesktopIconsControl.Location = new SystemDirectory(NavigationManager.CurrentPath, Dispatcher.CurrentDispatcher);
                }
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "EnableDesktop":
                        if (Settings.Instance.EnableDesktop)
                        {
                            initDesktop();
                        }
                        else
                        {
                            teardownDesktop();
                        }

                        break;
                    case "EnableDynamicDesktop" when IsEnabled:
                        if (Settings.Instance.EnableDynamicDesktop)
                        {
                            createToolbar();
                        }
                        else
                        {
                            destroyToolbar();
                            NavigationManager?.NavigateHome();
                        }

                        break;
                    case "EnableDesktopOverlayHotKey" when IsEnabled:
                        if (Settings.Instance.EnableDesktopOverlayHotKey)
                        {
                            registerHotKey();
                        }
                        else
                        {
                            unregisterHotKey();
                        }

                        break;
                    case "DesktopOverlayHotKey" when IsEnabled:
                        if (Settings.Instance.EnableDesktopOverlayHotKey)
                        {
                            unregisterHotKey();
                            registerHotKey();
                        }

                        break;
                }
            }
        }

        private void ScreensChanged(object sender, WindowManagerEventArgs e)
        {
            ResetPosition(e.DisplaysChanged);
        }

        private void OnShowDesktop(HotKey hotKey)
        {
            ToggleOverlay();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void Dispose()
        {
            teardownDesktop();
        }
    }
}
