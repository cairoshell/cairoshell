using CairoDesktop.Common;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using CairoDesktop.Application.Interfaces;
using ManagedShell.Common.Helpers;
using UserControl = System.Windows.Controls.UserControl;

namespace CairoDesktop.MenuBarExtensions
{
    public partial class LanguagePane : UserControl
    {
        private readonly ICommandService _commandService;
        private readonly Settings _settings;
        private readonly bool _isPrimaryScreen;

        public LanguagePane(IMenuBar host, ICommandService commandService, Settings settings)
        {
            InitializeComponent();

            _commandService = commandService;
            _settings = settings;

            _isPrimaryScreen = host.GetIsPrimaryDisplay();

            InitializeLang();
        }

        private void InitializeLang()
        {
            UpdateTextAndToolTip();

            // Create our timer for lang
            DispatcherTimer lang = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Background, Lang_Tick, Dispatcher);

            if (_isPrimaryScreen)
            {
                // register time changed handler to receive time zone updates for the lang to update correctly
                Microsoft.Win32.SystemEvents.TimeChanged += new EventHandler(UpdateText);
                Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            }
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.TimeChanged -= new EventHandler(UpdateText);
        }

        private void Lang_Tick(object sender, EventArgs args)
        {
            UpdateTextAndToolTip();
        }

        private void UpdateTextAndToolTip()
        {
            UpdateText(null, null);
            UpdateToolTip();
        }

        private void UpdateToolTip()
        {
            LangText.ToolTip = DateTime.Now.ToString(_settings.DateFormat);
        }

        private void UpdateText(object sender, EventArgs eventArgs)
        {
            LangText.Text = KeyboardLayoutHelper.GetKeyboardLayout().ThreeLetterName;
        }
    }
}