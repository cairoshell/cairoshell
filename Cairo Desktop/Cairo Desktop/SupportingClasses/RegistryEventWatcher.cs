using CairoDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CairoDesktop.SupportingClasses
{
    public sealed class RegistryEventWatcher : IDisposable
    {
        private const string REGISTRY_HIVE = @"HKEY_USERS";
        private const string REGISTRY_VALUE = @"AccentColorMenu";
        private const string REGISTRY_KEY = @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent";

        private readonly ManagementEventWatcher _watcher;
        private readonly CairoApplicationThemeService _themeService;

        public RegistryEventWatcher(CairoApplicationThemeService ts)
        {
            WqlEventQuery query = new WqlEventQuery(GetQuery());
            _watcher = new ManagementEventWatcher(query);
            _themeService = ts;
            WatchRegistry(true);
            _watcher.Start();
        }

        private String GetQuery()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            return $"SELECT * FROM RegistryValueChangeEvent WHERE Hive = '{REGISTRY_HIVE}' AND KeyPath = '{currentUser.User.Value}\\\\{REGISTRY_KEY}' AND ValueName = '{REGISTRY_VALUE}'";
        }

        public void WatchRegistry(bool subscribe)
        {
            _watcher.EventArrived -= new EventArrivedEventHandler(SystemEvents_RegistryChanged);
            if (subscribe)
            {
                _watcher.EventArrived += new EventArrivedEventHandler(SystemEvents_RegistryChanged);
            }
        }

        private void SystemEvents_RegistryChanged(object sender, EventArrivedEventArgs e)
        {
            _themeService.SetThemeFromSettings();
        }

        public void Dispose()
        {
            this.WatchRegistry(false);
            this._watcher.Stop();
            this._watcher.Dispose();
        }
    }
}
