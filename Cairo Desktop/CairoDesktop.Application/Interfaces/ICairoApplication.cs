using System;
using System.Collections.Generic;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoApplication
    {
        List<IMenuItem> CairoMenu { get; }
        
        List<IShellExtension> Extensions { get; }

        List<IMenuBarExtension> MenuBarExtensions { get; }

        List<IMenuItem> Places { get; }

        string AppVersion { get; }
        
        bool IsShuttingDown { get; }

        void ExitCairo();

        void RestartCairo();

        int Run();

        void Dispatch(Action action);

        void ClearResources();

        void AddResource(object resource);

        object FindResource(string name);
    }
}