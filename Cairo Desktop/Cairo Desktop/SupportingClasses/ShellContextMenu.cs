using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;

namespace CairoDesktop.SupportingClasses
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
            AddToStacks
        }
        
        // Properties
        public delegate void ItemSelectAction(string item, string path, System.Windows.Controls.Button sender);
        public delegate void FolderItemSelectAction(string item, string path);
        private ItemSelectAction itemSelected;
        private FolderItemSelectAction folderItemSelected;

        private IContextMenu iContextMenu, newContextMenu;
        private IContextMenu2 iContextMenu2, newContextMenu2;
        private IContextMenu3 iContextMenu3, newContextMenu3;

        private IntPtr newSubmenuPtr;

        private IntPtr[] pidls;
        private string[] paths;
        private string folder;
        private IntPtr folderPidl;
        private IntPtr folderRelPidl;
        private int x;
        private int y;
        private IShellFolder parentShellFolder;
        private System.Windows.Controls.Button sender;

        public ShellContextMenu(string[] files, System.Windows.Controls.Button sender, ItemSelectAction itemSelected)
        {
            lock (Shell.ComLock)
            {
                this.CreateHandle(new CreateParams());
                this.paths = files;

                this.parentShellFolder = getParentShellFolder(this.paths[0]);
                this.pidls = pathsToPidls(this.paths);
                this.x = Cursor.Position.X;
                this.y = Cursor.Position.Y;

                this.itemSelected = itemSelected;
                this.sender = sender;

                ShowContextMenu();
            }
        }

        public ShellContextMenu(string folder, FolderItemSelectAction folderItemSelected)
        {
            lock (Shell.ComLock)
            {
                this.CreateHandle(new CreateParams());
                this.folder = folder;

                this.parentShellFolder = getParentShellFolder(this.folder);
                this.folderPidl = pathToFullPidl(this.folder);
                this.folderRelPidl = pathToRelPidl(this.folder);
                this.x = Cursor.Position.X;
                this.y = Cursor.Position.Y;

                this.folderItemSelected = folderItemSelected;

                ShowFolderMenu();
            }
        }

        public static void ExecuteAction(string action, string path, System.Windows.Controls.Button sender)
        {
            if (action == "rename" || action == "addStack")
            {
                CustomCommands.PerformAction(action, path, sender);
            }
            else if (action != "cut" && action != "copy" && action != "link")
            {
                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.IsOverlayOpen = false;
            }
        }

        private IntPtr[] pathsToPidls(string[] files)
        {
            IntPtr[] pidls = new IntPtr[files.Length];

            for(int i = 0; i < files.Length; i++)
            {
                pidls[i] = pathToRelPidl(files[i]);
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

        private IShellFolder getParentShellFolder(string path)
        {
            FileInfo fi = new FileInfo(path);
            IntPtr parentPidl;

            // directory is null on drive root
            if (fi.Directory != null)
            {
                parentPidl = pathToFullPidl(fi.Directory.FullName);
            }
            else
            {
                parentPidl = pathToFullPidl(fi.FullName);
            }

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
                    contextMenu = ShellFolders.CreatePopupMenu();

                    iContextMenu.QueryContextMenu(
                        contextMenu,
                        0,
                        ShellFolders.CMD_FIRST,
                        ShellFolders.CMD_LAST,
                        ShellFolders.CMF.EXPLORE |
                        ShellFolders.CMF.CANRENAME |
                        ((Control.ModifierKeys & Keys.Shift) != 0 ? ShellFolders.CMF.EXTENDEDVERBS : 0));

                    // add to stacks option for folders
                    if (Interop.Shell.Exists(paths[0]) && (File.GetAttributes(this.paths[0]) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 1, string.Empty);
                        ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.AddToStacks, Localization.DisplayString.sInterface_AddToStacks);
                    }

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
                                        this.x,
                                        this.y,
                                        this.Handle,
                                        IntPtr.Zero);
                    

                    if (selected >= ShellFolders.CMD_FIRST)
                    {
                        string command = ShellFolders.GetCommandString(iContextMenu, selected - ShellFolders.CMD_FIRST, true);

                        if ((CairoContextMenuItem)selected == CairoContextMenuItem.AddToStacks)
                        {
                            command = "addStack";
                        }
                        else
                        {
                            ShellFolders.InvokeCommand(
                                iContextMenu,
                                selected - ShellFolders.CMD_FIRST,
                                paths[0],
                                new Point(this.x, this.y));
                        }

                        if (this.itemSelected != null)
                            itemSelected(command, paths[0], sender);
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
                
                ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.BYCOMMAND, (int)CairoContextMenuItem.Paste, Localization.DisplayString.sInterface_Paste);

                if (GetNewContextMenu(out newContextMenuPtr, out newContextMenu))
                {
                    ShellFolders.AppendMenu(contextMenu, ShellFolders.MFT.SEPARATOR, 1, string.Empty);
                    newContextMenu.QueryContextMenu(
                        contextMenu,
                        2,
                        ShellFolders.CMD_FIRST,
                        ShellFolders.CMD_LAST,
                        ShellFolders.CMF.NORMAL);

                    newSubmenuPtr = ShellFolders.GetSubMenu(contextMenu, 2);

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

                CairoContextMenuItem selected = (CairoContextMenuItem)ShellFolders.TrackPopupMenuEx(
                                    contextMenu,
                                    ShellFolders.TPM.RETURNCMD,
                                    this.x,
                                    this.y,
                                    this.Handle,
                                    IntPtr.Zero);
                
                if ((int)selected >= ShellFolders.CMD_FIRST)
                {
                    string command = "";
                    switch (selected)
                    {

                        case CairoContextMenuItem.Properties:
                            command = "properties";
                            ShellFolders.InvokeCommand(
                                folder,
                                parentShellFolder,
                                new IntPtr[] { folderPidl },
                                command,
                                new Point(this.x, this.y));
                            break;

                        case CairoContextMenuItem.Paste:
                            command = "paste";
                            ShellFolders.InvokeCommand(
                                folder,
                                parentShellFolder,
                                new IntPtr[] { folderPidl },
                                command,
                                new Point(this.x, this.y));
                            break;

                        default:
                            if ((uint)selected <= ShellFolders.CMD_LAST)
                            {
                                ShellFolders.InvokeCommand(
                                    newContextMenu,
                                    (uint)selected - ShellFolders.CMD_FIRST,
                                    this.folder,
                                    new Point(this.x, this.y));
                            }
                            break;
                    }

                    if (this.folderItemSelected != null)
                        folderItemSelected(command, folder);
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
            if (m.Msg == (int)NativeMethods.WM.MENUSELECT)
            {
                int a = 1;
            }

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
