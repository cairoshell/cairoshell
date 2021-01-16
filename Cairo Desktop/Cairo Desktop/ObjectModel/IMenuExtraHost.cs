using System;
using System.Windows.Forms;
using ManagedShell.WindowsTray;

namespace CairoDesktop.ObjectModel
{
    public interface IMenuExtraHost
    {
        IntPtr GetHandle();

        Screen GetScreen();

        TrayHostSizeData GetTrayHostSizeData();
    }
}
