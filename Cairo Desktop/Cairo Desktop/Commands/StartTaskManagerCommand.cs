using CairoDesktop.Application.Interfaces;
using CairoDesktop.Application.Structs;
using CairoDesktop.Common.Localization;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;

namespace CairoDesktop.Commands
{
    public class StartTaskManagerCommand : ICairoCommand
    {
        public ICairoCommandInfo Info => _info;

        private readonly StartTaskManagerCommandInfo _info = new StartTaskManagerCommandInfo();

        public void Setup() { }

        public bool Execute(List<CairoCommandParameter> parameters)
        {
            ShellHelper.StartTaskManager();
            return true;
        }

        public void Dispose() { }
    }

    public class StartTaskManagerCommandInfo : ICairoCommandInfo
    {
        public string Identifier => "StartTaskManagerCommand";

        public string Description => "Starts Task Manager.";

        public string Label => DisplayString.sCairoMenu_TaskManager;

        public bool IsAvailable => true;

        public IReadOnlyCollection<CairoCommandParameter> Parameters => null;
    }
}
