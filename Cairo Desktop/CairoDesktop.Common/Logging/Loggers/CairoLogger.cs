using Microsoft.Extensions.Logging;
using System;

namespace CairoDesktop.Common.Logging.Loggers
{
    public class CairoLogger : ILogger
    {
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
                    Logging.CairoLogger.Debug(formatter(state, exception));
                    break;
                case LogLevel.Information:
                    Logging.CairoLogger.Info(formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    Logging.CairoLogger.Warning(formatter(state, exception));
                    break;
                case LogLevel.Error:
                    Logging.CairoLogger.Error(formatter(state, exception));
                    break;
                case LogLevel.Critical:
                    Logging.CairoLogger.Fatal(formatter(state, exception));
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }
    }
}