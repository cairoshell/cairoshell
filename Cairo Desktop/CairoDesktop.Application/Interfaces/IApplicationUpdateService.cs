using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface IApplicationUpdateService
    {
        bool AutomaticUpdatesEnabled { get; set; }

        bool IsAvailable { get; }

        void CheckForUpdates();
    }
}