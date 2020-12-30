using System;

namespace CairoDesktop.Common.Logging.Legacy
{
    [Obsolete("Retiring in favor of Microsoft.Extensions.Logging")]
    public static partial class CairoLogger
    {
        public delegate void LogEventHandler(object sender, LogEventArgs e);

        // These Booleans are used strictly to improve performance.
        private static bool _isDebug;
        private static bool _isError;
        private static bool _isFatal;
        private static bool _isInfo;
        private static bool _isWarning;
        private static LogSeverity _severity;

        static CairoLogger()
        {
            // Default severity is Debug level
            Severity = LogSeverity.Debug;
        }

        public static LogSeverity Severity
        {
            get { return _severity; }
            set
            {
                _severity = value;

                // Set Booleans to help improve performance
                var severity = (int)_severity;

                _isDebug = ((int)LogSeverity.Debug) >= severity;
                _isInfo = ((int)LogSeverity.Info) >= severity;
                _isWarning = ((int)LogSeverity.Warning) >= severity;
                _isError = ((int)LogSeverity.Error) >= severity;
                _isFatal = ((int)LogSeverity.Fatal) >= severity;
            }
        }

        public static event LogEventHandler Log;

        public static void OnLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }

        public static void Attach(ILog observer)
        {
            Log += observer.Log;
        }

        public static void Detach(ILog observer)
        {
            Log -= observer.Log;
        }
    }
}