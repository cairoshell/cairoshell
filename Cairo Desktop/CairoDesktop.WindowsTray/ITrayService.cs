
namespace CairoDesktop.WindowsTray
{
    using System;

    /// <summary>
    /// Contract interface for the TrayService implementations
    /// </summary>
    public interface ITrayService: IDisposable
    {
        /// <summary>
        /// Sets the delegate for the system tray callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        void SetSystrayCallback(SystrayDelegate theDelegate);

        /// <summary>
        /// Sets the delegate for the icon data callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        void SetIconDataCallback(IconDataDelegate theDelegate);

        /// <summary>
        /// Sets the delegate for the menu bar size callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        void SetMenuBarSizeCallback(MenuBarSizeDelegate theDelegate);

        /// <summary>
        /// Initializes the system tray hooks.
        /// </summary>
        IntPtr Initialize();

        /// <summary>
        /// Starts the system tray listener (send the TaskbarCreated message).
        /// </summary>
        void Run();
    }
}
