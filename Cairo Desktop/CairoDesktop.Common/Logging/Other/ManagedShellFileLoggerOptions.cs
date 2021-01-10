using System;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Logging.Other
{
    public class ManagedShellFileLoggerOptions
    {
        public ManagedShellFileLoggerOptions()
        {
            DateFormat = "MM-dd-yyyy";
            FileExtension = "log";

            var date = DateTime.Now.ToString(DateFormat);

            LogFileName = $"{date}.{FileExtension}";
            BackupLogFileName = $"{date}-Backup.{FileExtension}";
        }

        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public string LogsFolderPath { get; set; }

        public string LogFileName { get; set; }

        public string DateFormat { get; set; }

        public string FileExtension { get; set; }

        public string BackupLogFileName { get; set; }
    }
}