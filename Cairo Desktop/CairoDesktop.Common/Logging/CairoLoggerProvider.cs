//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Concurrent;
//using System.Diagnostics;
//using System.IO;
//using System.Text;

//namespace CairoDesktop.Common.Logging
//{
//    public class CairoFileLoggerProvider : DisposableObject, ILoggerProvider
//    {
//        private readonly IOptionsMonitor<CairoFileLoggerOptions> _options;
//        private readonly ConcurrentDictionary<string, Lazy<CairoFileLogger>> _loggers;
//        private readonly IDisposable _settingsChangeToken;
//        private CairoFileLoggerOptions _settings;

//        private FileInfo _fileInfo;
//        private TextWriter _textWriter;

//        public CairoFileLoggerProvider(CairoFileLoggerOptions settings)
//        {
//            _loggers = new ConcurrentDictionary<string, Lazy<CairoFileLogger>>();
//            _settings = settings;

//            SetupLoggingSystem();
//        }

//        public CairoFileLoggerProvider(IOptionsMonitor<CairoFileLoggerOptions> options) : this(options.CurrentValue)
//        {
//            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/change-tokens
//            _options = options;
//            _settingsChangeToken = _options.OnChange(settings =>
//            {
//                _settings = settings;
//            });
//        }

//        public LogLevel LogLevel => _settings.LogLevel;

//        private void SetupLoggingSystem()
//        {
//            // use the default logs folder
//            string logsFolder = _settings.LogsFolderPath;

//            // create the filename that will power the current log file listener
//            string filename = Path.Combine(logsFolder, _settings.LogFileName);

//            // make sure it exists
//            if (!CreateLogsFolder(logsFolder))
//            {
//                // if it already existed, backup the existing log files
//                BackupExistingLogFiles(logsFolder);

//                // and then delete any that are too old
//                DeleteOldLogFiles(logsFolder);
//            }

//            _fileInfo = new FileInfo(filename);
//            var stream = File.AppendText(_fileInfo.FullName);
//            stream.AutoFlush = true;

//            _textWriter = TextWriter.Synchronized(stream);
//        }

//        /// <summary>
//        /// Creates the logs folder. Returns true if the directory was created, false otherwise.
//        /// </summary>
//        /// <param name="logsFolder">The directory to create.</param>
//        /// <returns></returns>
//        private bool CreateLogsFolder(string logsFolder)
//        {
//            try
//            {
//                if (Directory.Exists(logsFolder))
//                {
//                    return false;
//                }

//                Directory.CreateDirectory(logsFolder);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//            return true;
//        }

//        /// <summary>
//        /// Backs up the existing log file, as the last run backup.
//        /// </summary>
//        /// <param name="logsFolder"></param>
//        private void BackupExistingLogFiles(string logsFolder)
//        {
//            try
//            {
//                string currentFilename = Path.Combine(logsFolder, _settings.LogFileName);
//                if (File.Exists(currentFilename))
//                {
//                    string backupFilename = Path.Combine(logsFolder, _settings.BackupLogFileName);
//                    if (File.Exists(backupFilename))
//                    {
//                        File.Delete(backupFilename);
//                    }

//                    File.Move(currentFilename, backupFilename);
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//        }

//        /// <summary>
//        /// Deletes any log files older than a week.
//        /// </summary>
//        /// <param name="logsFolder"></param>
//        private void DeleteOldLogFiles(string logsFolder)
//        {
//            try
//            {
//                // look for all of the log files
//                DirectoryInfo info = new DirectoryInfo(logsFolder);
//                FileInfo[] files = info.GetFiles($"*.{_settings.FileExtension}", SearchOption.TopDirectoryOnly);

//                // delete any files that are more than a week old
//                DateTime now = DateTime.Now;
//                TimeSpan allowedDelta = new TimeSpan(7, 0, 0);

//                foreach (FileInfo file in files)
//                {
//                    if (now.Subtract(file.LastWriteTime) > allowedDelta)
//                    {
//                        file.Delete();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//        }

//        public ILogger CreateLogger(string categoryName)
//        {
//            var lazyLogger = _loggers.GetOrAdd(categoryName, CairoFileLoggerValueFactory);

//            var logger = lazyLogger.Value;

//            return logger;
//        }

//        private Lazy<CairoFileLogger> CairoFileLoggerValueFactory(string category)
//        {
//            return new Lazy<CairoFileLogger>(() => new CairoFileLogger(this, category));
//        }

//        protected override void DisposeOfManagedResources()
//        {
//            base.DisposeOfManagedResources();

//            try
//            {
//                _textWriter.Flush();
//                _textWriter.Close();
//                _textWriter.Dispose();
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex);
//            }
//        }

//        public void Log<TState>(string category, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
//        {
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append($"[{DateTime.Now}]");

//            if (!string.IsNullOrWhiteSpace(category))
//                stringBuilder.Append($" [{category}]");

//            stringBuilder.AppendLine($" {LogLevel}: {formatter(state, exception)}");

//            if (exception != null)
//            {
//                stringBuilder.AppendLine("\t:::Exception Details:::");
//                foreach (string line in exception.ToString().Split(new[] {Environment.NewLine}, StringSplitOptions.None))
//                {
//                    stringBuilder.AppendLine("\t" + line);
//                }
//            }

//            try
//            {
//                _textWriter.Write(stringBuilder.ToString());
//                _textWriter.Flush();
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"Error writing to log: {ex}");
//            }
//        }
//    }
//}