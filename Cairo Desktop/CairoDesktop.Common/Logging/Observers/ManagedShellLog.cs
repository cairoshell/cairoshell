using ManagedShell.Common.Logging;

namespace CairoDesktop.Common.Logging.Observers
{
    public class ManagedShellLog : Legacy.ILog
    {
        public void Log(object sender, Legacy.LogEventArgs e)
        {
            ShellLogger.OnLog(new LogEventArgs(e.Severity.ToManagedShellLogSeverity(), e.Message, e.Exception, e.Date));
        }
    }
}