﻿using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Common.Logging.Observers;
using CairoDesktop.Configuration;
using CairoDesktop.Extensibility.ObjectModel;
using CairoDesktop.Extensibility.ObjectModel.Services;
using CairoDesktop.Interop;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.IO;

namespace CairoDesktop
{
    public partial class Startup
    {
        private static void ProcessCommandLineArgs(string[] args)
        {
            commandLineParser = new CommandLineParser(args);

            isRestart = commandLineParser.ToBoolean("restart");
            isTour = commandLineParser.ToBoolean("tour");
        }

        private static void SingleInstanceCheck()
        {
            cairoMutex = new System.Threading.Mutex(true, "CairoShell", out bool ok);

            if (!ok && !isRestart)
            {
                // Another instance is already running.
                return;
            }
            else if (!ok && isRestart)
            {
                // this is a restart so let's wait for the old instance to end
                System.Threading.Thread.Sleep(2000);
            }
        }

        private static void SetShellReadyEvent()
        {
            int hShellReadyEvent;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Shell.IsWindows2kOrBetter)
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, true, @"Global\msgina: ShellReadyEvent");
            else
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, "msgina: ShellReadyEvent");

            if (hShellReadyEvent != 0)
            {
                NativeMethods.SetEvent(hShellReadyEvent);
                NativeMethods.CloseHandle(hShellReadyEvent);
            }
        }

        private static void SetupSettings()
        {
            if (Settings.IsFirstRun == true)
                Settings.Upgrade();
        }

        private static void SetupLoggingSystem()
        {
            // use the default logs folder
            string logsFolder = _CairoShell.LogsFolder;

            // create the filename that will power the current log file listener
            string filename = Path.Combine(logsFolder, DefaultLogName);

            // make sure it exists
            if (!CreateLogsFolder(logsFolder))
            {
                // if it already existed, backup the existing log files
                BackupExistingLogFiles(logsFolder);

                // and then delete any that are too old
                DeleteOldLogFiles(logsFolder);
            }

            var fileLog = new FileLog(filename);
            fileLog.Open();

            CairoLogger.Instance.Severity = GetLogSeveritySetting(defaultValue: LogSeverity.Info);
            CairoLogger.Instance.Attach(fileLog);
            CairoLogger.Instance.Attach(new ConsoleLog());
        }

        private static LogSeverity GetLogSeveritySetting(LogSeverity defaultValue)
        {
            if (!Enum.TryParse(Settings.LogSeverity, out LogSeverity result))
            {
                Settings.LogSeverity = defaultValue.ToString();
                result = defaultValue;
            }

            return result;
        }

        #region Log File Management

        /// <summary>
        /// Returns the default date format used to name log files.
        /// </summary>
        private static string DefaultDateFormat
        {
            get
            {
                return "MM-dd-yyyy";
            }
        }

        /// <summary>
        /// Returns the default file extension used to name log files.
        /// </summary>
        private static string DefaultLogFileExtension
        {
            get
            {
                return "log";
            }
        }

        /// <summary>
        /// Returns the default name of the log file.
        /// </summary>
        private static string DefaultLogName
        {
            get
            {
                return string.Format("{0}.{1}", DateTime.Now.ToString(DefaultDateFormat), DefaultLogFileExtension);
            }
        }

        /// <summary>
        /// Returns the default name of the backup log file.
        /// </summary>
        private static string DefaultBackupLogName
        {
            get
            {
                return string.Format("{0}-Backup.{1}", DateTime.Now.ToString(DefaultDateFormat), DefaultLogFileExtension);
            }
        }

        /// <summary>
        /// Creates the logs folder. Returns true if the directory was created, false otherwise.
        /// </summary>
        /// <param name="logsFolder">The directory to create.</param>
        /// <returns></returns>
        private static bool CreateLogsFolder(string logsFolder)
        {
            try
            {
                if (Directory.Exists(logsFolder))
                    return false;
                Directory.CreateDirectory(logsFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return true;
        }

        /// <summary>
        /// Backs up the existing log file, as the last run backup.
        /// </summary>
        /// <param name="logsFolder"></param>
        private static void BackupExistingLogFiles(string logsFolder)
        {
            try
            {
                // calc the current filename
                string currentFilename = Path.Combine(logsFolder, DefaultLogName);

                // look for it, if it exists, then back it up
                if (File.Exists(currentFilename))
                {
                    // calc the backup name
                    string backupFilename = Path.Combine(logsFolder, DefaultBackupLogName);

                    // if there was a previous backup, delete it
                    if (File.Exists(backupFilename))
                        File.Delete(backupFilename);

                    // move the current file to the backup location
                    File.Move(currentFilename, backupFilename);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Deletes any log files older than a week.
        /// </summary>
        /// <param name="logsFolder"></param>
        private static void DeleteOldLogFiles(string logsFolder)
        {
            try
            {
                // look for all of the log files
                DirectoryInfo info = new DirectoryInfo(logsFolder);
                FileInfo[] files = info.GetFiles(string.Format("*.{0}", DefaultLogFileExtension), SearchOption.TopDirectoryOnly);

                // delete any files that are more than a week old
                DateTime now = DateTime.Now;
                TimeSpan allowedDelta = new TimeSpan(7, 0, 0);

                foreach (FileInfo file in files)
                {
                    if (now.Subtract(file.LastWriteTime) > allowedDelta)
                        file.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        #endregion

        internal static void SetupPluginSystem()
        {
            PluginService pluginService = new PluginService();
            _CairoShell.Instance.ShellServices.Add(pluginService.GetType(), pluginService);
            pluginService.Start();
        }

        internal static void WriteApplicationDebugInfoToConsole()
        {
            const string @break = @"#############################################";

            CairoLogger.Instance.Info(@break);
            CairoLogger.Instance.Info(string.Format(@"{0}", _CairoShell.ProductName));
            CairoLogger.Instance.Info(string.Format(@"Version: {0}", _CairoShell.ProductVersion));
            CairoLogger.Instance.Info(string.Format(@"Operating System: {0}", new ComputerInfo().OSFullName)); //outputs the OS type based on version
            CairoLogger.Instance.Info(string.Format(@"OS Build: {0}", new ComputerInfo().OSVersion));
            CairoLogger.Instance.Info(string.Format(@"Processor Type: {0}", string.Format(@"{0}-bit", IntPtr.Size == 8 || InternalCheckIsWow64() ? 64 : 32)));
            CairoLogger.Instance.Info(string.Format(@"Startup Path: {0}", _CairoShell.StartupPath));
            CairoLogger.Instance.Info(string.Format(@"Running As: {0}-bit Process", IntPtr.Size * 8));
            CairoLogger.Instance.Info(@break);
        }

        internal static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;

                    try
                    {
                        if (!NativeMethods.IsWow64Process(p.Handle, out retVal))
                            return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    return retVal;
                }
            }
            return false;
        }

        internal static void SetSystemKeyboardShortcuts()
        {
            if (Shell.IsCairoUserShell)
            {
                // Commenting out as per comments on PR #274
                //SystemHotKeys.RegisterSystemHotkeys();
            }
        }
    }
}