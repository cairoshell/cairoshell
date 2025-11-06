using CairoDesktop.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CairoDesktop.Application.Interfaces;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class Clock : UserControl
    {
        internal IMenuBar Host;

        private readonly ICommandService _commandService;
        private readonly Settings _settings;

        private DispatcherTimer _clock;

        public Clock(IMenuBar host, ICommandService commandService, Settings settings)
        {
            InitializeComponent();

            Host = host;

            _commandService = commandService;
            _settings = settings;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeClock();
        }

        private void InitializeClock()
        {
            UpdateTextAndToolTip();

            // Create our timer for clock
            _clock = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Background, Clock_Tick, Dispatcher);
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

        public void ToggleClockDisplay()
        {
            ClockMenuItem.IsSubmenuOpen = !ClockMenuItem.IsSubmenuOpen;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _clock.Stop();
        }
    }
}