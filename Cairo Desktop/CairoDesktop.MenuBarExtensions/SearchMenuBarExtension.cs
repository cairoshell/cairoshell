using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Logging;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop.MenuBarExtensions
{
    class SearchMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly Settings _settings;
        private List<Search> _searchList = new List<Search>();

        internal SearchMenuBarExtension(ICairoApplication cairoApplication, Settings settings)
        {
            _cairoApplication = cairoApplication;
            _settings = settings;

            try
            {
                new HotKey(Key.S, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnShowSearchHotkey);
            }
            catch (Exception ex)
            {
                ShellLogger.Warning($"Search: Unable to bind hotkey: {ex.Message}");
            }
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableMenuExtraSearch) 
                return null;

            Search search = new Search(_cairoApplication, host);
            _searchList.Add(search);
            return search;
        }

        public override void StopControl(IMenuBar host)
        {
            List<Search> toRemove = new List<Search>();

            foreach (Search search in _searchList)
            {
                if (search.Host != host)
                {
                    continue;
                }

                search.Host = null;
                toRemove.Add(search);
            }

            foreach (Search search in toRemove)
            {
                _searchList.Remove(search);
            }

            toRemove.Clear();
        }

        private void OnShowSearchHotkey(HotKey hotKey)
        {
            int x = System.Windows.Forms.Cursor.Position.X;
            int y = System.Windows.Forms.Cursor.Position.Y;

            foreach (Search search in _searchList)
            {
                if (_settings.EnableMenuBarMultiMon)
                {
                    System.Windows.Forms.Screen clockScreen = System.Windows.Forms.Screen.FromHandle(search.Host.GetHandle());

                    if (x >= clockScreen.Bounds.Left && x <= clockScreen.Bounds.Right &&
                        y >= clockScreen.Bounds.Top && y <= clockScreen.Bounds.Bottom)
                    {
                        search.ToggleSearch();
                        return;
                    }

                    continue;
                }

                search.ToggleSearch();
            }
        }
    }
}