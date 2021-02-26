using ManagedShell.ShellFolders.Enums;

namespace CairoDesktop.SupportingClasses
{
    internal enum CairoContextMenuItem : uint
    {
        AddToStacks = CommonContextMenuItem.Properties + 1,
        RemoveFromStacks,
        OpenInNewWindow,
        OpenOnDesktop,
        Personalize,
        DisplaySettings
    }
}
