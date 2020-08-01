using CairoDesktop.Common.Logging;
using CairoDesktop.ObjectModel;
using System;
using System.ComponentModel.Composition;
using CairoDesktop.Interop;

namespace CairoDesktop.Extensions.VirtualDesktopSupport
{

    [Export(typeof(ShellExtension))]
    public sealed class VirtualDesktopSupportExtension : ShellExtension
    {
        public VirtualDesktopSupportExtension()
        {

        }

        private bool _started = false;

        public override void Start()
        {
            if (Shell.IsCairoRunningAsShell)
                return;

            CairoLogger.Instance.Debug($"VirtualDesktopSupportExtension_Starting");

            // Perform Start

            _started = true;
            CairoLogger.Instance.Debug($"VirtualDesktopSupportExtension_Started");
        }


        public override void Stop()
        {
            if (!_started)
                return;

            CairoLogger.Instance.Debug($"VirtualDesktopSupportExtension_Stopping");

            // Perform Stop

            _started = false;
            CairoLogger.Instance.Debug($"VirtualDesktopSupportExtension_Stopped");
        }
    }
}