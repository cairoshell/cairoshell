
namespace CairoDesktop
{
    using System;
    using System.Windows.Media;

    /// <summary>
    /// TrayIcon class representing a notification icon in the system tray.
    /// </summary>
    public class TrayIcon : IEquatable<TrayIcon>
    {
        /// <summary>
        /// Initializes a new instance of the TrayIcon class with no hwnd.
        /// </summary>
        public TrayIcon() : this(IntPtr.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the TrayIcon class with the specified hWnd.
        /// </summary>
        /// <param name="hWnd">The window handle of the icon.</param>
        public TrayIcon(IntPtr hWnd)
        {
            this.HWnd = hWnd;
        }

        /// <summary>
        /// Gets or sets the Icon's image.
        /// </summary>
        public ImageSource Icon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Icon's title (tool tip).
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pointer to the menu.
        /// </summary>
        public IntPtr Menu
        {
            get;
            set;
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

        #region IEquatable<TrayIcon> Members

        /// <summary>
        /// Checks the equality of the icon based on the hWnd and uID;
        /// </summary>
        /// <param name="other">The other TrayIcon to compare to.</param>
        /// <returns>Indication of equality.</returns>
        public bool Equals(TrayIcon other)
        {
            return this.HWnd.Equals(other.HWnd) && this.UID.Equals(other.UID);
        }

        #endregion
    }
}
