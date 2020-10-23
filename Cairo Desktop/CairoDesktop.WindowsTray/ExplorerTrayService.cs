using System;
using System.Runtime.InteropServices;
using CairoDesktop.Common.DesignPatterns;
using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using Microsoft.Win32;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTray
{
    internal class ExplorerTrayService: SingletonObject<ExplorerTrayService>
    {
        private SystrayDelegate trayDelegate;

        private ExplorerTrayService() { }

        public void SetSystrayCallback(SystrayDelegate theDelegate)
        {
            trayDelegate = theDelegate;
        }

        public void Run()
        {
            if (!Shell.IsCairoRunningAsShell && trayDelegate != null)
            {
                bool autoTrayEnabled = GetAutoTrayEnabled();

                if (autoTrayEnabled)
                {
                    // we can't get tray icons that are in the hidden area, so disable that temporarily if enabled
                    SetAutoTrayEnabled(false);
                }

                GetTrayItems();

                if (autoTrayEnabled)
                {
                    SetAutoTrayEnabled(true);
                }
            }
        }

        private void GetTrayItems()
        {
            IntPtr toolbarHwnd = FindExplorerTrayToolbarHwnd();

            if (toolbarHwnd == IntPtr.Zero)
            {
                return;
            }

            int count = GetNumTrayIcons(toolbarHwnd);

            if (count < 1)
            {
                return;
            }

            GetWindowThreadProcessId(toolbarHwnd, out var processId);
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, (int)processId);
            IntPtr hBuffer = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)Marshal.SizeOf(new TBBUTTON()), AllocationType.Commit,
                MemoryProtection.ReadWrite);

            for (int i = 0; i < count; i++)
            {
                NOTIFYICONDATA nid = GetTrayItemNID(GetTrayItem(i, hBuffer, hProcess, toolbarHwnd));

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

        private int GetNumTrayIcons(IntPtr toolbarHwnd)
        {
            return (int)SendMessage(toolbarHwnd, (int)TB.BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        private TrayItem GetTrayItem(int i, IntPtr hBuffer, IntPtr hProcess, IntPtr toolbarHwnd)
        {
            TBBUTTON tbButton = new TBBUTTON();
            TrayItem trayItem = new TrayItem();
            IntPtr hTBButton = Marshal.AllocHGlobal(Marshal.SizeOf(tbButton));
            IntPtr hTrayItem = Marshal.AllocHGlobal(Marshal.SizeOf(trayItem));

            IntPtr msgSuccess = SendMessage(toolbarHwnd, (int)TB.GETBUTTON, (IntPtr)i, hBuffer);
            if (ReadProcessMemory(hProcess, hBuffer, hTBButton, Marshal.SizeOf(tbButton), out _))
            {
                tbButton = (TBBUTTON)Marshal.PtrToStructure(hTBButton, typeof(TBBUTTON));

                if (tbButton.dwData != UIntPtr.Zero)
                {
                    if (ReadProcessMemory(hProcess, tbButton.dwData, hTrayItem, Marshal.SizeOf(trayItem), out _))
                    {
                        trayItem = (TrayItem) Marshal.PtrToStructure(hTrayItem, typeof(TrayItem));

                        CairoLogger.Instance.Debug(
                            $"ExplorerTrayService: Got tray item: {trayItem.szIconText}");
                    }
                }
            }

            return trayItem;
        }

        private NOTIFYICONDATA GetTrayItemNID(TrayItem trayItem)
        {
            NOTIFYICONDATA nid = new NOTIFYICONDATA();
            bool iconSuccess = false;

            nid.hWnd = (uint)trayItem.hWnd;
            nid.uID = trayItem.uID;
            nid.uCallbackMessage = trayItem.uCallbackMessage;
            nid.szTip = trayItem.szIconText;
            try
            {
                nid.hIcon = (uint) trayItem.hIcon;

                if (nid.hIcon != 0u)
                {
                    iconSuccess = true;
                }
            }
            catch { }
            nid.uVersion = trayItem.uVersion;
            nid.guidItem = trayItem.guidItem;
            nid.dwState = 0;
            nid.uFlags = NIF.GUID | NIF.MESSAGE | NIF.TIP;

            if (iconSuccess)
            {
                nid.uFlags |= NIF.ICON;
            }
            else
            {
                CairoLogger.Instance.Warning($"ExplorerTrayService: Unable to use {trayItem.szIconText} icon handle for NOTIFYICONDATA struct");
            }

            return nid;
        }

        private bool GetAutoTrayEnabled()
        {
            int enableAutoTray = 1;

            try
            {
                RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", false);

                if (explorerKey != null)
                {
                    var enableAutoTrayValue = explorerKey.GetValue("EnableAutoTray");

                    if (enableAutoTrayValue != null)
                    {
                        enableAutoTray = Convert.ToInt32(enableAutoTrayValue);
                    }
                }
            }
            catch (Exception e)
            {
                CairoLogger.Instance.Debug($"ExplorerTrayService: Unable to get EnableAutoTray setting: {e.Message}");
            }

            return enableAutoTray == 1;
        }

        private void SetAutoTrayEnabled(bool enabled)
        {
            TrayNotify trayNotify = new TrayNotify();

            try
            {
                if (Shell.IsWindows8OrBetter)
                {
                    var trayNotifyInstance = (ITrayNotify) trayNotify;
                    trayNotifyInstance.EnableAutoTray(enabled);
                }
                else
                {
                    var trayNotifyInstance = (ITrayNotifyLegacy) trayNotify;
                    trayNotifyInstance.EnableAutoTray(enabled);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(trayNotify);
            }
        }
    }
}
