using System;
using System.Runtime.InteropServices;
using CairoDesktop.Common.DesignPatterns;
using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTray
{
    internal class ExplorerTrayService: SingletonObject<ExplorerTrayService>
    {
        private SystrayDelegate trayDelegate;
        private IntPtr toolbarHwnd;

        private ExplorerTrayService() { }

        public void SetSystrayCallback(SystrayDelegate theDelegate)
        {
            trayDelegate = theDelegate;
        }

        public void Run()
        {
            if (!Shell.IsCairoRunningAsShell && trayDelegate != null)
            {
                toolbarHwnd = FindExplorerTrayToolbarHwnd();
                GetTrayItems();
            }
        }

        private IntPtr FindExplorerTrayToolbarHwnd()
        {
            IntPtr hwnd = FindWindow("Shell_TrayWnd", "");

            if (hwnd != IntPtr.Zero)
            {
                hwnd = FindWindowEx(hwnd, IntPtr.Zero, "TrayNotifyWnd", "");

                if (hwnd != IntPtr.Zero)
                {
                    hwnd = FindWindowEx(hwnd, IntPtr.Zero, "SysPager", "");

                    if (hwnd != IntPtr.Zero)
                    {
                        hwnd = FindWindowEx(hwnd, IntPtr.Zero, "ToolbarWindow32", IntPtr.Zero);

                        return hwnd;
                    }
                }
            }

            return IntPtr.Zero;
        }

        private int GetNumTrayIcons()
        {
            return (int)SendMessage(toolbarHwnd, (int)TB.BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        private TrayItem GetTrayItem(int i, IntPtr hBuffer, IntPtr hProcess)
        {
            TBBUTTON tbButton = new TBBUTTON();
            TrayItem trayItem = new TrayItem();
            IntPtr hTBButton = Marshal.AllocHGlobal(Marshal.SizeOf(tbButton));
            IntPtr hTrayItemBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(trayItem));

            IntPtr msgSuccess = SendMessage(toolbarHwnd, (int)TB.GETBUTTON, (IntPtr)i, hBuffer);
            if (ReadProcessMemory(hProcess, hBuffer, hTBButton, Marshal.SizeOf(tbButton), out _))
            {
                tbButton = (TBBUTTON)Marshal.PtrToStructure(hTBButton, typeof(TBBUTTON));

                if (ReadProcessMemory(hProcess, tbButton.dwData, hTrayItemBuffer, Marshal.SizeOf(trayItem), out _))
                {
                    trayItem = (TrayItem)Marshal.PtrToStructure(hTrayItemBuffer, typeof(TrayItem));

                    CairoLogger.Instance.Debug(
                        $"ExplorerTrayService: Got tray item: {trayItem.szIconText}");
                }
            }

            return trayItem;
        }

        private void GetTrayItems()
        {
            int count = GetNumTrayIcons();

            GetWindowThreadProcessId(toolbarHwnd, out var processId);
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, (int)processId);
            IntPtr hBuffer = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)Marshal.SizeOf(new TBBUTTON()), AllocationType.Commit,
                MemoryProtection.ReadWrite);

            for (int i = 0; i < count; i++)
            {
                NOTIFYICONDATA nid = GetTrayItemNID(GetTrayItem(i, hBuffer, hProcess));

                if (trayDelegate != null)
                {
                    if (!trayDelegate((uint)NIM.NIM_ADD, nid))
                    {
                        CairoLogger.Instance.Debug("ExplorerTrayService: Ignored notify icon message");
                    }
                }
            }

            VirtualFreeEx(hProcess, hBuffer, 0, AllocationType.Release);

            CloseHandle((int)hProcess);
        }

        private NOTIFYICONDATA GetTrayItemNID(TrayItem trayItem)
        {
            NOTIFYICONDATA nid = new NOTIFYICONDATA();

            nid.hWnd = (uint)trayItem.hWnd;
            nid.uID = trayItem.uID;
            nid.uCallbackMessage = trayItem.uCallbackMessage;
            nid.szTip = trayItem.szIconText;
            nid.hIcon = (uint)trayItem.hIcon;
            nid.uVersion = trayItem.uVersion;
            nid.guidItem = trayItem.guidItem;
            nid.dwState = 0;
            nid.uFlags = NIF.GUID | NIF.ICON | NIF.MESSAGE | NIF.TIP;

            return nid;
        }
    }
}
