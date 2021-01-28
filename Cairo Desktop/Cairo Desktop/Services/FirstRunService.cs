using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Configuration;
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
        private readonly AppGrabberService _appGrabber;
        private readonly ICairoApplication _cairoApplication;
        private readonly ILogger<FirstRunService> _logger;

        private readonly bool _forceTour;

        public FirstRunService(ICairoApplication cairoApplication, AppGrabberService appGrabber, ILogger<FirstRunService> logger, IOptionsMonitor<CommandLineOptions> options)
        {
            _appGrabber = appGrabber;
            _cairoApplication = cairoApplication;
            _logger = logger;

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
            if (Settings.Instance.IsFirstRun || _forceTour)
            {
                _cairoApplication.Dispatch(() =>
                {
                    ShowWelcome();
                });
            }
        }

        private void ShowWelcome()
        {
            Welcome welcome = new Welcome(_cairoApplication, _appGrabber);
            welcome.Show();
        }
    }
}
