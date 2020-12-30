using Microsoft.Extensions.Logging;
using System;

namespace CairoDesktop.Common.Logging
{
    public class CairoFileLoggerOptions
    {
        public CairoFileLoggerOptions()
        {
            DateFormat = "MM-dd-yyyy";
            FileExtension = "log";

            var date = DateTime.Now.ToString(DateFormat);

            LogFileName = $"{date}.{FileExtension}";
            BackupLogFileName = $"{date}-Backup.{FileExtension}";
        }

        public LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;

        public string LogsFolderPath { get; set; }

        public string LogFileName { get; set; }

        public string DateFormat { get; set; }

        public string FileExtension { get; set; }

        public string BackupLogFileName { get; set; }
    }
}