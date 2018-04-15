using System;

namespace CairoDesktop.Extensibility.Plugins
{
    /// <summary>
    /// Defines an EventArgs class for the PluginDescriptorEventHandler delegate.
    /// </summary>
    public sealed class PluginDescriptorEventArgs : EventArgs
    {
        private readonly PluginDescriptor _descriptor;

        /// <summary>
        /// Initializes a new instance of the PluginDescriptorEventArgs class
        /// </summary>
        /// <param name="descriptor"></param>
        public PluginDescriptorEventArgs(PluginDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        /// <summary>
        /// Returns the PluginDescriptor around which this event is based
        /// </summary>
        public PluginDescriptor Descriptor
        {
            get { return _descriptor; }
        }
    }
}