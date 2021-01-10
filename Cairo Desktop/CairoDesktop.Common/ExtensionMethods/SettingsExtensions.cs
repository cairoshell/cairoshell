using System;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;

namespace CairoDesktop.Common.ExtensionMethods
{
    public static class SettingsExtensions
    {
        public static LogSeverity GetLogSeverity(this Settings settings, LogSeverity defaultValue)
        {
            if (Enum.TryParse(settings.LogSeverity, out LogSeverity result))
                return result;

            settings.LogSeverity = defaultValue.ToString();
            result = defaultValue;

            return result;
        }
    }
}