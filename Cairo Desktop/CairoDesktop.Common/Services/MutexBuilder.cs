using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Services
{
    /// <summary>
    /// This is the configuration for the mutex service
    /// </summary>
    internal class MutexBuilder : IMutexBuilder
    {
        /// <inheritdoc />
        public string MutexId { get; set; }

        /// <inheritdoc />
        public bool IsGlobal { get; }

        /// <inheritdoc />
        public Action<IHostEnvironment, ILogger> WhenNotFirstInstance { get; set; }
    }
}