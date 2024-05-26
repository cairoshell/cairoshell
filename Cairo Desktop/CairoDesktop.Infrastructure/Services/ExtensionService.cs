﻿using CairoDesktop.Application.Interfaces;
using System.Collections.Generic;

namespace CairoDesktop.Infrastructure.Services;

public sealed class ExtensionService : IExtensionService
{
    private readonly ICairoApplication _app;
    private readonly IEnumerable<IShellExtension> _extensions;

    public ExtensionService(ICairoApplication app, IEnumerable<IShellExtension> extensions)
    {
        _app = app;
        _extensions = extensions;
    }

    public void Start()
    {
        foreach (var shellExtension in _extensions)
        {
            shellExtension.Start();
            _app.Extensions.Add(shellExtension);
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