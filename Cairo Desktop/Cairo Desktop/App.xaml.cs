using CairoDesktop.Common.Logging;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Configuration.Settings.ForceSoftwareRendering)
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }

        private static bool errorVisible = false;

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            string inner = "";
            if (e.Exception.InnerException != null)
                inner = "\r\n\r\nInner exception:\r\nMessage: " + e.Exception.InnerException.Message + "\r\nTarget Site: " + e.Exception.InnerException.TargetSite + "\r\n\r\n" + e.Exception.InnerException.StackTrace;

            string msg = "Would you like to restart Cairo?\r\n\r\nPlease submit a bug report with a screenshot of this error. Thanks! \r\nMessage: " + e.Exception.Message + "\r\nTarget Site: " + e.Exception.TargetSite + "\r\nVersion: " + version + "\r\n\r\n" + e.Exception.StackTrace + inner;

            CairoLogger.Instance.Error(msg, e.Exception);

            string dMsg;

            if (msg.Length > 1000)
                dMsg = msg.Substring(0, 999) + "...";
            else
                dMsg = msg;

            try
            {
                if (!errorVisible)
                {
                    errorVisible = true;

                    // Automatically restart for known render thread failure messages.
                    if (e.Exception.Message.StartsWith("UCEERR_RENDERTHREADFAILURE"))
                    {
                        CairoDesktop.Startup.Restart();
                        Environment.FailFast("Automatically restarted Cairo due to a render thread failure.");
                    }
                    else
                    {
                        if (MessageBox.Show(dMsg, "Cairo Desktop Error", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            // it's like getting a morning coffee.
                            CairoDesktop.Startup.Restart();
                            Environment.FailFast("User restarted Cairo due to an exception.");
                        }
                    }

                    errorVisible = false;
                }
            }
            catch
            {
                // If this fails we're probably up the creek. Abandon ship!
                CairoDesktop.Startup.Shutdown();
            }

            e.Handled = true;
        }
    }
}
