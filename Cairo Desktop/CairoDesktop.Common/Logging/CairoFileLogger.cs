//using System;
//using Microsoft.Extensions.Logging;

//namespace CairoDesktop.Common.Logging
//{
//    public class CairoFileLogger : ILogger
//    {
//        private readonly CairoFileLoggerProvider _provider;
//        private readonly string _category;

//        public CairoFileLogger(CairoFileLoggerProvider cairoFileLoggerProvider, string category)
//        {
//            _provider = cairoFileLoggerProvider;
//            _category = category;
//        }

//        public IDisposable BeginScope<TState>(TState state)
//        {
//            return NullScope.Instance;
//        }

//        public bool IsEnabled(LogLevel logLevel)
//        {
//            if (logLevel == LogLevel.None)
//                return false;

//            if (_provider.LogLevel == LogLevel.None)
//                return false;

//            bool result = Convert.ToInt32(logLevel) >= Convert.ToInt32(_provider.LogLevel);

//            return result;
//        }

//        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
//        {
//            if (IsEnabled(logLevel))
//            {
//                _provider.Log(_category, logLevel, eventId, state, exception, formatter);
//            }
//        }
//    }
//}