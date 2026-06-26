using CairoDesktop.Common;
using System;
using System.Globalization;
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
            dateText.ToolTip = FormatDateTimeWithIsoWeek(DateTime.Now, _settings.DateFormat);
        }

        private void UpdateText()
        {
            dateText.Text = FormatDateTimeWithIsoWeek(DateTime.Now, _settings.TimeFormat);
        }

        /// <summary>
        /// Formats a DateTime using standard .NET format strings with ISO week number support.
        /// Supports "W" for unpadded ISO week number and "WW" for zero-padded ISO week number.
        /// </summary>
        public static string FormatDateTimeWithIsoWeek(DateTime dateTime, string format)
        {
            if (string.IsNullOrEmpty(format))
            {
                return dateTime.ToString(CultureInfo.CurrentCulture);
            }

            var isoWeekNumber = GetIsoWeekNumber(dateTime);

            var result = format.Replace("WW", isoWeekNumber.ToString("D2"));
            result = result.Replace("W", isoWeekNumber.ToString());

            try
            {
                result = dateTime.ToString(result);
            }
            catch (FormatException)
            {
                result = dateTime.ToString(format);
            }

            return result;
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
        
        private static int GetIsoWeekNumber(DateTime date)
        {
#if NETCOREAPP
            return ISOWeek.GetWeekOfYear(date);
#else
            // ISO 8601 week number calculation for older .NET versions
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar
                .GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
#endif
        }
    }
}
