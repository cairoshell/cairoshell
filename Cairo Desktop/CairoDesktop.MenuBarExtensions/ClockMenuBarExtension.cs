using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop.MenuBarExtensions
{
    class ClockMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICommandService _commandService;
        private readonly Settings _settings;
        private List<Clock> _clocks = new List<Clock>();

        internal ClockMenuBarExtension(ICommandService commandService, Settings settings)
        {
            _commandService = commandService;
            _settings = settings;

            try
            {
                new HotKey(Key.D, HotKeyModifier.Win | HotKeyModifier.Alt | HotKeyModifier.NoRepeat, OnShowClock);
            }
            catch { }

            // register time changed handler to receive time zone updates for the clock to update correctly
            Microsoft.Win32.SystemEvents.TimeChanged += new EventHandler(TimeChanged);
        }

        public override UserControl StartControl(IMenuBar host)
        {
            if (!_settings.EnableMenuExtraClock)
            {
                return null;
            }
            
            Clock clock = new Clock(host, _commandService, _settings);
            _clocks.Add(clock);
            return clock;
        }

        public override void StopControl(IMenuBar host)
        {
            List<Clock> toRemove = new List<Clock>();

            foreach (Clock clock in _clocks)
            {
                if (clock.Host != host)
                {
                    continue;
                }

                clock.Host = null;
                toRemove.Add(clock);
            }

            foreach (Clock clock in toRemove)
            {
                _clocks.Remove(clock);
            }

            toRemove.Clear();
        }

        private void OnShowClock(HotKey hotKey)
        {
            int x = System.Windows.Forms.Cursor.Position.X;
            int y = System.Windows.Forms.Cursor.Position.Y;

            foreach (Clock clock in _clocks)
            {
                if (_settings.EnableMenuBarMultiMon)
                {
                    System.Windows.Forms.Screen clockScreen = System.Windows.Forms.Screen.FromHandle(clock.Host.GetHandle());

                    if (x >= clockScreen.Bounds.Left && x <= clockScreen.Bounds.Right &&
                        y >= clockScreen.Bounds.Top && y <= clockScreen.Bounds.Bottom)
                    {
                        clock.ToggleClockDisplay();
                        return;
                    }

                    continue;
                }

                clock.ToggleClockDisplay();
            }
        }

        private void TimeChanged(object sender, EventArgs e)
        {
            TimeZoneInfo.ClearCachedData();
        }
    }
}