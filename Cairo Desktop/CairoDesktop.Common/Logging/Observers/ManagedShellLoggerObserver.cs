using ManagedShell.Common.Logging;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Logging.Observers
{
    public class ManagedShellLoggerObserver : ILog
    {
        private readonly ILogger _logger;

        public ManagedShellLoggerObserver(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("ManagedShell.ShellLogger");
        }

        public void Log(object sender, ManagedShell.Common.Logging.LogEventArgs e)
        {
            var date = e.Date;
            var severity = e.Severity;
            var message = e.Message;
            var exception = e.Exception;

            _logger.Log(severity.ToLogLevel(), exception, message);
        }
    }
}