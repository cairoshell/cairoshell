using System;
using System.IO;

namespace CairoDesktop.Common.Logging.Observers
{
    /// <summary>
    /// Writes log events to a local file.
    /// </summary>
    /// <remarks>
    /// GoF Design Pattern: Observer.
    /// The Observer Design Pattern allows this class to attach itself to an
    /// the logger and 'listen' to certain events and be notified of the event. 
    /// </remarks>
    public class FileLog : DisposableLogBase
    {
        private readonly FileInfo _fileInfo;
        private TextWriter _textWriter;

        /// <summary>
        /// Constructor of ObserverLogToFile.
        /// </summary>
        /// <param name="fileName">File log to.</param>
        public FileLog(string fileName)
        {
            _fileInfo = new FileInfo(fileName);
        }

        public void Open()
        {
            if (!Directory.Exists(_fileInfo.DirectoryName))
                Directory.CreateDirectory(_fileInfo.DirectoryName);

            // trim existing log file
            if (File.Exists(_fileInfo.FullName))
            {
                int size = 512 * 1024; // 512 KB is enough log for us, but trim at 1 MB so that we aren't trimming every startup
                if (_fileInfo.Length > (size * 2))
                {
                    using (MemoryStream ms = new MemoryStream(size))
                    {
                        using (FileStream s = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite))
                        {
                            s.Seek(-size, SeekOrigin.End);
                            s.CopyTo(ms);
                            s.SetLength(size);
                            s.Position = 0;
                            ms.Position = 0; // Begin from the start of the memory stream
                            ms.CopyTo(s);
                        }
                    }
                }
            }

            var stream = File.AppendText(_fileInfo.FullName);
            stream.AutoFlush = true;

            _textWriter = TextWriter.Synchronized(stream);
        }

        protected override void DisposeOfManagedResources()
        {
            base.DisposeOfManagedResources();

            try
            {
                _textWriter.Flush();
                _textWriter.Close();
                _textWriter.Dispose();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Write a log request to a file.
        /// </summary>
        /// <param name="sender">Sender of the log request.</param>
        /// <param name="e">Parameters of the log request.</param>
        public override void Log(object sender, LogEventArgs e)
        {
            string message = string.Format("[{0}] {1}: {2}", e.Date, e.SeverityString, e.Message);

            try
            {
                _textWriter.WriteLine(message);
                _textWriter.Flush();
            }
            catch (Exception)
            {
                /* do nothing */
            }
        }
    }
}