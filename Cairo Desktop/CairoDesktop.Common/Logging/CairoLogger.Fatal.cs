using System;

namespace CairoDesktop.Common.Logging
{
    public static partial class CairoLogger
    {
        public static void Fatal(string message)
        {
            Fatal(null, message, null);
        }

        public static void Fatal(Exception exception)
        {
            Fatal(null, null, exception);
        }

        public static void Fatal(object sender, string message)
        {
            Fatal(sender, message, null);
        }

        public static void Fatal(object sender, Exception exception)
        {
            Fatal(sender, null, exception);
        }

        public static void Fatal(string message, Exception exception)
        {
            Fatal(null, message, exception);
        }

        public static void Fatal(object sender, string message, Exception exception)
        {
            if (_isFatal)
                OnLog(sender, new LogEventArgs(LogSeverity.Fatal, message, exception, DateTime.Now));
        }

        public static void FatalIf(bool condition, string message)
        {
            FatalIf(condition, null, message, null);
        }

        public static void FatalIf(bool condition, Exception exception)
        {
            FatalIf(condition, null, null, exception);
        }

        public static void FatalIf(bool condition, object sender, string message)
        {
            FatalIf(condition, sender, message, null);
        }

        public static void FatalIf(bool condition, object sender, Exception exception)
        {
            FatalIf(condition, sender, null, exception);
        }

        public static void FatalIf(bool condition, string message, Exception exception)
        {
            FatalIf(condition, null, message, exception);
        }

        public static void FatalIf(bool condition, object sender, string message, Exception exception)
        {
            if (condition)
                Fatal(sender, message, exception);
        }
    }
}