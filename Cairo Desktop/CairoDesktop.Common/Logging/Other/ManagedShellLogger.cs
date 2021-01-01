using ManagedShell.Common.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace CairoDesktop.Common.Logging.Other
{
    public class ManagedShellLogger : ILogger
    {
        private readonly ManagedShellLoggerProvider _provider;
        private readonly string _category;

        public ManagedShellLogger(ManagedShellLoggerProvider managedShellLoggerProvider, string category)
        {
            _provider = managedShellLoggerProvider;
            _category = category;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None)
                return false;

            if (_provider.LogLevel == LogLevel.None)
                return false;

            bool result = Convert.ToInt32(logLevel) >= Convert.ToInt32(_provider.LogLevel);

            return result;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = GetCategoryPrefix(_category) + formatter(state, exception);
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    ShellLogger.Debug(message);
                    break;
                case LogLevel.Information:
                    ShellLogger.Info(message);
                    break;
                case LogLevel.Warning:
                    ShellLogger.Warning(message);
                    break;
                case LogLevel.Error:
                    ShellLogger.Error(message);
                    break;
                case LogLevel.Critical:
                    ShellLogger.Fatal(message);
                    break;
                case LogLevel.None:
                default:
                    break;
            }
        }

        private string GetCategoryPrefix(string category)
        {
            if (string.IsNullOrWhiteSpace(_category))
                return string.Empty;

            return $"[{_category}] ";
        }
    }
}