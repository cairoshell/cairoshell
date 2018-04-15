using CairoDesktop.Common;
using CairoDesktop.Common.Helpers;
using CairoDesktop.Extensibility.Providers;
using System;
using System.Reflection;
using System.Threading;

namespace CairoDesktop.Extensibility.Plugins
{
    /// <summary>
    /// Defines an object in which Plugins are hosted.
    /// </summary>
    public sealed class PluginContext : DisposableObject
    {
        private static PluginContext _context;

        private Timer _gcTimer;
        private PluginDescriptorCollection _pluginDescriptors;
        private PluginProviderCollection _pluginProviders;

        #region PluginContextException

        /// <summary>
        /// Defines the base PluginContext exception that is thrown by the PluginContext class.
        /// </summary>
        public abstract class PluginContextException : ApplicationException
        {
            private readonly PluginContext _existingContext;

            /// <summary>
            /// Initializes a new instance of the PluginContextException class
            /// </summary>
            /// <param name="context">The PluginContext around which the exception is based</param>
            /// <param name="message"></param>
            protected PluginContextException(PluginContext context, string message)
                : base(message)
            {
                _existingContext = context;
            }

            /// <summary>
            /// Returns the PluginContext around which the exception is based
            /// </summary>
            public PluginContext ExistingContext
            {
                get { return _existingContext; }
            }
        }

        #endregion

        #region PluginContextAlreadyExistsException

        /// <summary>
        /// Defines an exception that is generated as a result of attempting
        /// to create more than one PluginContext per application.
        /// </summary>
        public sealed class PluginContextAlreadyExistsException : PluginContextException
        {
            /// <summary>
            /// Initializes a new instance of the PluginContextAlreadyExistsException class
            /// </summary>
            /// <param name="context">The PluginContext that already exists</param>
            internal PluginContextAlreadyExistsException(PluginContext context) :
                base(context, string.Format("A PluginContext already exists for the AppDomain '{0}'. Only one context can exist per application.", AppDomain.CurrentDomain.FriendlyName))
            {
            }
        }

        #endregion

        #region PluginContextAlreadyRunningException

        /// <summary>
        /// Defines an exception that is throw by the PluginContext class if the Run method is called more than one time.
        /// </summary>
        public sealed class PluginContextAlreadyRunningException : PluginContextException
        {
            /// <summary>
            /// Initializes a new instance of the PluginContextAlreadyRunningException class
            /// </summary>
            /// <param name="context">The PluginContext that is already running</param>
            internal PluginContextAlreadyRunningException(PluginContext context) :
                base(context, string.Format("A PluginContext is already running for the AppDomain '{0}'. Only one context can be run per application.", AppDomain.CurrentDomain.FriendlyName))
            {
            }
        }

        #endregion

        #region My Public Events

        /// <summary>
        /// Fires when a Plugin is started
        /// </summary>
        public event EventHandler<PluginDescriptorEventArgs> PluginStarted;

        /// <summary>
        /// Fires when a Plugin is stopped
        /// </summary>
        public event EventHandler<PluginDescriptorEventArgs> PluginStopped;

        /// <summary>
        /// Fires after all of the Plugins have been started
        /// </summary>
        public event EventHandler<PluginContextEventArgs> AfterPluginsStarted;

        /// <summary>
        /// Fires before all of the Plugins are stopped
        /// </summary>
        public event EventHandler<PluginContextEventArgs> BeforePluginsStopped;

        #endregion

        /// <summary>
        /// Initializes a new instance of the PluginContext class
        /// </summary>
        public PluginContext()
        {
            AssertThisIsTheOnlyCreatedContext();

            // kick off the gc timer, call back in 5 minutes
            _gcTimer = new Timer(OnGarbageCollectorTimerCallback, this, 50000, 50000);
        }

        #region My Static Methods

        /// <summary>
        /// Returns the current PluginContext that is hosting the current application's Plugins
        /// </summary>
        public static PluginContext Current
        {
            get { return _context; }
        }

        #endregion

        #region My Overrides

        /// <summary>
        /// Cleanup any managed resources
        /// </summary>
        protected override void DisposeOfManagedResources()
        {
            base.DisposeOfManagedResources();

            if (_pluginProviders != null)
            {
                _pluginProviders.Dispose();
                _pluginProviders = null;
            }

            if (_pluginDescriptors != null)
            {
                _pluginDescriptors.Dispose();
                _pluginDescriptors = null;
            }

            if (_gcTimer != null)
            {
                _gcTimer.Dispose();
                _gcTimer = null;
            }
        }

        #endregion

        #region My Public Properties

        /// <summary>
        /// Returns the Assembly that represents the starting executable
        /// </summary>
        public Assembly StartingAssembly
        {
            get { return Assembly.GetEntryAssembly(); }
        }

        /// <summary>
        /// Returns a collection of PluginProviders loaded in the PluginContext
        /// </summary>
        public PluginProviderCollection PluginProviders
        {
            get { return _pluginProviders; }
        }

        /// <summary>
        /// Returns a collection of PluginDescriptors that is loaded in this PluginContext
        /// </summary>
        public PluginDescriptorCollection PluginDescriptors
        {
            get { return _pluginDescriptors; }
        }

        #endregion

        #region My Public Methods

        /// <summary>
        /// Begins a new application plugin context. Should only be called once.
        /// </summary>
        public void Initialize(PluginProviderCollection pluginProviders)
        {
            // load the core sub-system providers
            _pluginProviders = pluginProviders;

            // use the plugin manager to load the plugin types that the plugin providers want loaded
            using (TypeCollection pluginTypes = PluginManager.LoadPluginTypes(_pluginProviders))
            {
                // use the plugin manager to create descriptors for all of the plugins
                _pluginDescriptors = PluginManager.CreatePluginDescriptors(pluginTypes);

                // validate the plugin dependencies
                PluginManager.ValidatePluginDependencies(_pluginDescriptors);

                // sort plugins to have the least dependent plugins first
                _pluginDescriptors = PluginManager.Sort(_pluginDescriptors, true);

                // create the plugins
                PluginManager.CreatePluginInstances(_pluginDescriptors);
            }
        }

        public void Start()
        {
            _pluginDescriptors = PluginManager.Sort(_pluginDescriptors, true);

            // start plugins
            PluginManager.StartPlugins(_pluginDescriptors);
        }

        public void Stop()
        {
            // sort plugins to have the most dependent plugins first
            _pluginDescriptors = PluginManager.Sort(_pluginDescriptors, false);

            // stop plugins
            PluginManager.StopPlugins(_pluginDescriptors);
        }

        #endregion

        /// <summary>
        /// Asserts that there are no other created PluginContexts in the current application
        /// </summary>
        private void AssertThisIsTheOnlyCreatedContext()
        {
            // there can be only one per application
            if (_context != null)
            {
                throw new PluginContextAlreadyExistsException(_context);
            }

            // the first thing is to set the context so that it can be retrieved
            // anywhere in the application from here on using the PluginContext.Current property
            _context = this;
        }

        /// <summary>
        /// Occurs when the GC timer kicks off to help force garbage collection.
        /// </summary>
        private void OnGarbageCollectorTimerCallback(object state)
        {
            GC.Collect();
        }

        /// <summary>
        /// Raises the PluginStarted event.
        /// </summary>
        /// <param name="e"></param>
        internal void OnPluginStarted(PluginDescriptorEventArgs e)
        {
            EventHandlerHelper.Raise(PluginStarted, this, e);
        }

        /// <summary>
        /// Raises the PluginStopped event.
        /// </summary>
        /// <param name="e"></param>
        internal void OnPluginStopped(PluginDescriptorEventArgs e)
        {
            EventHandlerHelper.Raise(PluginStopped, this, e);
        }

        /// <summary>
        /// Raises the AfterPluginsStarted event.
        /// </summary>
        /// <param name="e"></param>
        internal void OnAfterPluginsStarted(PluginContextEventArgs e)
        {
            EventHandlerHelper.Raise(AfterPluginsStarted, this, e);
        }

        /// <summary>
        /// Raises the BeforePluginsStopped event.
        /// </summary>
        /// <param name="e"></param>
        internal void OnBeforePluginsStopped(PluginContextEventArgs e)
        {
            EventHandlerHelper.Raise(BeforePluginsStopped, this, e);
        }
    }
}
