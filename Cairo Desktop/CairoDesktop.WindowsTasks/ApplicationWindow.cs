using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using CairoDesktop.Interop;
using CairoDesktop.AppGrabber;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using CairoDesktop.Common;
using System.Threading;
using CairoDesktop.Common.Logging;

namespace CairoDesktop.WindowsTasks
{
    [DebuggerDisplay("Title: {Title}, Handle: {Handle}")]
    public class ApplicationWindow : IEquatable<ApplicationWindow>, INotifyPropertyChanged
    {
        public DispatcherTimer VisCheck;

        public ApplicationWindow(IntPtr handle, WindowsTasksService sourceService)
        {
            Handle = handle;
            State = WindowState.Inactive;

            if (sourceService != null)
            {
                TasksService = sourceService;
            }

            if (Configuration.Settings.EnableTaskbarPolling)
            {
                VisCheck = new DispatcherTimer(new TimeSpan(0, 0, 2), DispatcherPriority.Background, delegate
                {
                    // some windows don't send a redraw notification after a property changes, try to catch those cases here
                    OnPropertyChanged("Title");
                    OnPropertyChanged("ShowInTaskbar");
                }, Application.Current.Dispatcher);
            }
        }

        public ApplicationWindow(IntPtr handle) : this(handle, null)
        {
        }

        public IntPtr Handle
        {
            get;
            set;
        }

        public WindowsTasksService TasksService
        {
            get;
            set;
        }

        private string _appUserModelId = "";

        public string AppUserModelID
        {
            get
            {
                if (string.IsNullOrEmpty(_appUserModelId))
                {
                    NativeMethods.IPropertyStore propStore;
                    var g = new Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");
                    NativeMethods.SHGetPropertyStoreForWindow(Handle, ref g, out propStore);

                    NativeMethods.PropVariant prop;

                    NativeMethods.PROPERTYKEY PKEY_AppUserModel_ID = new NativeMethods.PROPERTYKEY
                    {
                        fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),
                        pid = 5
                    };

                    if (propStore != null)
                    {
                        propStore.GetValue(PKEY_AppUserModel_ID, out prop);

                        try
                        {
                            _appUserModelId = prop.Value.ToString();
                        }
                        catch
                        {
                            _appUserModelId = "";
                        }

                        prop.Clear();
                    }
                }

                return _appUserModelId;
            }
        }

        private bool isUWP
        {
            get
            {
                return WinFileName.ToLower().Contains("applicationframehost.exe");
            }
        }

        private string _winFileName = "";

        public string WinFileName
        {
            get
            {
                if(string.IsNullOrEmpty(_winFileName))
                {
                    _winFileName = GetFileNameForWindow(Handle);
                }

                return _winFileName;
            }
        }

        private string _category;

        public string Category
        {
            get
            {
                if(_category == null)
                {
                    string backupCategory = "";
                    foreach (ApplicationInfo ai in AppGrabber.AppGrabber.Instance.CategoryList.FlatList)
                    {
                        if (ai.Target.ToLower() == WinFileName.ToLower() || (isUWP && ai.Target == AppUserModelID))
                        {
                            _category = ai.Category.DisplayName;
                            break;
                        }
                        else if (Title.ToLower().Contains(ai.Name.ToLower()))
                        {
                            backupCategory = ai.Category.DisplayName;
                        }
                    }

                    if (_category == null && WinFileName.ToLower().Contains("cairodesktop.exe"))
                        _category = "Cairo";
                    else if (_category == null && !string.IsNullOrEmpty(backupCategory))
                        _category = backupCategory;
                    else if (_category == null && WinFileName.ToLower().Contains("\\windows\\") && !isUWP)
                        _category = "Windows";
                    else if (_category == null)
                        _category = Localization.DisplayString.sAppGrabber_Uncategorized;
                }
                return _category;
            }
            set
            {
                _category = value;
            }
        }

        public ApplicationInfo QuickLaunchAppInfo
        {
            get
            {
                // it would be nice to cache this, but need to handle case of user adding/removing app via various means after first access
                foreach (ApplicationInfo ai in AppGrabber.AppGrabber.Instance.QuickLaunch)
                {
                    if (ai.Target.ToLower() == WinFileName.ToLower() || (isUWP && ai.Target == AppUserModelID))
                    {
                        return ai;
                    }
                    else if (Title.ToLower().Contains(ai.Name.ToLower()))
                    {
                        return ai;
                    }
                }

                return null;
            }
        }

        public static string GetFileNameForWindow(IntPtr hWnd)
        {
            // get process id
            uint procId;
            NativeMethods.GetWindowThreadProcessId(hWnd, out procId);

            // open process
            // QueryLimitedInformation flag allows us to access elevated applications as well
            IntPtr hProc = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.QueryLimitedInformation, false, (int)procId);

            // get filename
            StringBuilder outFileName = new StringBuilder(1024);
            int len = outFileName.Capacity;
            NativeMethods.QueryFullProcessImageName(hProc, 0, outFileName, ref len);

            outFileName.Replace("Excluded,", "");
            outFileName.Replace(",SFC protected", "");

            return outFileName.ToString();
        }

        public string Title
        {
            get
            {
                try
                {
                    int len = NativeMethods.GetWindowTextLength(Handle);

                    if (len < 1)
                        return "";

                    StringBuilder sb = new StringBuilder(len);
                    NativeMethods.GetWindowText(Handle, sb, len + 1);

                    return sb.ToString();
                }
                catch
                {
                    return "";
                }
            }
        }

        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                    SetIcon();

                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private bool _iconLoading = false;

        private ImageSource _icon = null;

        private WindowState _state;

        public WindowState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (value == WindowState.Active) hasActivated = true;

                _state = value;
                OnPropertyChanged("State");
            }
        }

        public bool IsMinimized
        {
            get { return NativeMethods.IsIconic(Handle); }
        }

        public NativeMethods.WindowShowStyle ShowStyle
        {
            get { return GetWindowShowStyle(Handle); }
        }

        public int WindowStyles
        {
            get
            {
                return NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE);
            }
        }

        public int ExtendedWindowStyles
        {
            get
            {
                return NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
            }
        }

        // set to true the first time the window state becomes active
        private bool hasActivated = false;

        // True if this window should be shown in the taskbar
        public bool ShowInTaskbar
        {
            get
            {
                // Don't show empty buttons.
                if (string.IsNullOrEmpty(Title))
                {
                    return false;
                }

                if ((State == WindowState.Hidden) || (NativeMethods.GetParent(Handle) != IntPtr.Zero))
                {
                    return false;
                }

                /* EnumWindows and ShellHook return UWP app windows that are 'cloaked', which should not be visible in the taskbar.
                 * The DWMA_CLOAKED attribute is set sometimes even when a window should be shown, so skip this check if the window has been activated. */
                if ((WindowsTasksService.IsStarting || !hasActivated) && Shell.IsWindows8OrBetter)
                {
                    uint cloaked;
                    int cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(uint));
                    NativeMethods.DwmGetWindowAttribute(Handle, NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out cloaked, cbSize);

                    if (cloaked > 0)
                    {
                        CairoLogger.Instance.Debug(string.Format("Cloaked ({0}) window ({1}) hidden from taskbar", cloaked, Title));
                        return false;
                    }
                }

                // Make sure this is a real application window and not a child or tool window
                IntPtr ownerWin = NativeMethods.GetWindow(Handle, NativeMethods.GW_OWNER);

                bool isAppWindow = (ExtendedWindowStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_APPWINDOW) != 0;
                bool hasEdge = (ExtendedWindowStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_WINDOWEDGE) != 0;
                bool isTopmostOnly = ExtendedWindowStyles == (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOPMOST;
                bool isToolWindow = (ExtendedWindowStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW) != 0;
                bool isAcceptFiles = (ExtendedWindowStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_ACCEPTFILES) != 0;
                bool isVisible = NativeMethods.IsWindowVisible(Handle);

                if ((isAppWindow || ((hasEdge || isTopmostOnly || ExtendedWindowStyles == 0) && ownerWin == IntPtr.Zero) || (isAcceptFiles && ShowStyle == NativeMethods.WindowShowStyle.ShowMaximized && ownerWin == IntPtr.Zero)) && !isToolWindow && isVisible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        int iconTries = 0;

        private void getIcon_Tick(object sender, EventArgs e)
        {
            if (_icon != null || iconTries > 5)
            {
                (sender as DispatcherTimer).Stop();
            }
            else
            {
                iconTries++;
                SetIcon();
            }
        }

        public void SetIcon()
        {
            if (!_iconLoading)
            {
                _iconLoading = true;

                var thread = new Thread(() =>
                {
                    if (isUWP && !string.IsNullOrEmpty(AppUserModelID))
                    {
                        // UWP apps
                        try
                        {
                            BitmapImage img = new BitmapImage();
                            img.BeginInit();
                            img.UriSource = new Uri(UWPInterop.StoreAppHelper.GetAppIcon(AppUserModelID, Configuration.Settings.TaskbarIconSize)[0], UriKind.Absolute);
                            img.CacheOption = BitmapCacheOption.OnLoad;
                            img.EndInit();
                            img.Freeze();
                            Icon = img;
                        }
                        catch
                        {
                            if (_icon == null) Icon = IconImageConverter.GetDefaultIcon();
                        }
                    }
                    else
                    {
                        // non-UWP apps
                        IntPtr hIco = default;
                        uint WM_GETICON = (uint)NativeMethods.WM.GETICON;
                        uint WM_QUERYDRAGICON = (uint)NativeMethods.WM.QUERYDRAGICON;
                        int GCL_HICON = -14;
                        int GCL_HICONSM = -34;
                        int sizeSetting = Configuration.Settings.TaskbarIconSize;

                        if (sizeSetting == 1)
                        {
                            NativeMethods.SendMessageTimeout(Handle, WM_GETICON, 2, 0, 2, 1000, ref hIco);
                            if (hIco == IntPtr.Zero)
                                NativeMethods.SendMessageTimeout(Handle, WM_GETICON, 0, 0, 2, 1000, ref hIco);
                        }
                        else
                        {
                            NativeMethods.SendMessageTimeout(Handle, WM_GETICON, 1, 0, 2, 1000, ref hIco);
                        }

                        if (hIco == IntPtr.Zero && sizeSetting == 1)
                        {
                            if (!Environment.Is64BitProcess)
                                hIco = NativeMethods.GetClassLong(Handle, GCL_HICONSM);
                            else
                                hIco = NativeMethods.GetClassLongPtr(Handle, GCL_HICONSM);
                        }

                        if (hIco == IntPtr.Zero)
                        {
                            if (!Environment.Is64BitProcess)
                                hIco = NativeMethods.GetClassLong(Handle, GCL_HICON);
                            else
                                hIco = NativeMethods.GetClassLongPtr(Handle, GCL_HICON);
                        }

                        if (hIco == IntPtr.Zero)
                        {
                            NativeMethods.SendMessageTimeout(Handle, WM_QUERYDRAGICON, 0, 0, 0, 1000, ref hIco);
                        }

                        if (hIco == IntPtr.Zero && _icon == null)
                        {
                            // last resort: find icon by executable. if we already have an icon from a previous fetch, then just skip this
                            if (Shell.Exists(WinFileName))
                            {
                                int size = 1;
                                if (sizeSetting != 1)
                                    size = 0;

                                hIco = Shell.GetIconByFilename(WinFileName, size);
                            }
                        }

                        if (hIco != IntPtr.Zero)
                        {
                            bool returnDefault = (_icon == null); // only return a default icon if we don't already have one. otherwise let's use what we have.
                            ImageSource icon = IconImageConverter.GetImageFromHIcon(hIco, returnDefault);
                            if (icon != null)
                            {
                                icon.Freeze();
                                Icon = icon;
                            }
                        }
                        else if (Configuration.Settings.EnableTaskbarPolling && iconTries == 0)
                        {
                            DispatcherTimer getIcon = new DispatcherTimer(DispatcherPriority.Background, Application.Current.Dispatcher);
                            getIcon.Interval = new TimeSpan(0, 0, 2);
                            getIcon.Tick += getIcon_Tick;
                            getIcon.Start();
                        }
                    }

                    _iconLoading = false;
                });
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }
        
        public void BringToFront()
        {
            // call restore if window is minimized
            if (IsMinimized)
            {
                Restore();
            }
            else
            {
                NativeMethods.ShowWindow(Handle, NativeMethods.WindowShowStyle.Show);
                NativeMethods.SetForegroundWindow(Handle);
            }
        }

        public void Minimize()
        {
            if ((WindowStyles & (int)NativeMethods.WindowStyles.WS_MINIMIZEBOX) != 0)
            {
                bool minimizeResult = NativeMethods.ShowWindow(Handle, NativeMethods.WindowShowStyle.Minimize);
                if (!minimizeResult)
                {
                    // elevated windows require WM_SYSCOMMAND messages
                    IntPtr retval = IntPtr.Zero;
                    NativeMethods.SendMessageTimeout(Handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.SC_MINIMIZE, 0, 2, 200, ref retval);
                }
            }
        }

        public void Restore()
        {
            bool restoreResult = NativeMethods.ShowWindow(Handle, NativeMethods.WindowShowStyle.Restore);
            if (!restoreResult)
            {
                // elevated windows require WM_SYSCOMMAND messages
                IntPtr retval = IntPtr.Zero;
                NativeMethods.SendMessageTimeout(Handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.SC_RESTORE, 0, 2, 200, ref retval);
            }
            NativeMethods.SetForegroundWindow(Handle);
        }

        public void Maximize()
        {
            bool maximizeResult = NativeMethods.ShowWindow(Handle, NativeMethods.WindowShowStyle.Maximize);
            if (!maximizeResult)
            {
                // we don't have a fallback for elevated windows here since our only hope, SC_MAXIMIZE, doesn't seem to work for them. fall back to restore.
                IntPtr retval = IntPtr.Zero;
                NativeMethods.SendMessageTimeout(Handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.SC_RESTORE, 0, 2, 200, ref retval);
            }
            NativeMethods.SetForegroundWindow(Handle);
        }

        public void Close()
        {
            IntPtr retval = IntPtr.Zero;
            NativeMethods.SendMessageTimeout(Handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.WM_CLOSE, 0, 2, 200, ref retval);

            if (retval != IntPtr.Zero)
            {
                CairoLogger.Instance.Debug(string.Format("Removing window {0} from collection due to no response", Title));
                TasksService.Windows.Remove(this);
            }
        }

        public void PinToQuickLaunch()
        {
            if (isUWP)
            {
                // store app, do special stuff
                AppGrabber.AppGrabber.Instance.AddStoreApp(AppUserModelID, AppCategoryType.QuickLaunch);
            }
            else
            {
                AppGrabber.AppGrabber.Instance.AddByPath(new string[] { WinFileName }, AppCategoryType.QuickLaunch);
            }
        }

        /// <summary>
        /// Returns whether a window is normal (1), minimized (2), or maximized (3).
        /// </summary>
        /// <param name="hWnd">The handle of the window.</param>
        public NativeMethods.WindowShowStyle GetWindowShowStyle(IntPtr hWnd)
        {
            NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
            NativeMethods.GetWindowPlacement(hWnd, ref placement);
            return placement.showCmd;
        }

        #region IEquatable<Window> Members

        public bool Equals(ApplicationWindow other)
        {
            return Handle.Equals(other.Handle);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        #endregion

        public enum WindowState
        {
            Active,
            Inactive,
            Hidden,
            Flashing,
            Unknown = 999
        }
    }

}
