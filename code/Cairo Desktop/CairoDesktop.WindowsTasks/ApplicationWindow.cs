using System;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;
using CairoDesktop.Interop;
using CairoDesktop.AppGrabber;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace CairoDesktop.WindowsTasks
{
    [DebuggerDisplay("Title: {Title}, Handle: {Handle}")]
    public class ApplicationWindow : IEquatable<ApplicationWindow>, ICairoNotifyPropertyChanged
    {
        public ApplicationWindow(IntPtr handle, WindowsTasksService sourceService)
        {
            this.Handle = handle;
            this.State = WindowState.Inactive;
            if (sourceService != null)
            {
                TasksService = sourceService;
                sourceService.Redraw += HandleRedraw;
            }

            DispatcherTimer visCheck = new DispatcherTimer(new TimeSpan(0, 0, 2), DispatcherPriority.Background, delegate
            {
                // some windows don't send a redraw notification after a property changes, try to catch those cases here
                OnPropertyChanged("Title");
                OnPropertyChanged("ShowInTaskbar");
            }, System.Windows.Application.Current.Dispatcher);
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

                    _appUserModelId = prop.Value.ToString();
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
                    foreach (ApplicationInfo ai in AppGrabber.AppGrabber.Instance.CategoryList.FlatList)
                    {
                        if(ai.Target == WinFileName || (WinFileName.Contains("ApplicationFrameHost.exe") && ai.Target == AppUserModelID))
                        {
                            _category = ai.Category.Name;
                            break;
                        }
                    }

                    if (_category == null)
                        _category = "Uncategorized";
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
                int len = NativeMethods.GetWindowTextLength(this.Handle);
                StringBuilder sb = new StringBuilder(len);
                NativeMethods.GetWindowText(this.Handle, sb, len + 1);

                return sb.ToString();
            }
        }

        public ImageSource Icon
        {
            get
            {
                if (WinFileName.Contains("ApplicationFrameHost.exe") && !string.IsNullOrEmpty(AppUserModelID))
                {
                    BitmapImage img = new BitmapImage();
                    try
                    {
                        img.BeginInit();
                        img.UriSource = new Uri(UWPInterop.StoreAppHelper.GetAppIcon(AppUserModelID)[0], UriKind.Absolute);
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.EndInit();
                        img.Freeze();
                        return img;
                    }
                    catch
                    {
                        return IconImageConverter.GetDefaultIcon();
                    }
                }

                if (this._icon != null)
                {
                    ImageSource icon = IconImageConverter.GetImageFromHIcon(this._icon.Handle);
                    icon.Freeze();
                    return icon;
                }

                return IconImageConverter.GetDefaultIcon();
            }
        }

        private Icon _icon
        {
            get
            {
                IntPtr iconHandle = GetIconForWindow(this.Handle);

                Icon ico = null;

                if (iconHandle != IntPtr.Zero)
                {
                    try
                    {
                        ico = System.Drawing.Icon.FromHandle(iconHandle);
                    }
                    catch
                    {
                        ico = null;
                    }
                }

                return ico;
            }
        }

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

                // Make sure this is a real application window and not a child or tool window
                int exStyles = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_EXSTYLE);
                IntPtr ownerWin = NativeMethods.GetWindow(this.Handle, NativeMethods.GW_OWNER);

                bool isAppWindow = (exStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_APPWINDOW) != 0;
                bool hasEdge = (exStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_WINDOWEDGE) != 0;
                bool isTopmostOnly = exStyles == (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOPMOST;
                bool isToolWindow = (exStyles & (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW) != 0;
                bool isVisible = NativeMethods.IsWindowVisible(this.Handle);

                if ((isAppWindow || hasEdge || isTopmostOnly || exStyles == 0) && ownerWin == IntPtr.Zero && !isToolWindow && isVisible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static IntPtr GetIconForWindow(IntPtr hWnd)
        {
            IntPtr hIco = default(IntPtr);
            uint WM_GETICON = 0x007f;
            IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
            int GCL_HICON = -14;
            int GCL_HICONSM = -34;

            NativeMethods.SendMessageTimeout(hWnd, WM_GETICON, 2, 0, 2, 1000, ref hIco);

            if (hIco == IntPtr.Zero)
            {
                NativeMethods.SendMessageTimeout(hWnd, WM_GETICON, 0, 0, 2, 1000, ref hIco);
            }

            if (hIco == IntPtr.Zero)
            {
                if(!Environment.Is64BitProcess)
                    hIco = NativeMethods.GetClassLong(hWnd, GCL_HICONSM);
                else
                    hIco = NativeMethods.GetClassLongPtr(hWnd, GCL_HICONSM);
            }

            if (hIco == IntPtr.Zero)
            {
                if (!Environment.Is64BitProcess)
                    hIco = NativeMethods.GetClassLong(hWnd, GCL_HICON);
                else
                    hIco = NativeMethods.GetClassLongPtr(hWnd, GCL_HICON);
            }

            if (hIco == IntPtr.Zero)
            {
                string winFileName = GetFileNameForWindow(hWnd);
                if (Shell.Exists(winFileName))
                    hIco = Shell.GetIconByFilename(winFileName, true);
            }

            return hIco;
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

        /// <summary>
        /// Handles calls to the refresh event of the source for this handle and notifies subscribers that the property has changed.
        /// </summary>
        /// <param name="handle">The handle of the window.</param>
        private void HandleRedraw(IntPtr handle)
        {
            if (!this.Handle.Equals(handle))
            {
                return;
            }

            //this trace spams the debugger into oblivion during file copy
            //Trace.WriteLine("Handling redraw call for handle " + handle.ToString());

            OnPropertyChanged("Title");
            OnPropertyChanged("Icon");
            OnPropertyChanged("ShowInTaskbar");
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
