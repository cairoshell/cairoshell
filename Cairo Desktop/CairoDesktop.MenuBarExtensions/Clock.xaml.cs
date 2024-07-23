using CairoDesktop.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class Clock : UserControl
    {
        private readonly ICommandService _commandService;
        private readonly Settings _settings;
        private readonly bool _isPrimaryScreen;
        private static bool isClockHotkeyRegistered;

        public Clock(IMenuBar host, ICommandService commandService, Settings settings)
        {
            InitializeComponent();

            _commandService = commandService;
            _settings = settings;

            _isPrimaryScreen = host.GetIsPrimaryDisplay();

            InitializeClock();
        }

        private void InitializeClock()
        {
            UpdateTextAndToolTip();

            // Create our timer for clock
            DispatcherTimer clock = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, Clock_Tick, Dispatcher);

            if (_isPrimaryScreen)
            {
                // register time changed handler to receive time zone updates for the clock to update correctly
                Microsoft.Win32.SystemEvents.TimeChanged += new EventHandler(TimeChanged);
                Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

                try
                {
                    if (!isClockHotkeyRegistered)
                    {
                        new HotKey(Key.D, HotKeyModifier.Win | HotKeyModifier.Alt | HotKeyModifier.NoRepeat, OnShowClock);
                        isClockHotkeyRegistered = true;
                    }
                }
                catch { }
            }
        }

        private void OnShowClock(HotKey hotKey)
        {
            ToggleClockDisplay();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.TimeChanged -= new EventHandler(TimeChanged);
        }

        private void Clock_Tick(object sender, EventArgs args)
        {
            UpdateTextAndToolTip();
        }

        private void UpdateTextAndToolTip()
        {
            UpdateText();
            UpdateToolTip();
        }

        private void UpdateToolTip()
        {
            dateText.ToolTip = DateTime.Now.ToString(_settings.DateFormat);
        }

        private void UpdateText()
        {
            dateText.Text = DateTime.Now.ToString(_settings.TimeFormat);
        }

        private void OpenTimeDateCPL(object sender, RoutedEventArgs e)
        {
            _commandService.InvokeCommand("OpenDateTimeControlPanel");
        }

        private void ClockMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            monthCalendar.DisplayDate = DateTime.Now;
        }

        private void TimeChanged(object sender, EventArgs e)
        {
            TimeZoneInfo.ClearCachedData();
        }

        public void ToggleClockDisplay()
        {
            ClockMenuItem.IsSubmenuOpen = !ClockMenuItem.IsSubmenuOpen;
        }
    }
}