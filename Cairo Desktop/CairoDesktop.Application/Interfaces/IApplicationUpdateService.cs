using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface IApplicationUpdateService
    {
        bool AutomaticUpdatesEnabled { get; set; }

        void CheckForUpdates();

        void Initialize(Action shutdownAction);
    }
}