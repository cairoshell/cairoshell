using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Logging.Providers
{
    public class CairoLoggerProvider : DisposableObject, ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new Loggers.CairoLogger(CairoLogger.Instance);
        }
    }
}