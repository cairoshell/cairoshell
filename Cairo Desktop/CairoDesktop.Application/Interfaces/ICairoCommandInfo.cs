using System.Collections.Generic;
using CairoDesktop.Application.Structs;

namespace CairoDesktop.Application.Interfaces
{
    public interface ICairoCommandInfo
    {
        /// <summary>
        /// Unique identifier for the command. If already in use, the command will not be registered.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Short description of the command. May be shown in configuration windows or tooltips.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Short label which will be the primary visual identification of the command.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Returns whether or not the command is available to be invoked.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// List of parameters that can be passed when invoking the command.
        /// </summary>
        IReadOnlyCollection<CairoCommandParameter> Parameters { get; }
    }
}