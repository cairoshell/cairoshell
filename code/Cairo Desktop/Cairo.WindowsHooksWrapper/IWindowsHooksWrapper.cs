
namespace Cairo.WindowsHooksWrapper
{
    using System;

    /// <summary>
    /// Contract interface for the WindowsHooksWrapper implementations
    /// </summary>
    public interface IWindowsHooksWrapper
    {
        /// <summary>
        /// Sets the delegate for the system tray callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        void SetSystrayCallback(Delegate theDelegate);

        /// <summary>
        /// Sets the delegate for the task callback.
        /// </summary>
        /// <param name="theDelegate">The task callback delegate.</param>
        void SetTaskCallback(Delegate theDelegate);

        /// <summary>
        /// Initializes the system tray hooks.
        /// </summary>
        void InitializeSystray();

        /// <summary>
        /// Initializes the task hooks.
        /// </summary>
        void InitializeTask();

        /// <summary>
        /// Starts the system tray listener (send the TaskbarCreated message).
        /// </summary>
        void Run();

        /// <summary>
        /// Shuts down all the tasks.
        /// </summary>
        void ShutdownAll();

        /// <summary>
        /// Shuts down the task hooks.
        /// </summary>
        void ShutdownTask();

        /// <summary>
        /// Shuts down the system tray hooks.
        /// </summary>
        void ShutdownSystray();
    }
}
