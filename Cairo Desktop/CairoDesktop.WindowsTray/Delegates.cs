using System;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTray
{
    /// <summary>
    /// Delegate signature for the system tray callback.
    /// </summary>
    /// <param name="msg">The system tray message number.</param>
    /// <param name="nicData">The NotifyIconData structure</param>
    /// <returns>Indication of message outcome.</returns>
    public delegate bool SystrayDelegate(uint msg, NOTIFYICONDATA nicData);

    /// <summary>
    /// Delegate signature for the icon data callback.
    /// </summary>
    /// <param name="iconData">The NotifyIconIdentifier structure</param>
    /// <returns>Indication of message outcome.</returns>
    public delegate IntPtr IconDataDelegate(CAIROWINNOTIFYICONIDENTIFIER iconData);
}
