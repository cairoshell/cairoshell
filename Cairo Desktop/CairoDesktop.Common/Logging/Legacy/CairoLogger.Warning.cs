using System;

namespace CairoDesktop.Common.Logging.Legacy
{
    public static partial class CairoLogger
    {
        public static void Warning(string message)
        {
            Warning(null, message, null);
        }

        public static void Warning(Exception exception)
        {
            Warning(null, null, exception);
        }

        public static void Warning(object sender, string message)
        {
            Warning(sender, message, null);
        }

        public static void Warning(object sender, Exception exception)
        {
            Warning(sender, null, exception);
        }

        public static void Warning(string message, Exception exception)
        {
            Warning(null, message, exception);
        }

        public static void Warning(object sender, string message, Exception exception)
        {
            if (_isWarning)
                OnLog(sender, new LogEventArgs(LogSeverity.Warning, message, exception, DateTime.Now));
        }

        public static void WarningIf(bool condition, string message)
        {
            WarningIf(condition, null, message, null);
        }

        public static void WarningIf(bool condition, Exception exception)
        {
            WarningIf(condition, null, null, exception);
        }

        public static void WarningIf(bool condition, object sender, string message)
        {
            WarningIf(condition, sender, message, null);
        }

        public static void WarningIf(bool condition, object sender, Exception exception)
        {
            WarningIf(condition, sender, null, exception);
        }

        public static void WarningIf(bool condition, string message, Exception exception)
        {
            WarningIf(condition, null, message, exception);
        }

        public static void WarningIf(bool condition, object sender, string message, Exception exception)
        {
            if (condition)
                Warning(sender, message, exception);
        }
    }
}