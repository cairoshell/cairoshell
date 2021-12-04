using System;
using System.Collections.Specialized;
using System.IO;
using CairoDesktop.Common;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;

namespace CairoDesktop.SupportingClasses
{
    public static class CustomCommands
    {
        static CustomCommands()
        {
        }

        public static void PerformAction(string verb, string fileName)
        {
            switch (verb)
            {
                case Actions.Open:
                    ShellHelper.StartProcess(fileName, workingDirectory: getParent(fileName));
                    return;
                case Actions.Delete:
                    string displayName = ShellHelper.GetDisplayName(fileName);
                    CairoMessage.ShowOkCancel(string.Format(Localization.DisplayString.sDesktop_DeleteInfo, displayName), 
                        Localization.DisplayString.sDesktop_DeleteTitle, CairoMessageImage.Warning, 
                        Localization.DisplayString.sInterface_Delete, Localization.DisplayString.sInterface_Cancel,
                        result =>
                        {
                            if (result == true)
                            {
                                ShellHelper.SendToRecycleBin(fileName);
                            }
                        });
                    return;
                case Actions.Properties:
                    ShellHelper.ShowFileProperties(fileName);
                    return;
                case Actions.Copy:
                    StringCollection scPath = new StringCollection();
                    scPath.Add(fileName);
                    System.Windows.Forms.Clipboard.SetFileDropList(scPath);
                    return;
                case Actions.AddStack:
                    StacksManager.Instance.AddLocation(fileName);
                    return;
                case Actions.RemoveStack:
                    StacksManager.Instance.RemoveLocation(fileName);
                    return;
                case Actions.OpenWithShell:
                    FolderHelper.OpenWithShell(fileName);
                    break;
                case Actions.Personalize:
                    ShellHelper.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,2");
                    break;
                case Actions.DisplaySettings:
                    ShellHelper.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,3");
                    break;
                default:
                    ShellHelper.StartProcess(fileName, "", verb, getParent(fileName));
                    break;
            }
        }

        private static string getParent(string fileName)
        {
            string parent = null;

            try
            {
                parent = Directory.GetParent(fileName).FullName;
            }
            catch (Exception e)
            {
                ShellLogger.Warning($"CustomCommands: Unable to get parent folder for {fileName}: {e.Message}");
            }

            return parent;
        }

        public sealed class Actions
        {
            private Actions() { }

            public const string Open = "open";

            public const string Delete = "delete";

            public const string Properties = "properties";

            public const string Copy = "copy";

            public const string Cut = "cut";

            public const string Link = "link";

            public const string OpenWithShell = "openWithShell";

            public const string Personalize = "personalize";

            public const string DisplaySettings = "displaySettings";

            public const string Rename = "rename";

            public const string AddStack = "addStack";

            public const string RemoveStack = "removeStack";
        }
    }
}
