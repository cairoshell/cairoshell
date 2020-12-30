using CairoDesktop.Common.Logging.Legacy;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Logging.Observers
{
    public class CairoShellLoggerObserver : ILog
    {
        private readonly ILogger _logger;

        public CairoShellLoggerObserver(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("CairoShell.ShellLogger");
        }

        public void Log(object sender, LogEventArgs e)
        {
            var date = e.Date;
            var severity = e.Severity;
            var message = e.Message;
            var exception = e.Exception;

            _logger.Log(severity.ToLogLevel(), exception, message);
        }
    }
}