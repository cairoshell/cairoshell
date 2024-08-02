using System;

namespace CairoDesktop.Application.Interfaces
{
    public interface IInitializationService
    {
        void SetIsCairoRunningAsShell();

        void WriteApplicationDebugInfoToConsole(Version productVersion);

        bool LoadCommands();

        bool LoadExtensions();

        void SetTheme();

        void SetupWindowServices();
    }
}