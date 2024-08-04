using System.Threading.Tasks;
using CairoDesktop.Common;
using System.Windows.Input;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.DependencyInjection;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.Services
{
    public sealed class ShellHotKeyService : CairoBackgroundService
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly ICommandService _commandService;

        public ShellHotKeyService(ICairoApplication cairoApplication, ICommandService commandService)
        {
            _cairoApplication = cairoApplication;

            ServiceStartTask = new Task(RegisterSystemHotkeys);
            _commandService = commandService;
        }

        private void RegisterSystemHotkeys()
        {
            if (EnvironmentHelper.IsAppRunningAsShell)
            {
                _cairoApplication.Dispatch(() =>
                {
                    new HotKey(Key.R, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinRCommand);
                    new HotKey(Key.D, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinDCommand);
                    new HotKey(Key.OemComma, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinDCommand);
                    new HotKey(Key.E, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinECommand);
                    new HotKey(Key.I, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinICommand);
                    new HotKey(Key.Pause, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinPauseCommand);
                });
            }
        }

        private void OnWinDCommand(HotKey obj)
        {
            _commandService.InvokeCommand("ToggleDesktopOverlay");
        }
        
        private void OnWinRCommand(HotKey cmd)
        {
            _commandService.InvokeCommand("ShowRunDialog");
        }

        private void OnWinECommand(HotKey cmd)
        {
            _commandService.InvokeCommand("OpenLocation", ("Path", "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"));
        }

        private void OnWinICommand(HotKey cmd)
        {
            _commandService.InvokeCommand("OpenControlPanel");
        }

        private void OnWinPauseCommand(HotKey cmd)
        {
            ShellHelper.StartProcess("system.cpl");
        }

        // TODO: Add window management related HotKeys
        // Win + [up]
        // Win + [down]
        // Win + [left]
        // Win + [right]
    }
}