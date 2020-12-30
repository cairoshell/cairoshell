using System;

namespace CairoDesktop.Common.Logging.Legacy
{
    public static partial class CairoLogger
    {
        public static void Error(string message)
        {
            Error(null, message, null);
        }

        public static void Error(Exception exception)
        {
            Error(null, null, exception);
        }

        public static void Error(object sender, string message)
        {
            Error(sender, message, null);
        }

        public static void Error(object sender, Exception exception)
        {
            Error(sender, null, exception);
        }

        public static void Error(string message, Exception exception)
        {
            Error(null, message, exception);
        }

        public static void Error(object sender, string message, Exception exception)
        {
            if (_isError)
                OnLog(sender, new LogEventArgs(LogSeverity.Error, message, exception, DateTime.Now));
        }

        public static void ErrorIf(bool condition, string message)
        {
            ErrorIf(condition, null, message, null);
        }

        public static void ErrorIf(bool condition, Exception exception)
        {
            ErrorIf(condition, null, null, exception);
        }

        public static void ErrorIf(bool condition, object sender, string message)
        {
            ErrorIf(condition, sender, message, null);
        }

        public static void ErrorIf(bool condition, object sender, Exception exception)
        {
            ErrorIf(condition, sender, null, exception);
        }

        public static void ErrorIf(bool condition, string message, Exception exception)
        {
            ErrorIf(condition, null, message, exception);
        }

        public static void ErrorIf(bool condition, object sender, string message, Exception exception)
        {
            if (condition)
                Error(sender, message, exception);
        }
    }
}