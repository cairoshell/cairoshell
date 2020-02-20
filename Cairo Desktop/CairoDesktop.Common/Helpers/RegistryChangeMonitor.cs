namespace CairoDesktop.Common.Helpers
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32;
    using static CairoDesktop.Interop.NativeMethods;

    public delegate void RegistryChangeHandler(object sender, RegistryChangeEventArgs e);

    public class RegistryChangeEventArgs : EventArgs
    {
        public RegistryChangeEventArgs(RegistryChangeMonitor monitor)
        {
            Monitor = monitor;
        }

        public RegistryChangeMonitor Monitor { get; }

        public Exception Exception { get; set; }

        public bool Stop { get; set; }
    }

    /// <summary>
    /// Credit to the nice guys at https://www.pinvoke.net/default.aspx/advapi32/RegNotifyChangeKeyValue.html
    /// </summary>
    public class RegistryChangeMonitor : IDisposable
    {
        private REG_NOTIFY_CHANGE _filter;
        private Thread _monitorThread;
        private RegistryKey _monitorKey;

        public RegistryChangeMonitor(string registryPath) : this(registryPath, REG_NOTIFY_CHANGE.LAST_SET) {; }

        public RegistryChangeMonitor(string registryPath, REG_NOTIFY_CHANGE filter)
        {
            RegistryPath = registryPath.ToUpper();
            _filter = filter;
        }

        ~RegistryChangeMonitor()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                GC.SuppressFinalize(this);

            Stop();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Start()
        {
            lock (this)
            {
                if (_monitorThread == null)
                {
                    ThreadStart ts = new ThreadStart(MonitorThread);
                    _monitorThread = new Thread(ts) { IsBackground = true };
                }

                if (!_monitorThread.IsAlive)
                {
                    _monitorThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (this)
            {
                Changed = null;
                Error = null;

                if (_monitorThread != null)
                {
                    _monitorThread = null;
                }

                // The "Close()" will trigger RegNotifyChangeKeyValue if it is still listening
                if (_monitorKey != null)
                {
                    _monitorKey.Close();
                    _monitorKey = null;
                }
            }
        }

        private void MonitorThread()
        {
            try
            {
                IntPtr ptr = IntPtr.Zero;

                lock (this)
                {
                    if (RegistryPath.StartsWith("HKEY_CLASSES_ROOT"))
                        _monitorKey = Registry.ClassesRoot.OpenSubKey(RegistryPath.Substring(18));
                    else if (RegistryPath.StartsWith("HKCR"))
                        _monitorKey = Registry.ClassesRoot.OpenSubKey(RegistryPath.Substring(5));
                    else if (RegistryPath.StartsWith("HKEY_CURRENT_USER"))
                        _monitorKey = Registry.CurrentUser.OpenSubKey(RegistryPath.Substring(18));
                    else if (RegistryPath.StartsWith("HKCU"))
                        _monitorKey = Registry.CurrentUser.OpenSubKey(RegistryPath.Substring(5));
                    else if (RegistryPath.StartsWith("HKEY_LOCAL_MACHINE"))
                        _monitorKey = Registry.LocalMachine.OpenSubKey(RegistryPath.Substring(19));
                    else if (RegistryPath.StartsWith("HKLM"))
                        _monitorKey = Registry.LocalMachine.OpenSubKey(RegistryPath.Substring(5));
                    else if (RegistryPath.StartsWith("HKEY_USERS"))
                        _monitorKey = Registry.Users.OpenSubKey(RegistryPath.Substring(11));
                    else if (RegistryPath.StartsWith("HKU"))
                        _monitorKey = Registry.Users.OpenSubKey(RegistryPath.Substring(4));
                    else if (RegistryPath.StartsWith("HKEY_CURRENT_CONFIG"))
                        _monitorKey = Registry.CurrentConfig.OpenSubKey(RegistryPath.Substring(20));
                    else if (RegistryPath.StartsWith("HKCC"))
                        _monitorKey = Registry.CurrentConfig.OpenSubKey(RegistryPath.Substring(5));

                    // Fetch the native handle
                    if (_monitorKey != null)
                    {
                        object hkey = typeof(RegistryKey).InvokeMember(
                           "hkey",
                           BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                           null,
                           _monitorKey,
                           null
                           );

                        ptr = (IntPtr)typeof(SafeHandle).InvokeMember(
                           "handle",
                           BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic,
                           null,
                           hkey,
                           null);
                    }
                }

                if (ptr != IntPtr.Zero)
                {
                    while (true)
                    {
                        // If this._monitorThread is null that probably means Dispose is being called. Don't monitor anymore.
                        if ((_monitorThread == null) || (_monitorKey == null))
                            break;

                        // RegNotifyChangeKeyValue blocks until a change occurs.
                        int result = Interop.NativeMethods.RegNotifyChangeKeyValue(ptr, true, _filter, IntPtr.Zero, false);

                        if ((_monitorThread == null) || (_monitorKey == null))
                            break;

                        if (result == 0)
                        {
                            if (Changed != null)
                            {
                                RegistryChangeEventArgs e = new RegistryChangeEventArgs(this);
                                Changed(this, e);

                                if (e.Stop) break;
                            }
                        }
                        else
                        {
                            if (Error != null)
                            {
                                Win32Exception ex = new Win32Exception();

                                // Unless the exception is thrown, nobody is nice enough to set a good stacktrace for us. Set it ourselves.
                                typeof(Exception).InvokeMember(
                                "_stackTrace",
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField,
                                null,
                                ex,
                                new object[] { new StackTrace(true) }
                                );

                                RegistryChangeEventArgs e = new RegistryChangeEventArgs(this) { Exception = ex };
                                Error(this, e);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Error != null)
                {
                    RegistryChangeEventArgs e = new RegistryChangeEventArgs(this) { Exception = ex };
                    Error(this, e);
                }
            }
            finally
            {
                Stop();
            }
        }

        public event RegistryChangeHandler Changed;
        public event RegistryChangeHandler Error;

        public bool Monitoring
        {
            get
            {
                if (_monitorThread != null)
                    return _monitorThread.IsAlive;

                return false;
            }
        }

        public string RegistryPath { get; }
    }
}