using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using System;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTray
{
    public class ShellServiceObject: IDisposable
    {
        const string CGID_SHELLSERVICEOBJECT = "000214D2-0000-0000-C000-000000000046";
        private IOleCommandTarget sysTrayObject;

        public void Start()
        {
            if (Shell.IsCairoRunningAsShell)
            {
                try
                {
                    sysTrayObject = (IOleCommandTarget)new SysTrayObject();
                    Guid sso = new Guid(CGID_SHELLSERVICEOBJECT);
                    sysTrayObject.Exec(ref sso, OLECMDID_NEW, OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {
                    CairoLogger.Instance.Debug("ShellServiceObject: Unable to start");
                }
            }
        }

        public void Dispose()
        {
            if (sysTrayObject != null)
            {
                try
                {
                    Guid sso = new Guid(CGID_SHELLSERVICEOBJECT);
                    sysTrayObject.Exec(ref sso, OLECMDID_SAVE, OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {
                    CairoLogger.Instance.Debug("ShellServiceObject: Unable to stop");
                }
            }
        }
    }
}
