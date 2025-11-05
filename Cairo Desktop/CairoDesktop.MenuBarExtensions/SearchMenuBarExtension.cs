using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.Logging;
using ManagedShell.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace CairoDesktop.MenuBarExtensions
{
    class SearchMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly Settings _settings;
        private readonly List<Search> _searchList = new List<Search>();
        private ObjectDataProvider _odp;
        private DispatcherTimer _searchCheck;

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

            if (_settings.EnableMenuExtraSearch)
            {
                SetupSearch();
            }
            _settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(_settings.EnableMenuExtraSearch))
            {
                if (_odp == null && _settings.EnableMenuExtraSearch)
                {
                    SetupSearch();
                }
            }
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableMenuExtraSearch) 
                return null;

            Search search = new Search(host, _odp);
            _searchList.Add(search);
            return search;
        }

        public override void StopControl(IMenuBar host, UserControl control)
        {
            if (control is Search search && _searchList.Contains(search))
            {
                search.Host = null;
                _searchList.Remove(search);
            }
        }

        private void SetupSearch()
        {
            // Show the search button only if the service is running
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                SetSearchProvider();
            }
            else
            {
                _searchCheck = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.CurrentDispatcher)
                {
                    Interval = new TimeSpan(0, 0, 5)
                };
                _searchCheck.Tick += searchCheck_Tick;
                _searchCheck.Start();
            }
        }

        private void searchCheck_Tick(object sender, EventArgs e)
        {
            if (WindowsServices.QueryStatus("WSearch") == ServiceStatus.Running)
            {
                SetSearchProvider();
                (sender as DispatcherTimer)?.Stop();
            }
        }

        private void SetSearchProvider()
        {
            if (_odp != null)
            {
                return;
            }

            var thread = new Thread(() =>
            {
                // this sometimes takes a while
                Type provider = typeof(SearchHelper);

                _cairoApplication.Dispatch(() =>
                {
                    _odp = new ObjectDataProvider
                    {
                        ObjectType = provider
                    };

                    foreach (var search in _searchList)
                    {
                        search.SetSearchProvider(_odp);
                    }
                });
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void OnShowSearchHotkey(HotKey hotKey)
        {
            int x = System.Windows.Forms.Cursor.Position.X;
            int y = System.Windows.Forms.Cursor.Position.Y;

            foreach (Search search in _searchList)
            {
                if (_settings.EnableMenuBarMultiMon)
                {
                    var clockScreen = System.Windows.Forms.Screen.FromHandle(search.Host.GetHandle());

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