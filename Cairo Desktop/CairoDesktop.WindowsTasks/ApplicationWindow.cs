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

namespace CairoDesktop.WindowsTasks
{
    [DebuggerDisplay("Title: {Title}, Handle: {Handle}")]
    public class ApplicationWindow : IEquatable<ApplicationWindow>, INotifyPropertyChanged
    {
        public DispatcherTimer VisCheck;

        public ApplicationWindow(IntPtr handle, WindowsTasksService sourceService)
        {
            this.Handle = handle;
            this.State = WindowState.Inactive;
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
                    uint cchLen = 256;
                    StringBuilder appUserModelID = new StringBuilder((int)cchLen);

                    NativeMethods.IPropertyStore propStore;
                    var g = new Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");
                    int result = NativeMethods.SHGetPropertyStoreForWindow(this.Handle, ref g, out propStore);

                    NativeMethods.PropVariant prop;

                    NativeMethods.PROPERTYKEY PKEY_AppUserModel_ID = new NativeMethods.PROPERTYKEY();
                    PKEY_AppUserModel_ID.fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");
                    PKEY_AppUserModel_ID.pid = 5;

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

                return _appUserModelId;
            }
        }

        private string _winFileName = "";

        public string WinFileName
        {
            get
            {
                if(string.IsNullOrEmpty(_winFileName))
                {
                    _winFileName = GetFileNameForWindow(this.Handle);
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
                        if (ai.Target == WinFileName || (WinFileName.ToLower().Contains("applicationframehost.exe") && ai.Target == AppUserModelID))
                        {
                            _category = ai.Category.DisplayName;
                            break;
                        }
                        else if (this.Title.ToLower().Contains(ai.Name.ToLower()))
                        {
                            backupCategory = ai.Category.DisplayName;
                        }
                    }

                    if (_category == null && WinFileName.ToLower().Contains("cairodesktop.exe"))
                        _category = "Cairo";
                    else if (_category == null && !string.IsNullOrEmpty(backupCategory))
                        _category = backupCategory;
                    else if (_category == null && WinFileName.ToLower().Contains("\\windows\\") && !WinFileName.ToLower().Contains("applicationframehost.exe"))
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

        public static string GetFileNameForWindow(IntPtr hWnd)
        {
            // get process id
            uint procId;
            NativeMethods.GetWindowThreadProcessId(hWnd, out procId);

            // open process
            IntPtr hProc = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.QueryInformation | NativeMethods.ProcessAccessFlags.VirtualMemoryRead, false, (int)procId);

            // get filename
            StringBuilder outFileName = new StringBuilder(1024);
            NativeMethods.GetModuleFileNameEx(hProc, IntPtr.Zero, outFileName, outFileName.Capacity);

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
                    int len = NativeMethods.GetWindowTextLength(this.Handle);
                    StringBuilder sb = new StringBuilder(len);
                    NativeMethods.GetWindowText(this.Handle, sb, len + 1);

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
                _state = value;
                OnPropertyChanged("State");
            }
        }

        public int Placement
        {
            get { return GetWindowPlacement(this.Handle); }
        }

        // True if this window should be shown in the taskbar
        public bool ShowInTaskbar
        {
            get
            {
                // Don't show empty buttons.
                if (string.IsNullOrEmpty(this.Title))
                {
                    return false;
                }

                if ((this.State == WindowState.Hidden) || (NativeMethods.GetParent(this.Handle) != IntPtr.Zero))
                {
                    return false;
                }

                /* When starting, WindowsTasksService calls EnumWindows to get the currently open windows. However, this shows suspended UWP apps. Check if we should hide the app here during startup. */
                if (WindowsTasksService.IsStarting && Shell.IsWindows8OrBetter)
                {
                    bool cloaked;
                    int cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(bool));
                    NativeMethods.DwmGetWindowAttribute(this.Handle, NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out cloaked, cbSize);

                    if (cloaked)
                    {
                        Trace.WriteLine(string.Format("Cloaked window ({0}) hidden from taskbar", this.Title));
                        return false;
                    }
                }

                // Make sure this is a real application window and not a child or tool window
                int exStyles = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_EXSTYLE);
                IntPtr ownerWin = NativeMethods.GetWindow(this.Handle, NativeMethods.GW_OWNER);

                bool isAppWindow = (exStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_APPWINDOW) != 0;
                bool hasEdge = (exStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_WINDOWEDGE) != 0;
                bool isTopmostOnly = exStyles == (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOPMOST;
                bool isToolWindow = (exStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW) != 0;
                bool isVisible = NativeMethods.IsWindowVisible(this.Handle);

                if ((isAppWindow || ((hasEdge || isTopmostOnly || exStyles == 0) && ownerWin == IntPtr.Zero)) && !isToolWindow && isVisible)
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
            if (this._icon != null || iconTries > 5)
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
                    if (WinFileName.Contains("ApplicationFrameHost.exe") && !string.IsNullOrEmpty(AppUserModelID))
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
                            Icon = IconImageConverter.GetDefaultIcon();
                        }
                    }
                    else
                    {
                        // non-UWP apps
                        IntPtr hIco = default(IntPtr);
                        uint WM_GETICON = 0x007f;
                        IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
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
                            string winFileName = GetFileNameForWindow(Handle);
                            if (Shell.Exists(winFileName))
                            {
                                int size = 1;
                                if (sizeSetting != 1)
                                    size = 0;

                                hIco = Shell.GetIconByFilename(winFileName, size);
                            }
                        }

                        if (hIco != IntPtr.Zero)
                        {
                            ImageSource icon = IconImageConverter.GetImageFromHIcon(hIco);
                            icon.Freeze();
                            Icon = icon;
                        }
                        else if (iconTries == 0)
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
            // so that maximized windows stay that way
            if(Placement == 3)
                NativeMethods.ShowWindowAsync(this.Handle, NativeMethods.WindowShowStyle.Show);
            else
                NativeMethods.ShowWindowAsync(this.Handle, NativeMethods.WindowShowStyle.Restore);

            NativeMethods.SetForegroundWindow(this.Handle);
        }

        public void Minimize()
        {
            NativeMethods.ShowWindowAsync(this.Handle, NativeMethods.WindowShowStyle.Minimize);
        }

        public void Close()
        {
            IntPtr retval = IntPtr.Zero;
            NativeMethods.SendMessageTimeout(Handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.WM_CLOSE, 0, 2, 200, ref retval);

            if (retval != IntPtr.Zero)
            {
                Trace.WriteLine(string.Format("Removing window {0} from collection due to no response", this.Title));
                TasksService.Windows.Remove(this);
            }
        }

        /// <summary>
        /// Returns whether a window is normal (1), minimized (2), or maximized (3).
        /// </summary>
        /// <param name="hWnd">The handle of the window.</param>
        public int GetWindowPlacement(IntPtr hWnd)
        {
            NativeMethods.WINDOWPLACEMENT placement = new NativeMethods.WINDOWPLACEMENT();
            NativeMethods.GetWindowPlacement(hWnd, ref placement);
            return placement.showCmd;
        }

        #region IEquatable<Window> Members

        public bool Equals(ApplicationWindow other)
        {
            return this.Handle.Equals(other.Handle);
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
