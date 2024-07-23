using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.DependencyInjection;
using CairoDesktop.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace CairoDesktop.Services
{
    class FirstRunService : CairoBackgroundService
    {
        private readonly IAppGrabber _appGrabber;
        private readonly ICairoApplication _cairoApplication;
        private readonly ILogger<FirstRunService> _logger;
        private readonly Settings _settings;

        private readonly bool _forceTour;

        public FirstRunService(ICairoApplication cairoApplication, IAppGrabber appGrabber, ILogger<FirstRunService> logger, IOptionsMonitor<CommandLineOptions> options, Settings settings)
        {
            _appGrabber = appGrabber;
            _cairoApplication = cairoApplication;
            _logger = logger;
            _settings = settings;

            try
            {
                _forceTour = options.CurrentValue.Tour;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to read tour command line option: {e.Message}");
            }

            ServiceStartTask = new Task(FirstRun);
        }

        private void FirstRun()
        {
            if (_settings.IsFirstRun || _forceTour)
            {
                _cairoApplication.Dispatch(() =>
                {
                    ShowWelcome();
                });
            }
        }

        private void ShowWelcome()
        {
            Welcome welcome = new Welcome(_cairoApplication, _appGrabber, _settings);
            welcome.Show();
        }
    }
}
