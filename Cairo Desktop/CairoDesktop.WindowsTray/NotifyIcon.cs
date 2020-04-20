using CairoDesktop.Configuration;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Input;
using static CairoDesktop.Interop.NativeMethods;
using CairoDesktop.Common.Logging;
using System.Collections.Generic;
using System.Linq;

namespace CairoDesktop.WindowsTray
{
    /// <summary>
    /// NotifyIcon class representing a notification area icon.
    /// </summary>
    public class NotifyIcon : IEquatable<NotifyIcon>, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the TrayIcon class with no hwnd.
        /// </summary>
        public NotifyIcon() : this(IntPtr.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TrayIcon class with the specified hWnd.
        /// </summary>
        /// <param name="hWnd">The window handle of the icon.</param>
        public NotifyIcon(IntPtr hWnd)
        {
            HWnd = hWnd;
        }

        private ImageSource _icon;

        /// <summary>
        /// Gets or sets the Icon's image.
        /// </summary>
        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        private string _title;

        /// <summary>
        /// Gets or sets the Icon's title (tool tip).
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the path to the application that created the icon.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        private bool _isPinned;

        /// <summary>
        /// Gets or sets whether or not the icon is pinned.
        /// </summary>
        public bool IsPinned
        {
            get
            {
                return _isPinned;
            }
            private set
            {
                _isPinned = value;
                OnPropertyChanged();
            }
        }

        private int _pinOrder;

        /// <summary>
        /// Gets or sets the order index of the item in the pinned icons
        /// </summary>
        public int PinOrder
        {
            get
            {
                return _pinOrder;
            }
            private set
            {
                _pinOrder = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the owners handle.
        /// </summary>
        public IntPtr HWnd
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the callback message id.
        /// </summary>
        public uint CallbackMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the UID of the Icon.
        /// </summary>
        public uint UID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the GUID of the Icon.
        /// </summary>
        public Guid GUID
        {
            get;
            set;
        }

        public uint Version
        {
            get;
            set;
        }

        public Rect Placement
        {
            get;
            set;
        }

        private string Identifier
        {
            get
            {
                if (GUID != default) return GUID.ToString();
                else return Path + ":" + UID.ToString();
            }
        }

        public void Pin(int position)
        {
            bool updated = false;

            if (IsPinned && position != PinOrder)
            {
                // already pinned, just moving
                List<string> icons = Settings.Instance.PinnedNotifyIcons.ToList();
                icons.Remove(Identifier);
                icons.Insert(position, Identifier);
                Settings.Instance.PinnedNotifyIcons = icons.ToArray();
                updated = true;
            }
            else if (!IsPinned)
            {
                // newly pinned. welcome to the party!
                List<string> icons = Settings.Instance.PinnedNotifyIcons.ToList();
                icons.Insert(position, Identifier);
                Settings.Instance.PinnedNotifyIcons = icons.ToArray();
                updated = true;
            }

            if (updated)
            {
                // update all pinned icons
                foreach (NotifyIcon notifyIcon in NotificationArea.Instance.TrayIcons)
                {
                    notifyIcon.SetPinValues();
                }
            }
        }

        public void Unpin()
        {
            if (IsPinned)
            {
                List<string> icons = Settings.Instance.PinnedNotifyIcons.ToList();
                icons.Remove(Identifier);
                Settings.Instance.PinnedNotifyIcons = icons.ToArray();

                IsPinned = false;
                PinOrder = 0;

                // update all pinned icons
                foreach (NotifyIcon notifyIcon in NotificationArea.Instance.TrayIcons)
                {
                    notifyIcon.SetPinValues();
                }
            }
        }

        public void SetPinValues()
        {
            for (int i = 0; i < Settings.Instance.PinnedNotifyIcons.Length; i++)
            {
                string item = Settings.Instance.PinnedNotifyIcons[i].ToLower();
                if (item == GUID.ToString().ToLower() || (GUID == default && item == (Path.ToLower() + ":" + UID.ToString())))
                {
                    IsPinned = true;
                    PinOrder = i;
                    break;
                }
            }
        }

        #region Mouse events

        private DateTime _lastLClick = DateTime.Now;
        private DateTime _lastRClick = DateTime.Now;

        public void IconMouseEnter(uint mouse)
        {
            if (!IsWindow(HWnd))
            {
                NotificationArea.Instance.TrayIcons.Remove(this);
                return;
            }
            else
            {
                uint wparam = UID;

                if (Version > 3)
                    wparam = mouse;

                PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.MOUSEHOVER);

                if (Version > 3)
                    PostMessage(HWnd, CallbackMessage, wparam, NIN_POPUPOPEN);
            }
        }

        public void IconMouseLeave(uint mouse)
        {
            if (!IsWindow(HWnd))
            {
                NotificationArea.Instance.TrayIcons.Remove(this);
                return;
            }
            else
            {
                uint wparam = UID;

                if (Version > 3)
                    wparam = mouse;

                PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.MOUSELEAVE);

                if (Version > 3)
                    PostMessage(HWnd, CallbackMessage, wparam, NIN_POPUPCLOSE);
            }
        }

        public void IconMouseMove(uint mouse)
        {
            if (!IsWindow(HWnd))
            {
                NotificationArea.Instance.TrayIcons.Remove(this);
                return;
            }
            else
            {
                uint wparam = UID;

                if (Version > 3)
                    wparam = mouse;

                PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.MOUSEMOVE);
            }
        }

        public void IconMouseClick(MouseButton button, uint mouse, int doubleClickTime)
        {
            CairoLogger.Instance.Debug(string.Format("{0} mouse button clicked icon: {1}", button.ToString(), Title));

            uint wparam = UID;

            if (Version > 3)
                wparam = mouse;

            if (button == MouseButton.Left)
            {
                if (DateTime.Now.Subtract(_lastLClick).TotalMilliseconds <= doubleClickTime)
                {
                    PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.LBUTTONDBLCLK);
                }
                else
                {
                    PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.LBUTTONDOWN);
                }

                PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.LBUTTONUP);
                if (Version >= 4) PostMessage(HWnd, CallbackMessage, mouse, (NIN_SELECT | (UID << 16)));

                _lastLClick = DateTime.Now;
            }
            else if (button == MouseButton.Right)
            {
                if (DateTime.Now.Subtract(_lastRClick).TotalMilliseconds <= doubleClickTime)
                {
                    PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.RBUTTONDBLCLK);
                }
                else
                {
                    PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.RBUTTONDOWN);
                }

                PostMessage(HWnd, CallbackMessage, wparam, (uint)WM.RBUTTONUP);
                if (Version >= 4) PostMessage(HWnd, CallbackMessage, mouse, ((uint)WM.CONTEXTMENU | (UID << 16)));

                _lastRClick = DateTime.Now;
            }

            SetForegroundWindow(HWnd);
        }
        #endregion

        #region IEquatable<NotifyIcon> Members

        /// <summary>
        /// Checks the equality of the icon based on the hWnd and uID;
        /// </summary>
        /// <param name="other">The other NotifyIcon to compare to.</param>
        /// <returns>Indication of equality.</returns>
        public bool Equals(NotifyIcon other)
        {
            return HWnd.Equals(other.HWnd) && UID.Equals(other.UID);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion
    }
}
