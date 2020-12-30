using System;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Logging.Other
{
    //public class CairoLogger : ILogger
    //{
    //    public IDisposable BeginScope<TState>(TState state)
    //    {
    //        return null;
    //    }

    //    public bool IsEnabled(LogLevel logLevel)
    //    {
    //        return logLevel != LogLevel.None;
    //    }

    //    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    //    {
    //        if (!IsEnabled(logLevel))
    //        {
    //            return;
    //        }

    //        switch (logLevel)
    //        {
    //            case LogLevel.Trace:
    //            case LogLevel.Debug:
    //                Legacy.CairoLogger.Debug(formatter(state, exception));
    //                break;
    //            case LogLevel.Information:
    //                Legacy.CairoLogger.Info(formatter(state, exception));
    //                break;
    //            case LogLevel.Warning:
    //                Legacy.CairoLogger.Warning(formatter(state, exception));
    //                break;
    //            case LogLevel.Error:
    //                Legacy.CairoLogger.Error(formatter(state, exception));
    //                break;
    //            case LogLevel.Critical:
    //                Legacy.CairoLogger.Fatal(formatter(state, exception));
    //                break;
    //            case LogLevel.None:
    //            default:
    //                break;
    //        }
    //    }
    //}
}