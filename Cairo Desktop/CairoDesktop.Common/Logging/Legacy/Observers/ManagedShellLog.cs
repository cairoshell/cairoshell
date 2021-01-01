using ManagedShell.Common.Logging;

namespace CairoDesktop.Common.Logging.Legacy.Observers
{
    public class ManagedShellLog : ILog
    {
        public void Log(object sender, LogEventArgs e)
        {
            ShellLogger.OnLog(new ManagedShell.Common.Logging.LogEventArgs(e.Severity.ToManagedShellLogSeverity(), e.Message, e.Exception, e.Date));
        }
    }
}