using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;

namespace CairoDesktop.Common
{
    // Provides ability to use the shell context menu functions within Cairo.
    // Derived from Steven Roebert's C# File Browser
    // https://www.codeproject.com/Articles/15059/C-File-Browser

    public class ShellContextMenu : NativeWindow
    {
        // Enums
        private enum CairoContextMenuItem
        {
            Paste = (int)ShellFolders.CMD_LAST + 1,
            Properties,
            AddToStacks,
            RemoveFromStacks,
            OpenInNewWindow,
            Personalize,
            DisplaySettings
        }
        
        // Properties
        public delegate void ItemSelectAction(string item, string path);
        public delegate void FolderItemSelectAction(string item, string path);
        private ItemSelectAction itemSelected;
        private FolderItemSelectAction folderItemSelected;

        private IContextMenu iContextMenu, newContextMenu;
        private IContextMenu2 iContextMenu2, newContextMenu2;
        private IContextMenu3 iContextMenu3, newContextMenu3;

        private IntPtr newSubmenuPtr;

        private IntPtr[] pidls;
        private SystemFile[] paths;
        private SystemDirectory directory;
        private string folder;
        private IntPtr folderPidl;
        private IntPtr folderRelPidl;
        private int x;
        private int y;
        private string parent;
        private IShellFolder parentShellFolder;

        public ShellContextMenu(SystemFile[] files, ItemSelectAction itemSelected)
        {
            lock (Shell.ComLock)
            {
                CreateHandle(new CreateParams());
                paths = files;

                parent = getParentDir(paths[0].FullName);
                parentShellFolder = getParentShellFolder(paths[0].FullName);
                pidls = pathsToPidls(paths);
                x = Cursor.Position.X;
                y = Cursor.Position.Y;

                this.itemSelected = itemSelected;

                ShowContextMenu();
            }
        }

        public ShellContextMenu(SystemDirectory directory, FolderItemSelectAction folderItemSelected)
        {
            if (directory != null)
            {
                lock (Shell.ComLock)
                {
                    CreateHandle(new CreateParams());
                    this.directory = directory;
                    folder = directory.FullName;

                    parent = getParentDir(folder);
                    parentShellFolder = getParentShellFolder(folder);
                    folderPidl = pathToFullPidl(folder);
                    folderRelPidl = pathToRelPidl(folder);
                    x = Cursor.Position.X;
                    y = Cursor.Position.Y;

                    this.folderItemSelected = folderItemSelected;

                    ShowFolderMenu();
                }
            }
        }

        private IntPtr[] pathsToPidls(SystemFile[] files)
        {
            IntPtr[] pidls = new IntPtr[files.Length];

            for(int i = 0; i < files.Length; i++)
            {
                pidls[i] = pathToRelPidl(files[i].FullName);
            }

            return pidls;
        }

        private IntPtr pathToRelPidl(string path)
        {
            IntPtr pidl;
            uint pchEaten = 0;
            ShellFolders.SFGAO pdwAttributes = 0;

            string file = Path.GetFileName(path);

            if (parentShellFolder != null)
            {
                int result = parentShellFolder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, file, ref pchEaten, out pidl, ref pdwAttributes);

                if (pidl == IntPtr.Zero)
                {
                    CairoLogger.Instance.Debug("HRESULT " + result + " retrieving pidl for " + path);
                }
                
                return pidl;
            }
            else
            {
                CairoLogger.Instance.Debug("Parent IShellFolder for " + path + " is null");
                return IntPtr.Zero;
            }
        }

        private IntPtr pathToFullPidl(string path)
        {
            IntPtr pidl = ShellFolders.ILCreateFromPath(path);
            if (pidl == IntPtr.Zero)
            {
                CairoLogger.Instance.Debug("Unable to get pidl for " + path);
            }

            return pidl;
        }

        private string getParentDir(string path)
        {
            FileInfo fi = new FileInfo(path);
            string dir;

            // directory is null on drive root
            if (fi.Directory != null)
            {
                dir = fi.Directory.FullName;
            }
            else
            {
                dir = fi.FullName;
            }

            return dir;
        }

        private IShellFolder getParentShellFolder(string path)
        {
            IntPtr parentPidl = pathToFullPidl(parent);

            IntPtr desktopFolderPtr;
            ShellFolders.SHGetDesktopFolder(out desktopFolderPtr);
            IShellFolder desktop = (IShellFolder)Marshal.GetTypedObjectForIUnknown(desktopFolderPtr, typeof(IShellFolder));

            IntPtr parentPtr;
            if (desktop.BindToObject(parentPidl, IntPtr.Zero, ref ShellFolders.IID_IShellFolder, out parentPtr) == ShellFolders.S_OK)
            {
                Marshal.FinalReleaseComObject(desktop);
                return (IShellFolder)Marshal.GetTypedObjectForIUnknown(parentPtr, typeof(IShellFolder));
            }
            else
            {
                CairoLogger.Instance.Debug("Unable to bind IShellFolder");
                return null;
            }
        }
        
        private bool GetNewContextMenu(out IntPtr iContextMenuPtr, out IContextMenu iContextMenu)
        {
            if (ShellFolders.CoCreateInstance(
                    ref ShellFolders.CLSID_NewMenu,
                    IntPtr.Zero,
                    ShellFolders.CLSCTX.INPROC_SERVER,
                    ref ShellFolders.IID_IContextMenu,
                    out iContextMenuPtr) == ShellFolders.S_OK)
            {
                iContextMenu = Marshal.GetTypedObjectForIUnknown(iContextMenuPtr, typeof(IContextMenu)) as IContextMenu;

                IntPtr iShellExtInitPtr;
                if (Marshal.QueryInterface(
                    iContextMenuPtr,
                    ref ShellFolders.IID_IShellExtInit,
                    out iShellExtInitPtr) == ShellFolders.S_OK)
                {
                    IShellExtInit iShellExtInit = Marshal.GetTypedObjectForIUnknown(
                        iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;
                    
                    iShellExtInit.Initialize(folderPidl, IntPtr.Zero, 0);

                    Marshal.ReleaseComObject(iShellExtInit);
                    Marshal.Release(iShellExtInitPtr);

                    return true;
                }
                else
                {
                    if (iContextMenu != null)
                    {
                        Marshal.ReleaseComObject(iContextMenu);
                        iContextMenu = null;
                    }

                    if (iContextMenuPtr != IntPtr.Zero)
                    {
                        Marshal.Release(iContextMenuPtr);
                        iContextMenuPtr = IntPtr.Zero;
                    }

                    return false;
                }
            }
            else
            {
                iContextMenuPtr = IntPtr.Zero;
                iContextMenu = null;
                return false;
            }
        }

        private uint prependItems(IntPtr contextMenu, bool allFolders, bool allInStacks)
        {
            // add items to the top of the context menu here

            uint numPrepended = 0;
            
            // add open in new window option to files that are folders if dynamic desktop is enabled
            if (allFolders && Configuration.Settings.Instance.EnableDynamicDesktop)
            {
                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.OpenInNewWindow, Localization.DisplayString.sStacks_OpenInNewWindow);
                numPrepended++;
            }

            // add stacks options for files that are folders
            if (allFolders)
            {
                if (!allInStacks)
                {
                    ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.AddToStacks, Localization.DisplayString.sInterface_AddToStacks);
                }
                else
                {
                    ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.RemoveFromStacks, Localization.DisplayString.sInterface_RemoveFromStacks);
                }

                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 1, string.Empty);
                numPrepended += 2;
            }

            return numPrepended;
        }

        private void appendItems(IntPtr contextMenu, bool allFolders, bool allInStacks)
        {
            // add items to the bottom of the context menu here
        }
        
        public void ShowContextMenu()
        {
            IntPtr contextMenu = IntPtr.Zero,
                iContextMenuPtr = IntPtr.Zero,
                iContextMenuPtr2 = IntPtr.Zero,
                iContextMenuPtr3 = IntPtr.Zero;
            
            try
            {
                if (ShellFolders.GetIContextMenu(parentShellFolder, pidls, out iContextMenuPtr, out iContextMenu))
                {
                    // get some properties about our file(s)
                    bool allFolders = true;
                    bool allInStacks = true;
                    foreach (SystemFile file in paths)
                    {
                        if (!file.IsDirectory)
                        {
                            allFolders = false;
                        }
                        else
                        {
                            bool contains = false;
                            foreach(SystemDirectory dir in StacksManager.Instance.StackLocations)
                            {
                                if (dir.Equals(file.FullName))
                                {
                                    contains = true;
                                    break;
                                }
                            }

                            if (!contains) allInStacks = false;
                        }
                    }

                    contextMenu = ShellFolders.CreatePopupMenu();

                    uint numPrepended = prependItems(contextMenu, allFolders, allInStacks);

                    iContextMenu.QueryContextMenu(
                        contextMenu,
                        numPrepended,
                        ShellFolders.CMD_FIRST,
                        ShellFolders.CMD_LAST,
                        ShellFolders.CMF.EXPLORE |
                        ShellFolders.CMF.CANRENAME |
                        ((Control.ModifierKeys & Keys.Shift) != 0 ? ShellFolders.CMF.EXTENDEDVERBS : 0));

                    appendItems(contextMenu, allFolders, allInStacks);

                    Marshal.QueryInterface(iContextMenuPtr, ref ShellFolders.IID_IContextMenu2, out iContextMenuPtr2);
                    Marshal.QueryInterface(iContextMenuPtr, ref ShellFolders.IID_IContextMenu3, out iContextMenuPtr3);

                    try
                    {
                        iContextMenu2 =
                            (IContextMenu2)Marshal.GetTypedObjectForIUnknown(iContextMenuPtr2, typeof(IContextMenu2));

                        iContextMenu3 =
                            (IContextMenu3)Marshal.GetTypedObjectForIUnknown(iContextMenuPtr3, typeof(IContextMenu3));
                    }
                    catch (Exception) { }

                    uint selected = ShellFolders.TrackPopupMenuEx(
                                        contextMenu,
                                        ShellFolders.TPM.RETURNCMD,
                                        x,
                                        y,
                                        Handle,
                                        IntPtr.Zero);
                    

                    if (selected >= ShellFolders.CMD_FIRST)
                    {
                        string command = ShellFolders.GetCommandString(iContextMenu, selected - ShellFolders.CMD_FIRST, true);

                        // set action strings for us to use in our custom execution function
                        switch ((CairoContextMenuItem)selected)
                        {
                            case CairoContextMenuItem.AddToStacks:
                                command = "addStack";
                                break;

                            case CairoContextMenuItem.RemoveFromStacks:
                                command = "removeStack";
                                break;

                            case CairoContextMenuItem.OpenInNewWindow:
                                command = "openWithShell";
                                break;

                            default:
                                if (command == "open" && allFolders)
                                {
                                    // suppress running system code
                                    command = "openFolder";
                                }
                                else
                                {
                                    ShellFolders.InvokeCommand(
                                    iContextMenu,
                                    selected - ShellFolders.CMD_FIRST,
                                    parent,
                                    new System.Drawing.Point(x, y));
                                }
                                break;
                        }

                        itemSelected?.Invoke(command, paths[0].FullName);
                    }
                }
                else
                {
                    CairoLogger.Instance.Debug("Error retrieving IContextMenu");
                }
            }
            catch (Exception) { }
            finally
            {
                if (iContextMenu != null)
                {
                    Marshal.FinalReleaseComObject(iContextMenu);
                    iContextMenu = null;
                }

                if (iContextMenu2 != null)
                {
                    Marshal.FinalReleaseComObject(iContextMenu2);
                    iContextMenu2 = null;
                }

                if (iContextMenu3 != null)
                {
                    Marshal.FinalReleaseComObject(iContextMenu3);
                    iContextMenu3 = null;
                }

                if (parentShellFolder != null)
                {
                    Marshal.FinalReleaseComObject(parentShellFolder);
                    parentShellFolder = null;
                }

                if (contextMenu != null)
                    ShellFolders.DestroyMenu(contextMenu);

                if (iContextMenuPtr != IntPtr.Zero)
                    Marshal.Release(iContextMenuPtr);

                if (iContextMenuPtr2 != IntPtr.Zero)
                    Marshal.Release(iContextMenuPtr2);

                if (iContextMenuPtr3 != IntPtr.Zero)
                    Marshal.Release(iContextMenuPtr3);

                for (int i = 0; i < pidls.Length; i++)
                {
                    Marshal.FreeCoTaskMem(pidls[i]);
                    pidls[i] = IntPtr.Zero;
                }
            }
        }


        private void ShowFolderMenu()
        {
            IntPtr contextMenu = IntPtr.Zero, viewSubMenu = IntPtr.Zero;
            IntPtr newContextMenuPtr = IntPtr.Zero, newContextMenuPtr2 = IntPtr.Zero, newContextMenuPtr3 = IntPtr.Zero;
            newSubmenuPtr = IntPtr.Zero;

            try
            {
                contextMenu = ShellFolders.CreatePopupMenu();

                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.OpenInNewWindow, Localization.DisplayString.sStacks_OpenInNewWindow);
                if (!StacksManager.Instance.StackLocations.Contains(directory))
                {
                    ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.AddToStacks, Localization.DisplayString.sInterface_AddToStacks);
                }
                else
                {
                    ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.RemoveFromStacks, Localization.DisplayString.sInterface_RemoveFromStacks);
                }

                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 1, string.Empty);

                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.Paste, Localization.DisplayString.sInterface_Paste);

                if (GetNewContextMenu(out newContextMenuPtr, out newContextMenu))
                {
                    ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 1, string.Empty);
                    newContextMenu.QueryContextMenu(
                        contextMenu,
                        5,
                        ShellFolders.CMD_FIRST,
                        ShellFolders.CMD_LAST,
                        ShellFolders.CMF.NORMAL);

                    newSubmenuPtr = ShellFolders.GetSubMenu(contextMenu, 5);

                    Marshal.QueryInterface(newContextMenuPtr, ref ShellFolders.IID_IContextMenu2, out newContextMenuPtr2);
                    Marshal.QueryInterface(newContextMenuPtr, ref ShellFolders.IID_IContextMenu3, out newContextMenuPtr3);

                    try
                    {
                        newContextMenu2 =
                            (IContextMenu2)Marshal.GetTypedObjectForIUnknown(newContextMenuPtr2, typeof(IContextMenu2));

                        newContextMenu3 =
                            (IContextMenu3)Marshal.GetTypedObjectForIUnknown(newContextMenuPtr3, typeof(IContextMenu3));
                    }
                    catch (Exception) { }
                }
                
                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 0, string.Empty);
                ShellFolders.AppendMenu(contextMenu, 0, (int)CairoContextMenuItem.Properties, Localization.DisplayString.sInterface_Properties);
                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 0, string.Empty);

                if (!Shell.IsCairoRunningAsShell)
                {
                    ShellFolders.AppendMenu(contextMenu, 0, (int)CairoContextMenuItem.DisplaySettings, Localization.DisplayString.sDesktop_DisplaySettings);
                }

                ShellFolders.AppendMenu(contextMenu, 0, (int)CairoContextMenuItem.Personalize, Localization.DisplayString.sDesktop_Personalize);

                CairoContextMenuItem selected = (CairoContextMenuItem)ShellFolders.TrackPopupMenuEx(
                                    contextMenu,
                                    ShellFolders.TPM.RETURNCMD,
                                    x,
                                    y,
                                    Handle,
                                    IntPtr.Zero);
                
                if ((int)selected >= ShellFolders.CMD_FIRST)
                {
                    string command = "";
                    switch (selected)
                    {
                        case CairoContextMenuItem.OpenInNewWindow:
                            command = "openWithShell";
                            break;

                        case CairoContextMenuItem.AddToStacks:
                            command = "addStack";
                            break;

                        case CairoContextMenuItem.RemoveFromStacks:
                            command = "removeStack";
                            break;

                        case CairoContextMenuItem.Personalize:
                            command = "personalize";
                            break;

                        case CairoContextMenuItem.DisplaySettings:
                            command = "displaySettings";
                            break;

                        case CairoContextMenuItem.Properties:
                            command = "properties";
                            ShellFolders.InvokeCommand(
                                folder,
                                parentShellFolder,
                                new IntPtr[] { folderPidl },
                                command,
                                new System.Drawing.Point(x, y));
                            break;

                        case CairoContextMenuItem.Paste:
                            command = "paste";
                            ShellFolders.InvokeCommand(
                                folder,
                                parentShellFolder,
                                new IntPtr[] { folderPidl },
                                command,
                                new System.Drawing.Point(x, y));
                            break;

                        default:
                            if ((uint)selected <= ShellFolders.CMD_LAST)
                            {
                                command = "new";
                                ShellFolders.InvokeCommand(
                                    newContextMenu,
                                    (uint)selected - ShellFolders.CMD_FIRST,
                                    folder,
                                    new System.Drawing.Point(x, y));
                            }
                            break;
                    }

                    folderItemSelected?.Invoke(command, folder);
                }
            }
            catch (Exception) { }
            finally
            {
                if (newContextMenu != null)
                {
                    Marshal.FinalReleaseComObject(newContextMenu);
                    newContextMenu = null;
                }

                if (newContextMenu2 != null)
                {
                    Marshal.FinalReleaseComObject(newContextMenu2);
                    newContextMenu2 = null;
                }

                if (newContextMenu3 != null)
                {
                    Marshal.FinalReleaseComObject(newContextMenu3);
                    newContextMenu3 = null;
                }

                if (contextMenu != null)
                    ShellFolders.DestroyMenu(contextMenu);

                if (viewSubMenu != null)
                    ShellFolders.DestroyMenu(viewSubMenu);

                if (newContextMenuPtr != IntPtr.Zero)
                    Marshal.Release(newContextMenuPtr);

                if (newContextMenuPtr2 != IntPtr.Zero)
                    Marshal.Release(newContextMenuPtr2);

                if (newContextMenuPtr3 != IntPtr.Zero)
                    Marshal.Release(newContextMenuPtr3);

                newSubmenuPtr = IntPtr.Zero;

                Marshal.FreeCoTaskMem(folderPidl);
                Marshal.FreeCoTaskMem(folderRelPidl);
                folderPidl = IntPtr.Zero;
                folderRelPidl = IntPtr.Zero;
            }
        }

        /// <summary>
        /// This method receives WindowMessages. It will make the "Open With" and "Send To" work 
        /// by calling HandleMenuMsg and HandleMenuMsg2.
        /// </summary>
        /// <param name="m">the Message of the Browser's WndProc</param>
        /// <returns>true if the message has been handled, false otherwise</returns>
        protected override void WndProc(ref Message m)
        {
            if (iContextMenu != null &&
                m.Msg == (int)NativeMethods.WM.MENUSELECT &&
                ((int)ShellFolders.HiWord(m.WParam) & (int)ShellFolders.MFT.SEPARATOR) == 0 &&
                ((int)ShellFolders.HiWord(m.WParam) & (int)ShellFolders.MFT.POPUP) == 0)
            {
                string info = string.Empty;
                info = ShellFolders.GetCommandString(
                    iContextMenu,
                    (uint)ShellFolders.LoWord(m.WParam) - ShellFolders.CMD_FIRST,
                    false);
            }

            if (iContextMenu2 != null &&
                (m.Msg == (int)NativeMethods.WM.INITMENUPOPUP ||
                 m.Msg == (int)NativeMethods.WM.MEASUREITEM ||
                 m.Msg == (int)NativeMethods.WM.DRAWITEM))
            {
                if (iContextMenu2.HandleMenuMsg(
                    (uint)m.Msg, m.WParam, m.LParam) == ShellFolders.S_OK)
                    return;
            }

            if (newContextMenu2 != null &&
                ((m.Msg == (int)NativeMethods.WM.INITMENUPOPUP && m.WParam == newSubmenuPtr) ||
                 m.Msg == (int)NativeMethods.WM.MEASUREITEM ||
                 m.Msg == (int)NativeMethods.WM.DRAWITEM))
            {
                if (newContextMenu2.HandleMenuMsg(
                    (uint)m.Msg, m.WParam, m.LParam) == ShellFolders.S_OK)
                    return;
            }

            if (iContextMenu3 != null &&
                m.Msg == (int)NativeMethods.WM.MENUCHAR)
            {
                if (iContextMenu3.HandleMenuMsg2(
                    (uint)m.Msg, m.WParam, m.LParam, IntPtr.Zero) == ShellFolders.S_OK)
                    return;
            }

            if (newContextMenu3 != null &&
                m.Msg == (int)NativeMethods.WM.MENUCHAR)
            {
                if (newContextMenu3.HandleMenuMsg2(
                    (uint)m.Msg, m.WParam, m.LParam, IntPtr.Zero) == ShellFolders.S_OK)
                    return;
            }

            base.WndProc(ref m);
        }
    }
}
