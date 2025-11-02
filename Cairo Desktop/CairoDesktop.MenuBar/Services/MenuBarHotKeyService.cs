using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.DependencyInjection;
using ManagedShell.Common.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CairoDesktop.MenuBar.Services
{
    public sealed class MenuBarHotKeyService : CairoBackgroundService
    {
        private HotKey cairoMenuHotKey;
        private List<HotKey> programsMenuHotKeys = new List<HotKey>();

        // private LowLevelKeyboardListener keyboardListener; // temporarily removed due to stuck key issue, commented out to prevent warnings

        private readonly ICairoApplication _cairoApplication;
        private readonly ICommandService _commandService;
        private readonly Settings _settings;

        public MenuBarHotKeyService(ICairoApplication cairoApplication, ICommandService commandService, Settings settings)
        {
            _cairoApplication = cairoApplication;
            _settings = settings;
            _settings.PropertyChanged += Settings_PropertyChanged;

            ServiceStartTask = new Task(RegisterHotkeys);
            _commandService = commandService;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName)) return;

            switch (e.PropertyName)
            {
                case nameof(_settings.CairoMenuHotKey):
                    if (_settings.EnableCairoMenuHotKey)
                    {
                        unregisterCairoMenuHotKey();
                        registerCairoMenuHotKey();
                    }

                    break;
                case nameof(_settings.EnableCairoMenuHotKey):
                    if (_settings.EnableCairoMenuHotKey)
                    {
                        registerCairoMenuHotKey();
                    }
                    else
                    {
                        unregisterCairoMenuHotKey();
                    }

                    break;
                case nameof(_settings.EnableProgramsMenu):
                case nameof(_settings.EnableWinKey):
                    if (_settings.EnableWinKey && _settings.EnableProgramsMenu)
                    {
                        registerWinKey();
                    }
                    else
                    {
                        unregisterWinKey();
                    }
                    break;
            }
        }

        private void RegisterHotkeys()
        {
            registerCairoMenuHotKey();
            registerWinKey();
        }

        private void registerCairoMenuHotKey()
        {
            if (!_settings.EnableCairoMenuHotKey)
            {
                return;
            }

            _cairoApplication.Dispatch(() =>
            {
                cairoMenuHotKey = HotKeyManager.RegisterHotKey(_settings.CairoMenuHotKey, OnToggleCairoMenu);
            });
        }

        private void registerWinKey()
        {
            if (!EnvironmentHelper.IsAppRunningAsShell || !_settings.EnableWinKey || !_settings.EnableProgramsMenu)
            {
                return;
            }

            _cairoApplication.Dispatch(() =>
            {
                /*if (keyboardListener == null)
                    keyboardListener = new LowLevelKeyboardListener();

                keyboardListener.OnKeyPressed += keyboardListener_OnKeyPressed;
                keyboardListener.HookKeyboard();*/

                programsMenuHotKeys.Add(new HotKey(Key.LWin, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnToggleProgramsMenu));
                programsMenuHotKeys.Add(new HotKey(Key.RWin, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnToggleProgramsMenu));
                programsMenuHotKeys.Add(new HotKey(Key.Escape, HotKeyModifier.Ctrl | HotKeyModifier.NoRepeat, OnToggleProgramsMenu));
            });
        }

        private void unregisterCairoMenuHotKey()
        {
            cairoMenuHotKey?.Dispose();
            cairoMenuHotKey = null;
        }

        private void unregisterWinKey()
        {
            foreach (var hotkey in programsMenuHotKeys)
            {
                hotkey.Unregister();
            }

            programsMenuHotKeys.Clear();
        }

        private void OnToggleCairoMenu(HotKey obj)
        {
            _commandService.InvokeCommand("ToggleCairoMenu");
        }

        private void OnToggleProgramsMenu(HotKey obj)
        {
            _commandService.InvokeCommand("ToggleProgramsMenu");
        }
    }
}
