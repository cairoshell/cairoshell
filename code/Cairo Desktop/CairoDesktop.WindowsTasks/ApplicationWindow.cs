using System;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;

namespace CairoDesktop.WindowsTasks
{
    [DebuggerDisplay("Title: {Title}, Handle: {Handle}")]
    public class ApplicationWindow : IEquatable<ApplicationWindow>, ICairoNotifyPropertyChanged
    {
        //private bool _isActive;

        public ApplicationWindow(IntPtr handle, WindowsTasksService sourceService)
        {
            this.Handle = handle;
            this.State = WindowState.Inactive;
            if (sourceService != null)
            {
                sourceService.Redraw += HandleRedraw;
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
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Title
        {
            get
            {
                int len = NativeWindowEx.GetWindowTextLength(this.Handle);
                StringBuilder sb = new StringBuilder(len);
                NativeWindowEx.GetWindowText(this.Handle, sb, len + 1);

                return sb.ToString();
            }
        }

        public bool IsAppResponding
        {
            get
            {
                return NativeWindowEx.IsAppHungWindow(this.Handle);
            }
        }

        public Icon Icon
        {
            get
            {
                IntPtr iconHandle = GetIconForWindow(this.Handle);

                Icon ico = null;
                if (iconHandle != IntPtr.Zero)
                {
                    try
                    {
                        ico = Icon.FromHandle(iconHandle);
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
            [NotifyPropertyChangedAspect("State")]

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

        public bool IsActive
        {
            get;
            [NotifyPropertyChangedAspect("IsActive")]
            set;
        }
        
        public bool Flash
        {
            get;
            [NotifyPropertyChangedAspect("Flash")]
            set;
        }

        // True if this window should be shown in the taskbar
        public bool ShowInTaskbar
        {
            get
            {
                if ((this.State == WindowState.Hidden) || (NativeWindowEx.GetParent(this.Handle) != IntPtr.Zero))
                {
                    return false;
                }

                // Don't show empty buttons.
                if (string.IsNullOrEmpty(this.Title))
                {
                    return false;
                }

                // Make sure this is a real application window and not a child or tool window
                int GWL_EXSTYLE = -0x14;
                int GWL_STYLE = -16;
                int GW_Owner = 4;

                int exStyles = NativeWindowEx.GetWindowLong(this.Handle, GWL_EXSTYLE);
                int style = NativeWindowEx.GetWindowLong(this.Handle, GWL_STYLE);
                IntPtr ownerWin = NativeWindowEx.GetWindow(this.Handle, GW_Owner);

                if (((exStyles & (int)ExtendedWindowStyles.WS_EX_APPWINDOW) != 0 || (exStyles & (int)ExtendedWindowStyles.WS_EX_WINDOWEDGE) != 0) && (ownerWin == IntPtr.Zero && (exStyles & (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW) == 0 && (style & (int)WindowStyles.WS_VISIBLE) == (int)WindowStyles.WS_VISIBLE))
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

            NativeWindowEx.SendMessageTimeout(hWnd, WM_GETICON, 2, 0, 2, 200, ref hIco);

            if (hIco == IntPtr.Zero)
            {
                NativeWindowEx.SendMessageTimeout(hWnd, WM_GETICON, 0, 0, 2, 200, ref hIco);
            }

            if (hIco == IntPtr.Zero)
            {
                if(!Environment.Is64BitProcess)
                    hIco = NativeWindowEx.GetClassLong(hWnd, GCL_HICONSM);
                else
                    hIco = NativeWindowEx.GetClassLongPtr(hWnd, GCL_HICONSM);
            }

            if (hIco == IntPtr.Zero)
            {
                if (!Environment.Is64BitProcess)
                    hIco = NativeWindowEx.GetClassLong(hWnd, GCL_HICON);
                else
                    hIco = NativeWindowEx.GetClassLongPtr(hWnd, GCL_HICON);
            }

            return hIco;
        }

        public void BringToFront()
        {
            // so that maximized windows stay that way
            if(Placement == 3)
                NativeWindowEx.ShowWindow(this.Handle, NativeWindowEx.WindowShowStyle.Show);
            else
                NativeWindowEx.ShowWindow(this.Handle, NativeWindowEx.WindowShowStyle.Restore);

            NativeWindowEx.SetForegroundWindow(this.Handle);
        }

        public void Minimize()
        {
            NativeWindowEx.ShowWindow(this.Handle, NativeWindowEx.WindowShowStyle.Minimize);
        }

        /// <summary>
        /// Returns whether a window is normal (1), minimized (2), or maximized (3).
        /// </summary>
        /// <param name="hWnd">The handle of the window.</param>
        public int GetWindowPlacement(IntPtr hWnd)
        {
            NativeWindowEx.WINDOWPLACEMENT placement = new NativeWindowEx.WINDOWPLACEMENT();
            NativeWindowEx.GetWindowPlacement(hWnd, ref placement);
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

            Trace.WriteLine("Handling redraw call for handle " + handle.ToString());

            OnPropertyChanged("Title");
            OnPropertyChanged("Icon");
            OnPropertyChanged("ShowInTaskbar");
        }

        #region P/Invoke Declarations
        
        #endregion

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
