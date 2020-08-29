using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;

namespace CairoDesktop
{
    public partial class MenuExtraClock : UserControl
    {
        private readonly bool _isPrimaryScreen;
        private static bool isClockHotkeyRegistered;

        public MenuExtraClock(MenuBar menuBar)
        {
            InitializeComponent();

            _isPrimaryScreen = menuBar.Screen.Primary;

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
            dateText.ToolTip = DateTime.Now.ToString(Settings.Instance.DateFormat);
        }

        private void UpdateText()
        {
            dateText.Text = DateTime.Now.ToString(Settings.Instance.TimeFormat);
        }

        private void OpenTimeDateCPL(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("timedate.cpl");
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
