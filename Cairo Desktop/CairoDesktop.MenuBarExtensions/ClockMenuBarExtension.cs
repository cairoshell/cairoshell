using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.Common.Helpers;

namespace CairoDesktop.MenuBarExtensions
{
    class ClockMenuBarExtension : UserControlMenuBarExtension
    {
        private readonly ICommandService _commandService;
        private readonly Settings _settings;
        private readonly List<Clock> _clocks = new List<Clock>();

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

        public override void StopControl(IMenuBar host, UserControl control)
        {
            if (control is Clock clock && _clocks.Contains(clock))
            {
                clock.Host = null;
                _clocks.Remove(clock);
            }
        }

        private void OnShowClock(HotKey hotKey)
        {
            foreach (Clock clock in _clocks)
            {
                if (_settings.EnableMenuBarMultiMon)
                {
                    if (CursorHelper.IsCursorOnScreen(System.Windows.Forms.Screen.FromHandle(clock.Host.GetHandle())))
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