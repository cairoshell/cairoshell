using CairoDesktop.Common;
using ManagedShell.Common.SupportingClasses;
using ManagedShell.ShellFolders;

namespace CairoDesktop.Interfaces
{
    public interface IDesktopManager
    {
        ShellWindow ShellWindow { get; }

        bool AllowProgmanChild { get; }

        DesktopNavigationToolbar DesktopToolbar { get; }
        
        bool IsOverlayOpen { get; set; }
        
        ShellFolder DesktopLocation { get; }
        
        NavigationManager NavigationManager { get; }
        
        DesktopOverlay DesktopOverlayWindow { get; }
        
        DesktopIcons DesktopIconsControl { get; }

        void ConfigureDesktop();
    }
}