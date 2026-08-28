using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ManagedShell.Common.Logging;
using ManagedShell.ShellFolders;
using ManagedShell.ShellFolders.Enums;
using Microsoft.VisualBasic.FileIO;

namespace CairoDesktop.Common.Helpers
{
    /// <summary>
    /// Recycle Bin items live on disk under a mangled physical name (e.g. $RXXXXXX.ext) that differs from
    /// their original display name. FileOperationWorker names its destination after the dropped file's
    /// on-disk name, so a plain move/copy would otherwise land under that mangled name instead of the
    /// original one (unlike Explorer, which preserves it). This resolves the real name via the shell
    /// namespace and performs the operation against those items directly, bypassing FileOperationWorker
    /// only for the items that actually need renaming.
    /// </summary>
    public static class FileDropHelper
    {
        private const string RecycleBinPathSegment = "\\$RECYCLE.BIN\\";

        public static void PerformOperation(FileOperationWorker fileWorker, string[] fileNames, string targetDirectory, bool isMove)
        {
            if (fileWorker == null || fileNames == null || fileNames.Length == 0)
            {
                return;
            }

            List<string> regularFiles = new List<string>();

            foreach (string path in fileNames)
            {
                string originalName = TryGetRecycleBinDisplayName(path);

                if (originalName != null &&
                    !string.Equals(Path.GetFileName(path), originalName, StringComparison.OrdinalIgnoreCase))
                {
                    PerformNamedFileOperation(path, targetDirectory, originalName, isMove);
                }
                else
                {
                    regularFiles.Add(path);
                }
            }

            if (regularFiles.Count > 0)
            {
                fileWorker.PerformOperation(isMove ? FileOperation.Move : FileOperation.Copy, regularFiles.ToArray(), targetDirectory);
            }
        }

        private static string TryGetRecycleBinDisplayName(string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath) ||
                physicalPath.IndexOf(RecycleBinPathSegment, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return null;
            }

            try
            {
                using (ShellFolder recycleBin = new ShellFolder("shell:RecycleBinFolder", IntPtr.Zero, false, false))
                {
                    foreach (ShellFile file in recycleBin.Files)
                    {
                        if (!string.Equals(file.Path, physicalPath, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // ManagedShell's ShellFile.DisplayName for a Recycle Bin item is the item's
                        // parsing name, which is the original *full path* (e.g.
                        // "C:\Users\me\Downloads\report.txt"), not just the file name. Take the leaf so
                        // the caller combines it with the drop target directory rather than restoring
                        // the item to its original location. (If a future ShellFile revision returns a
                        // bare name here, GetFileName is a harmless no-op.)
                        string displayName = Path.GetFileName(file.DisplayName);

                        if (string.IsNullOrEmpty(displayName))
                        {
                            return null;
                        }

                        string sourceExtension = Path.GetExtension(physicalPath);

                        // A display name can omit the extension when "hide known extensions" is set, so
                        // reattach the real extension (preserved by the Recycle Bin on the mangled
                        // physical name) if needed.
                        if (!string.IsNullOrEmpty(sourceExtension) &&
                            !displayName.EndsWith(sourceExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            displayName += sourceExtension;
                        }

                        return displayName;
                    }
                }
            }
            catch (Exception e)
            {
                ShellLogger.Warning($"FileDropHelper: Unable to resolve Recycle Bin display name for {physicalPath}: {e.Message}");
            }

            return null;
        }

        private static void PerformNamedFileOperation(string sourcePath, string targetDirectory, string destinationName, bool isMove)
        {
            Task.Run(() =>
            {
                try
                {
                    string destinationPath = Path.Combine(targetDirectory, destinationName);
                    FileAttributes attributes = System.IO.File.GetAttributes(sourcePath);

                    if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        if (isMove)
                        {
                            FileSystem.MoveDirectory(sourcePath, destinationPath, UIOption.AllDialogs);
                        }
                        else
                        {
                            FileSystem.CopyDirectory(sourcePath, destinationPath, UIOption.AllDialogs);
                        }
                    }
                    else
                    {
                        if (isMove)
                        {
                            FileSystem.MoveFile(sourcePath, destinationPath, UIOption.AllDialogs);
                        }
                        else
                        {
                            FileSystem.CopyFile(sourcePath, destinationPath, UIOption.AllDialogs);
                        }
                    }
                }
                catch (Exception e)
                {
                    ShellLogger.Error($"FileDropHelper: Unable to {(isMove ? "move" : "copy")} {sourcePath} to {targetDirectory} as {destinationName}: {e.Message}");
                }
            });
        }
    }
}
