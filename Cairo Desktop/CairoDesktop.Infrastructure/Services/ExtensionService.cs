using CairoDesktop.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CairoDesktop.Infrastructure.Services
{
    public sealed class ExtensionService : IExtensionService
    {
        private readonly ICairoApplication _app;
        private readonly IEnumerable<IShellExtension> _extensions;
        private readonly ILogger<ExtensionService> _logger;

        public ExtensionService(ICairoApplication app, ILogger<ExtensionService> logger, IEnumerable<IShellExtension> extensions)
        {
            _app = app;
            _extensions = extensions;
            _logger = logger;
        }

        public void Start()
        {
            foreach (var shellExtension in _extensions)
            {
                try
                {
                    shellExtension.Start();
                    _app.Extensions.Add(shellExtension);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to start shell extension: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            foreach (var shellExtension in _extensions)
            {
                _app.Extensions.Remove(shellExtension);
                shellExtension.Stop();
            }
        }
    }
}