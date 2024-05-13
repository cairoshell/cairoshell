using ManagedShell.Common.Logging;
using System;
using System.Management;
using System.Security.Principal;

namespace CairoDesktop.SupportingClasses
{
    public class RegistryEventWatcher : IDisposable
    {
        private const string REGISTRY_HIVE = @"HKEY_USERS";
        private const string REGISTRY_VALUE = @"AccentColorMenu";
        private const string REGISTRY_KEY = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent";

        private readonly ManagementEventWatcher _watcher;
        private EventArrivedEventHandler _handler;
        private bool _running = false;
        private bool disposed = false;

        /// <summary>
        /// Defines a RegistryEventWatcher
        /// </summary>
        public RegistryEventWatcher()
        {
            WqlEventQuery query = new WqlEventQuery(GetQuery());
            _watcher = new ManagementEventWatcher(query);
        }

        /// <summary>
        /// Defines a RegistryEventWatcher with its handler
        /// </summary>
        /// <param name="handler">the EventArrivedEventHandler method which code will be executed on a registry change</param>
        public RegistryEventWatcher(EventArrivedEventHandler handler) : this()
        {
            SetHandler(handler);
        }

        /// <summary>
        /// Enables the event watcher, <i>subscribing</i> to the registry change event
        /// </summary>
        public void StartWatcher()
        {
            if(_handler == null) ShellLogger.Warning("RegistryEventWatcher: Registry event watcher started, but no handler has been set");
            this.SetWatchRegistry(true);
            this._running = true;
        }

        /// <summary>
        /// Disables the event watcher, <i><b>un</b>subscribing</i> to the registry change event
        /// </summary>
        public void StopWatcher()
        {
            this.SetWatchRegistry(false);
            this._running = false;
        }
        /// <summary>
        /// Stops the watcher and disposes the current resource.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.StopWatcher();
                    this._watcher.Dispose();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Sets the event handler, a method fired when an event
        /// </summary>
        /// <param name="handler">the EventArrivedEventHandler method which code will be executed on a registry change</param>
        public void SetHandler(EventArrivedEventHandler handler)
        {
            bool isRunning = this._running;
            if (isRunning) this.StopWatcher();
                this._handler = handler;
            if (isRunning) this.StartWatcher();
        }

        private String GetQuery()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            return $"SELECT * FROM RegistryValueChangeEvent WHERE Hive = '{REGISTRY_HIVE}' AND KeyPath = '{currentUser.User.Value}\\\\{REGISTRY_KEY}' AND ValueName = '{REGISTRY_VALUE}'";
        }

        private void SetWatchRegistry(bool subscribe)
        {
            _watcher.Stop();
            _watcher.EventArrived -= new EventArrivedEventHandler(this._handler);
            if (subscribe)
            {
                _watcher.EventArrived += new EventArrivedEventHandler(this._handler);
                _watcher.Start();
            }
        }
    }
}
