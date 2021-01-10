using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CairoDesktop.Common.Logging
{

    public static class LogSeverityExtensions
    {
        private static readonly Dictionary<ManagedShell.Common.Logging.LogSeverity, LogLevel> ManagedShellLogSeverityToLogLevelDictionary = new Dictionary<ManagedShell.Common.Logging.LogSeverity, LogLevel>
        {
            {ManagedShell.Common.Logging.LogSeverity.Debug, LogLevel.Debug},
            {ManagedShell.Common.Logging.LogSeverity.Error, LogLevel.Error},
            {ManagedShell.Common.Logging.LogSeverity.Fatal, LogLevel.Critical},
            {ManagedShell.Common.Logging.LogSeverity.Info, LogLevel.Information},
            {ManagedShell.Common.Logging.LogSeverity.Warning, LogLevel.Warning}
        };

        private static readonly Dictionary<LogSeverity, LogLevel> CairoShellLogSeverityToLogLevelDictionary = new Dictionary<LogSeverity, LogLevel>
        {
            {LogSeverity.Debug, LogLevel.Debug},
            {LogSeverity.Error, LogLevel.Error},
            {LogSeverity.Fatal, LogLevel.Critical},
            {LogSeverity.Info, LogLevel.Information},
            {LogSeverity.Warning, LogLevel.Warning}
        };

        private static readonly Dictionary<LogLevel, ManagedShell.Common.Logging.LogSeverity> LogLevelToManagedShellLogSeverityDictionary = new Dictionary<LogLevel, ManagedShell.Common.Logging.LogSeverity>
        {
            {LogLevel.Debug, ManagedShell.Common.Logging.LogSeverity.Debug},
            {LogLevel.Error, ManagedShell.Common.Logging.LogSeverity.Error},
            {LogLevel.Critical, ManagedShell.Common.Logging.LogSeverity.Fatal},
            {LogLevel.Information, ManagedShell.Common.Logging.LogSeverity.Info},
            {LogLevel.Warning, ManagedShell.Common.Logging.LogSeverity.Warning}
        };

        private static readonly Dictionary<LogSeverity, ManagedShell.Common.Logging.LogSeverity> CairoShellLogSeverityToManagedShellLogSeverityDictionary = new Dictionary<LogSeverity, ManagedShell.Common.Logging.LogSeverity>
        {
            {LogSeverity.Debug, ManagedShell.Common.Logging.LogSeverity.Debug},
            {LogSeverity.Error, ManagedShell.Common.Logging.LogSeverity.Error},
            {LogSeverity.Fatal, ManagedShell.Common.Logging.LogSeverity.Fatal},
            {LogSeverity.Info, ManagedShell.Common.Logging.LogSeverity.Info},
            {LogSeverity.Warning, ManagedShell.Common.Logging.LogSeverity.Warning}
        };

        public static LogLevel ToLogLevel(this LogSeverity logSeverity)
        {
            return CairoShellLogSeverityToLogLevelDictionary[logSeverity];
        }

        public static LogLevel ToLogLevel(this ManagedShell.Common.Logging.LogSeverity logSeverity)
        {
            return ManagedShellLogSeverityToLogLevelDictionary[logSeverity];
        }

        public static ManagedShell.Common.Logging.LogSeverity ToManagedShellLogSeverity(this LogSeverity logSeverity)
        {
            return CairoShellLogSeverityToManagedShellLogSeverityDictionary[logSeverity];
        }

        public static ManagedShell.Common.Logging.LogSeverity ToManagedShellLogSeverity(this LogLevel logLevel)
        {
            return LogLevelToManagedShellLogSeverityDictionary[logLevel];
        }
    }
}