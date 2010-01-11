namespace Cairo.WindowsHooksWrapper
{
    /// <summary>
    /// Numerical values of the NIM_* messages represented as an enumeration.
    /// </summary>
    public enum NIM : uint
    {
        /// <summary>
        /// Add a new icon.
        /// </summary>
        NIM_ADD = 0,

        /// <summary>
        /// Modify an existing icon.
        /// </summary>
        NIM_MODIFY = 1,

        /// <summary>
        /// Delete an icon.
        /// </summary>
        NIM_DELETE = 2,

        /// <summary>
        /// Shell v5 and above - Return focus to the notification area.
        /// </summary>
        NIM_SETFOCUS = 3,

        /// <summary>
        /// Shell v4 and above - Instructs the taskbar to behave accordingly based on the version (uVersion) set in the notifiyicondata struct.
        /// </summary>
        NIM_SETVERSION = 4
    }
}
