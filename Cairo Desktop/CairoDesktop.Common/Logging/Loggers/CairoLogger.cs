using Microsoft.Extensions.Logging;
using System;

namespace CairoDesktop.Common.Logging.Loggers
{
    public class CairoLogger : ILogger
    {
        private Logging.CairoLogger _legacyCairoLogger;

        public CairoLogger(Logging.CairoLogger legacyCairoLogger)
        {
            _legacyCairoLogger = legacyCairoLogger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    _legacyCairoLogger.Debug(formatter(state, exception));
                    break;
                case LogLevel.Information:
                    _legacyCairoLogger.Info(formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    _legacyCairoLogger.Warning(formatter(state, exception));
                    break;
                case LogLevel.Error:
                    _legacyCairoLogger.Error(formatter(state, exception));
                    break;
                case LogLevel.Critical:
                    _legacyCairoLogger.Fatal(formatter(state, exception));
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }
}