
namespace Cairo.WindowsHooksWrapper
{
    using System;

    /// <summary>
    /// The wrapper class the for C++ Windows Hooks library.
    /// </summary>
    public class WindowsHooksWrapper : IWindowsHooksWrapper
    {
        /// <summary>
        /// Sets the delegate for the system tray callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        public void SetSystrayCallback(Delegate theDelegate)
        {
            InteropCalls.SetSystrayCallback(theDelegate);
        }

        /// <summary>
        /// Sets the delegate for the task callback.
        /// </summary>
        /// <param name="theDelegate">The task callback delegate.</param>
        public void SetTaskCallback(Delegate theDelegate)
        {
            InteropCalls.SetTaskCallback(theDelegate);
        }

        /// <summary>
        /// Initializes the system tray hooks.
        /// </summary>
        public void InitializeSystray()
        {
            InteropCalls.InitializeSystray();
        }

        /// <summary>
        /// Initializes the task hooks.
        /// </summary>
        public void InitializeTask()
        {
            InteropCalls.InitializeTask();
        }

        /// <summary>
        /// Starts the system tray listener (send the TaskbarCreated message).
        /// </summary>
        public void Run()
        {
            InteropCalls.Run();
        }

        /// <summary>
        /// Shuts down all the tasks.
        /// </summary>
        public void ShutdownAll()
        {
            InteropCalls.ShutdownAll();
        }

        /// <summary>
        /// Shuts down the task hooks.
        /// </summary>
        public void ShutdownTask()
        {
            InteropCalls.ShutdownTask();
        }

        /// <summary>
        /// Shuts down the system tray hooks.
        /// </summary>
        public void ShutdownSystray()
        {
            InteropCalls.ShutdownSystray();
        }
    }
}
