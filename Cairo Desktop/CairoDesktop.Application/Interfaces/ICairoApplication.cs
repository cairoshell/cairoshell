using System;
using System.Collections.Generic;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoApplication
    {
        List<IShellExtension> Extensions { get; }

        List<IMenuItem> Places { get; }
    }
}