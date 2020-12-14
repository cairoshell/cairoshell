using System;

namespace CairoDesktop.Common.Logging
{
    public static partial class CairoLogger
    {
        public static void Info(string message)
        {
            Info(null, message, null);
        }

        public static void Info(Exception exception)
        {
            Info(null, null, exception);
        }

        public static void Info(object sender, string message)
        {
            Info(sender, message, null);
        }

        public static void Info(object sender, Exception exception)
        {
            Info(sender, null, exception);
        }

        public static void Info(string message, Exception exception)
        {
            Info(null, message, exception);
        }

        public static void Info(object sender, string message, Exception exception)
        {
            if (_isInfo)
                OnLog(sender, new LogEventArgs(LogSeverity.Info, message, exception, DateTime.Now));
        }

        public static void InfoIf(bool condition, string message)
        {
            InfoIf(condition, null, message, null);
        }

        public static void InfoIf(bool condition, Exception exception)
        {
            InfoIf(condition, null, null, exception);
        }

        public static void InfoIf(bool condition, object sender, string message)
        {
            InfoIf(condition, sender, message, null);
        }

        public static void InfoIf(bool condition, object sender, Exception exception)
        {
            InfoIf(condition, sender, null, exception);
        }

        public static void InfoIf(bool condition, string message, Exception exception)
        {
            InfoIf(condition, null, message, exception);
        }

        public static void InfoIf(bool condition, object sender, string message, Exception exception)
        {
            if (condition)
                Info(sender, message, exception);
        }
    }
}