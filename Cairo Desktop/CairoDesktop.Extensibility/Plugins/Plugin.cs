using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Extensibility.ObjectModel;
using System;

namespace CairoDesktop.Extensibility.Plugins
{
    /// <summary>
    /// Defines the base class from which all Plugin classes must derive.
    /// </summary>
    public abstract class Plugin : DisposableObject
    {
        /// <summary>
        /// The abstract method that must be overriden by derived classes to start plugin functionality
        /// </summary>
        /// <param name="context">The PluginContext that is hosting this Plugin instance.</param>
        /// <param name="e">EventArgs that contain a PluginDescriptor with meta-data about the Plugin instance.</param>
        protected abstract void Start(PluginContext context, PluginDescriptorEventArgs e);

        /// <summary>
        /// The abstract method that must be overriden by derived classes to stop plugin functionality
        /// </summary>
        /// <param name="context">The PluginContext that is hosting this Plugin instance.</param>
        /// <param name="e">EventArgs that contain a PluginDescriptor with meta-data about the Plugin instance.</param>
        protected abstract void Stop(PluginContext context, PluginDescriptorEventArgs e);

        /// <summary>
        /// Calls the Start method of the Plugin class.
        /// </summary>
        /// <param name="context">The PluginContext that is hosting this Plugin instance.</param>
        /// <param name="e">EventArgs that contain a PluginDescriptor with meta-data about the Plugin instance.</param>
        internal void OnStart(PluginContext context, PluginDescriptorEventArgs e)
        {
            try
            {
                CairoLogger.Instance.Debug(string.Format("Starting Plugin, Plugin: '{0}'.", e.Descriptor.PluginName));

                // inform the plugin that it should start its services
                Start(context, e);

                _CairoShell.DoEvents(); // Since we are WPF we use this DoEvents instead of the Application.DoEvents from System.Windows.Forms

                // fire the PluginStarted event of the PluginContext
                context.OnPluginStarted(e);
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Debug(ex.Message, ex);
            }
        }

        /// <summary>
        /// Calls the Stop method of the Plugin class.
        /// </summary>
        /// <param name="context">The PluginContext that is hosting this Plugin instance.</param>
        /// <param name="e">EventArgs that contain a PluginDescriptor with meta-data about the Plugin instance.</param>
        internal void OnStop(PluginContext context, PluginDescriptorEventArgs e)
        {
            try
            {
                CairoLogger.Instance.Debug(string.Format("Stopping Plugin, Plugin: '{0}'.", e.Descriptor.PluginName));

                // inform the plugin that it should stop its services
                Stop(context, e);

                // fire the PluginStopped event of the PluginContext
                context.OnPluginStopped(e);
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Debug(ex.Message, ex);
            }
        }
    }
}