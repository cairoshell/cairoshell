namespace Cairo.WindowsHooksWrapper
{
    /// <summary>
    /// Delegate signature for the system tray callback.
    /// </summary>
    /// <param name="msg">The system tray message number.</param>
    /// <param name="nicData">The NotifyIconData structure</param>
    /// <returns>Indication of message outcome.</returns>
    public delegate bool SystrayDelegate(uint msg, NOTIFYICONDATA nicData);
    
    /// <summary>
    /// Delegate signature for the task callbacks.
    /// </summary>
    /// <param name="nCode">The task message code.</param>
    /// <param name="taskInfo">The task information structure.</param>
    /// <returns>Indication of message outcome.</returns>
    public delegate bool TaskDelegate(int nCode, ref TASKINFORMATION taskInfo);
}
