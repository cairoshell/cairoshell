using ManagedShell.Common.Logging;
using ManagedShell.Common.Logging.Observers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using ManagedShell.Common;

namespace CairoDesktop.Common.Logging.Other
{
    public class ManagedShellLoggerProvider : DisposableObject, ILoggerProvider
    {
        private readonly IOptionsMonitor<ManagedShellFileLoggerOptions> _options;
        private readonly Settings cairoSettings;
        private readonly ConcurrentDictionary<string, Lazy<ManagedShellLogger>> _loggers;
        private readonly IDisposable _settingsChangeToken;
        private ManagedShellFileLoggerOptions _settings;
        private string _filename;
        private FileLog _fileLog = null;

        public ManagedShellLoggerProvider(ManagedShellFileLoggerOptions settings, Settings cairoSettings)
        {
            _loggers = new ConcurrentDictionary<string, Lazy<ManagedShellLogger>>();
            _settings = settings;
            this.cairoSettings = cairoSettings;

            SetupLoggingSystem();

            SetupLoggingSeverity();

            cairoSettings.PropertyChanged += CairoSettings_PropertyChanged;

            try
            {
                _fileLog = new FileLog(_filename);
                _fileLog.Open();
            }
            catch (Exception ex)
            {
                ShellLogger.Error($"Unable to initialize file logger: {ex}");
            }

            if (_fileLog == null)
            {
                ShellLogger.Attach(new ConsoleLog(), true);
            }
            else
            {
                ShellLogger.Attach(new ILog[] { _fileLog, new ConsoleLog() }, true);
            }
        }

        private void CairoSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(cairoSettings.LogSeverity)) 
                return;

            SetupLoggingSeverity();
        }

        private void SetupLoggingSeverity()
        {
            var severity = cairoSettings.LogSeverity;
            
            ShellLogger.Severity = severity.ToManagedShellLogSeverity();
            _settings.LogLevel = severity.ToLogLevel();
        }

        public LogLevel LogLevel => _settings.LogLevel;

        public ManagedShellLoggerProvider(IOptionsMonitor<ManagedShellFileLoggerOptions> options, Settings cairoSettings) : this(options.CurrentValue, cairoSettings)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/change-tokens
            _options = options;
            _settingsChangeToken = _options.OnChange(settings =>
            {
                _settings = settings;
            });
        }

        private void SetupLoggingSystem()
        {
            // use the default logs folder
            string logsFolder = _settings.LogsFolderPath;

            // create the filename that will power the current log file listener
            string filename = Path.Combine(logsFolder, _settings.LogFileName);

            // make sure it exists
            if (!CreateLogsFolder(logsFolder))
            {
                // if it already existed, backup the existing log files
                BackupExistingLogFiles(logsFolder);

                // and then delete any that are too old
                DeleteOldLogFiles(logsFolder);
            }

            _filename = filename;
        }

        /// <summary>
        /// Creates the logs folder. Returns true if the directory was created, false otherwise.
        /// </summary>
        /// <param name="logsFolder">The directory to create.</param>
        /// <returns></returns>
        private bool CreateLogsFolder(string logsFolder)
        {
            try
            {
                if (Directory.Exists(logsFolder))
                {
                    return false;
                }

                Directory.CreateDirectory(logsFolder);
            }
            catch (Exception ex)
            {
                ShellLogger.Error($"Unable to create Logs folder: {ex}");
            }
            return true;
        }

        /// <summary>
        /// Backs up the existing log file, as the last run backup.
        /// </summary>
        /// <param name="logsFolder"></param>
        private void BackupExistingLogFiles(string logsFolder)
        {
            try
            {
                string currentFilename = Path.Combine(logsFolder, _settings.LogFileName);
                if (File.Exists(currentFilename))
                {
                    string backupFilename = Path.Combine(logsFolder, _settings.BackupLogFileName);
                    if (File.Exists(backupFilename))
                    {
                        File.Delete(backupFilename);
                    }

                    File.Move(currentFilename, backupFilename);
                }
            }
            catch (Exception ex)
            {
                ShellLogger.Error($"Unable to back up the previous log file: {ex}");
            }
        }

        /// <summary>
        /// Deletes any log files older than a week.
        /// </summary>
        /// <param name="logsFolder"></param>
        private void DeleteOldLogFiles(string logsFolder)
        {
            try
            {
                // look for all of the log files
                DirectoryInfo info = new DirectoryInfo(logsFolder);
                FileInfo[] files = info.GetFiles($"*.{_settings.FileExtension}", SearchOption.TopDirectoryOnly);

                // delete any files that are more than a week old
                DateTime now = DateTime.Now;
                TimeSpan allowedDelta = new TimeSpan(7, 0, 0);

                foreach (FileInfo file in files)
                {
                    if (now.Subtract(file.LastWriteTime) > allowedDelta)
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                ShellLogger.Error($"Unable to delete old log files: {ex}");
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            var lazyLogger = _loggers.GetOrAdd(categoryName, CairoFileLoggerValueFactory);

            var logger = lazyLogger.Value;

            return logger;
        }

        private Lazy<ManagedShellLogger> CairoFileLoggerValueFactory(string category)
        {
            return new Lazy<ManagedShellLogger>(() => new ManagedShellLogger(this, category));
        }

        protected override void DisposeOfManagedResources()
        {
            _fileLog?.Dispose();

            base.DisposeOfManagedResources();
        }
    }
}