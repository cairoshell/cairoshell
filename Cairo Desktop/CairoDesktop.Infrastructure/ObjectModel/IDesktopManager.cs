namespace CairoDesktop.Infrastructure.ObjectModel
{
    public interface IDesktopManager
    {
        bool IsOverlayOpen { get; set; }

        bool IsEnabled { get; }

        void Initialize();

        void SetPath(string path);

        void ResetPosition(bool displayChanged);

        void ToggleOverlay();
    }
}