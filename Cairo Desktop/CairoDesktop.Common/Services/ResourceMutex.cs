using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Common.Services
{
    /// <summary>
    ///     This protects your resources or application from running more than once
    ///     Simplifies the usage of the Mutex class, as described here:
    ///     https://msdn.microsoft.com/en-us/library/System.Threading.Mutex.aspx
    /// </summary>
    public sealed class ResourceMutex : IDisposable
    {
        private readonly ILogger _logger;

        private readonly string _mutexId;
        private readonly string _resourceName;
        private Mutex _applicationMutex;

        /// <summary>
        ///     Private constructor
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="mutexId">string with a unique Mutex ID</param>
        /// <param name="resourceName">optional name for the resource</param>
        private ResourceMutex(ILogger logger, string mutexId, string resourceName = null)
        {
            _logger = logger;
            _mutexId = mutexId;
            _resourceName = resourceName ?? mutexId;
        }

        /// <summary>
        ///     Test if the Mutex was created and locked.
        /// </summary>
        public bool IsLocked { get; private set; }

        /// <summary>
        ///     Create a ResourceMutex for the specified mutex id and resource-name
        /// </summary>
        /// <param name="logger">ILogger</param>
        /// <param name="mutexId">ID of the mutex, preferably a Guid as string</param>
        /// <param name="resourceName">Name of the resource to lock, e.g your application name, useful for logs</param>
        /// <param name="global">true to have a global mutex see: https://msdn.microsoft.com/en-us/library/bwe34f1k.aspx</param>
        public static ResourceMutex Create(ILogger logger, string mutexId, string resourceName = null, bool global = false)
        {
            if (mutexId == null)
            {
                throw new ArgumentNullException(nameof(mutexId));
            }

            logger = logger ?? new LoggerFactory().CreateLogger<ResourceMutex>();

            var applicationMutex = new ResourceMutex(logger, (global ? @"Global\" : @"Local\") + mutexId, resourceName);
            applicationMutex.Lock();
            return applicationMutex;
        }

        /// <summary>
        ///     This tries to get the Mutex, which takes care of having multiple instances running
        /// </summary>
        /// <returns>true if it worked, false if another instance is already running or something went wrong</returns>
        public bool Lock()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("{0} is trying to get Mutex {1}", _resourceName, _mutexId);
            }

            IsLocked = true;
            // check whether there's an local instance running already, but use local so this works in a multi-user environment
            try
            {
                // 1) Create Mutex
                _applicationMutex = new Mutex(true, _mutexId, out var createdNew);

                // 2) if the mutex wasn't created new get the right to it, this returns false if it's already locked
                if (!createdNew)
                {
                    IsLocked = _applicationMutex.WaitOne(2000, false);
                    if (!IsLocked)
                    {
                        _logger.LogWarning("Mutex {0} is already in use and couldn't be locked for the caller {1}", _mutexId, _resourceName);
                        // Clean up
                        _applicationMutex.Dispose();
                        _applicationMutex = null;
                    }
                    else
                    {
                        _logger.LogInformation("{0} has claimed the mutex {1}", _resourceName, _mutexId);
                    }
                }
                else
                {
                    _logger.LogInformation("{0} has created & claimed the mutex {1}", _resourceName, _mutexId);
                }
            }
            catch (AbandonedMutexException e)
            {
                // Another instance didn't cleanup correctly!
                // we can ignore the exception, it happened on the "WaitOne" but still the mutex belongs to us
                _logger.LogWarning(e, "{0} didn't cleanup correctly, but we got the mutex {1}.", _resourceName, _mutexId);
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.LogError(e, "{0} is most likely already running for a different user in the same session, can't create/get mutex {1} due to error.",
                    _resourceName, _mutexId);
                IsLocked = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problem obtaining the Mutex {1} for {0}, assuming it was already taken!", _resourceName, _mutexId);
                IsLocked = false;
            }
            return IsLocked;
        }

        //  To detect redundant Dispose calls
        private bool disposedValue;

        /// <summary>
        ///     Dispose the application mutex
        /// </summary>
        public void Dispose()
        {
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;
            if (_applicationMutex == null)
            {
                return;
            }
            try
            {
                if (IsLocked)
                {
                    _applicationMutex.ReleaseMutex();
                    IsLocked = false;
                    _logger.LogInformation("Released Mutex {0} for {1}", _mutexId, _resourceName);
                }
                _applicationMutex.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing Mutex {0} for {1}", _mutexId, _resourceName);
            }
            _applicationMutex = null;
        }
    }
}