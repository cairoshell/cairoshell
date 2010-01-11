using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;

namespace CairoDesktop.WindowsTasks
{
    [DebuggerDisplay("Title: {Title}, Handle: {Handle}")]
    public class ApplicationWindow : IEquatable<ApplicationWindow>, ICairoNotifyPropertyChanged
    {
        private bool _isActive;

        public ApplicationWindow(IntPtr handle, WindowsTasksService sourceService)
        {
            this.Handle = handle;
            this.State = WindowState.Active;
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
                int len = GetWindowTextLength(this.Handle);
                StringBuilder sb = new StringBuilder(len);
                GetWindowText(this.Handle, sb, len + 1);

                return sb.ToString();
            }
        }

        public bool IsAppResponding
        {
            get
            {
                return IsAppHungWindow(this.Handle);
            }
        }

        public Icon Icon
        {
            get
            {
                uint iconHandle = GetIconForWindow();

                Icon ico = null;
                try
                {
                    ico = Icon.FromHandle(new IntPtr(iconHandle));
                }   
                catch (Exception ex)
                {
                    ico = null;
                }

                return ico;
            }
        }

        public WindowState State
        {
            get;
            [NotifyPropertyChangedAspect("State")]
            set;
        }

        public bool IsActive
        {
            get;
            [NotifyPropertyChangedAspect("IsActive")]
            set;
        }

        /// <summary>
        /// TODO: Implement the flash property - or should this be an event??
        /// Perhaps we can bind to a wpf animation enabled property??
        /// </summary>
        public bool Flash
        {
            get;
            [NotifyPropertyChangedAspect("Flash")]
            set;
        }

        // TODO: Implement show in taskbar property.
        public bool ShowInTaskbar
        {
            get
            {
                if ((this.State != WindowState.Hidden) && (GetParent(this.Handle) != IntPtr.Zero))
                {
                    return false;
                }

                // Don't show empty buttons.
                if (string.IsNullOrEmpty(this.Title))
                {
                    return false;
                }

                int GWL_EXSTYLE = -20;
                int WS_EX_TOOLWINDOW = 0x00000080;
                int GW_Owner = 4;

                int exStyles = GetWindowLong(this.Handle, GWL_EXSTYLE);

                IntPtr ownerWin = GetWindow(this.Handle, GW_Owner);
                // TODO: Add method to check that this isn't a child window...
                if ((((exStyles & WS_EX_TOOLWINDOW) == 0) && (ownerWin == IntPtr.Zero)) ||
                    (((exStyles & WS_EX_TOOLWINDOW) == 0) && (ownerWin != IntPtr.Zero)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private uint GetIconForWindow()
        {
            uint hIco = 0;
            SendMessageTimeout(this.Handle, 127, 2, 0, 2, 200, ref hIco);
            int GCL_HICONSM = -34;
            if(hIco == 0)
            {
                hIco = GetClassLong(this.Handle, GCL_HICONSM);
            }

            return hIco;
        }

        public void BringToFront()
        {
            ShowWindow(this.Handle, WindowShowStyle.Restore);
            SetForegroundWindow(this.Handle);
        }

        public void Minimize()
        {
            //...In your code some where: show a form, without making it active
            ShowWindow(this.Handle, WindowShowStyle.Minimize);
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
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder sb, int Length);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern bool IsAppHungWindow(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern uint GetClassLong(IntPtr handle, int longClass);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint SendMessageTimeout(IntPtr hWnd, uint messageId, uint wparam, uint lparam, uint timeoutFlags, uint timeout, ref uint retval);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr handle, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr handle, int wCmd);

        /// <summary>Shows a Window</summary>
        /// <remarks>
        /// <para>To perform certain special effects when showing or hiding a 
        /// window, use AnimateWindow.</para>
        ///<para>The first time an application calls ShowWindow, it should use 
        ///the WinMain function's nCmdShow parameter as its nCmdShow parameter. 
        ///Subsequent calls to ShowWindow must use one of the values in the 
        ///given list, instead of the one specified by the WinMain function's 
        ///nCmdShow parameter.</para>
        ///<para>As noted in the discussion of the nCmdShow parameter, the 
        ///nCmdShow value is ignored in the first call to ShowWindow if the 
        ///program that launched the application specifies startup information 
        ///in the structure. In this case, ShowWindow uses the information 
        ///specified in the STARTUPINFO structure to show the window. On 
        ///subsequent calls, the application must call ShowWindow with nCmdShow 
        ///set to SW_SHOWDEFAULT to use the startup information provided by the 
        ///program that launched the application. This behavior is designed for 
        ///the following situations: </para>
        ///<list type="">
        ///    <item>Applications create their main window by calling CreateWindow 
        ///    with the WS_VISIBLE flag set. </item>
        ///    <item>Applications create their main window by calling CreateWindow 
        ///    with the WS_VISIBLE flag cleared, and later call ShowWindow with the 
        ///    SW_SHOW flag set to make it visible.</item>
        ///</list></remarks>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="nCmdShow">Specifies how the window is to be shown. 
        /// This parameter is ignored the first time an application calls 
        /// ShowWindow, if the program that launched the application provides a 
        /// STARTUPINFO structure. Otherwise, the first time ShowWindow is called, 
        /// the value should be the value obtained by the WinMain function in its 
        /// nCmdShow parameter. In subsequent calls, this parameter can be one of 
        /// the WindowShowStyle members.</param>
        /// <returns>
        /// If the window was previously visible, the return value is nonzero. 
        /// If the window was previously hidden, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        /// <summary>Enumeration of the different ways of showing a window using 
        /// ShowWindow</summary>
        private enum WindowShowStyle : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized 
            /// or maximized, the system restores it to its original size and 
            /// position. An application should specify this flag when displaying 
            /// the window for the first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,
            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,
            /// <summary>Displays a window in its most recent size and position. 
            /// This value is similar to "ShowNormal", except the window is not 
            /// actived.</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,
            /// <summary>Activates the window and displays it in its current size 
            /// and position.</summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,
            /// <summary>Minimizes the specified window and activates the next 
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is 
            /// similar to "ShowMinimized", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,
            /// <summary>Displays the window in its current size and position. This 
            /// value is similar to "Show", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,
            /// <summary>Activates and displays the window. If the window is 
            /// minimized or maximized, the system restores it to its original size 
            /// and position. An application should specify this flag when restoring 
            /// a minimized window.</summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,
            /// <summary>Sets the show state based on the SW_ value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,
            /// <summary>Windows 2000/XP: Minimizes a window, even if the thread 
            /// that owns the window is hung. This flag should only be used when 
            /// minimizing windows from a different thread.</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }
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
