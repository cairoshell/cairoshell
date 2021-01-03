using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Services
{
    /// <summary>
    /// This is to configure the ForceSingleInstance extension
    /// </summary>
    public interface IMutexBuilder
    {
        /// <summary>
        /// The name of the mutex, usually a GUID
        /// </summary>
        string MutexId { get; set; }

        /// <summary>
        /// This decides what prefix the mutex name gets, true will prepend Global\ and false Local\
        /// </summary>
        bool IsGlobal { get; }

        /// <summary>
        /// The action which is called when the mutex cannot be locked
        /// </summary>
        Action<IHostEnvironment, ILogger> WhenNotFirstInstance { get; set; }
    }
}