using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuExtraClock.xaml
    /// </summary>
    public partial class MenuExtraClock : UserControl
    {
        private bool _isPrimaryScreen;
        private static bool isClockHotkeyRegistered;

        public MenuExtraClock(MenuBar menuBar)
        {
            InitializeComponent();

            _isPrimaryScreen = menuBar.Screen.Primary;

            initializeClock();
        }

        /// <summary>
        /// Initializes the dispatcher timers to updates the time and date bindings
        /// </summary>
        private void initializeClock()
        {
            // initial display
            clock_Tick(null, null);

            // Create our timer for clock
            DispatcherTimer clock = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, clock_Tick, Dispatcher);

            if (_isPrimaryScreen)
            {
                // register time changed handler to receive time zone updates for the clock to update correctly
                Microsoft.Win32.SystemEvents.TimeChanged += new EventHandler(TimeChanged);
                Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

                try
                {
                    if (!isClockHotkeyRegistered)
                    {
                        new HotKey(Key.D, KeyModifier.Win | KeyModifier.Alt | KeyModifier.NoRepeat, OnShowClock);
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

        private void clock_Tick(object sender, EventArgs args)
        {
            dateText.Text = DateTime.Now.ToString(Settings.Instance.TimeFormat);
            dateText.ToolTip = DateTime.Now.ToString(Settings.Instance.DateFormat);
        }

        private void OpenTimeDateCPL(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("timedate.cpl");
        }

        private void miClock_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            monthCalendar.DisplayDate = DateTime.Now;
        }

        private void TimeChanged(object sender, EventArgs e)
        {
            TimeZoneInfo.ClearCachedData();
        }

        public void ToggleClockDisplay()
        {
            miClock.IsSubmenuOpen = !miClock.IsSubmenuOpen;
        }
    }
}
