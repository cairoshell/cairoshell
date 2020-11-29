using System.Collections.Generic;
using CairoDesktop.Core.Objects;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoApplication
    {
        List<ShellExtension> Extensions { get; }
    }
}