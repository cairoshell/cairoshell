//using System.Collections.Generic;
//using System.Linq;
//using CairoDesktop.Common.Logging.Legacy;

//namespace CairoDesktop.Common.Logging.Observers
//{
//    public class LoggerLogAttacher
//    {
//        public LoggerLogAttacher(IEnumerable<ILog> cairoLogs, IEnumerable<ManagedShell.Common.Logging.ILog> managedShellLogs)
//        {
//            foreach (var log in cairoLogs)
//            {
//                CairoLogger.Attach(log);
//            }

//            foreach (var log in managedShellLogs)
//            {
//                ManagedShell.Common.Logging.ShellLogger.Attach(log);
//            }

//        }
//    }
//}