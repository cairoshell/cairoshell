using System;

namespace CairoDesktop.Common.Logging.Legacy
{
    public static partial class CairoLogger
    {
        public static void Debug(string message)
        {
            Debug(null, message, null);
        }

        public static void Debug(Exception exception)
        {
            Debug(null, null, exception);
        }

        public static void Debug(object sender, string message)
        {
            Debug(sender, message, null);
        }

        public static void Debug(object sender, Exception exception)
        {
            Debug(sender, null, exception);
        }

        public static void Debug(string message, Exception exception)
        {
            Debug(null, message, exception);
        }

        public static void Debug(object sender, string message, Exception exception)
        {
            if (_isDebug)
                OnLog(sender, new LogEventArgs(LogSeverity.Debug, message, exception, DateTime.Now));
        }

        public static void DebugIf(bool condition, string message)
        {
            DebugIf(condition, null, message, null);
        }

        public static void DebugIf(bool condition, Exception exception)
        {
            DebugIf(condition, null, null, exception);
        }

        public static void DebugIf(bool condition, object sender, string message)
        {
            DebugIf(condition, sender, message, null);
        }

        public static void DebugIf(bool condition, object sender, Exception exception)
        {
            DebugIf(condition, sender, null, exception);
        }

        public static void DebugIf(bool condition, string message, Exception exception)
        {
            DebugIf(condition, null, message, exception);
        }

        public static void DebugIf(bool condition, object sender, string message, Exception exception)
        {
            if (condition)
                Debug(sender, message, exception);
        }
    }
}