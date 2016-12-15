using System.Windows;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UnhandledExceptionFilter.UnhandledExceptionManager.AddHandler(false);
            base.OnStartup(e);
        }
    }
}
