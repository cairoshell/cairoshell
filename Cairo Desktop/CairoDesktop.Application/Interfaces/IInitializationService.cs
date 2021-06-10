using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface IInitializationService
    {
        void SetIsCairoRunningAsShell();

        void WriteApplicationDebugInfoToConsole(Version productVersion);

        void LoadExtensions();

        void SetTheme();

        void SetupWindowServices();
    }
}