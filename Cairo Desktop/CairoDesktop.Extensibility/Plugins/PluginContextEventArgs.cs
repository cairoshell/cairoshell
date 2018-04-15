using System;

namespace CairoDesktop.Extensibility.Plugins
{
    /// <summary>
    /// Defines an EventArgs class that contains a PluginContext as the context of the event.
    /// </summary>
    public sealed class PluginContextEventArgs : EventArgs
    {
        private readonly PluginContext _context;

        /// <summary>
        /// Initializes a new instance of the PluginContextEventArgs class.
        /// </summary>
        /// <param name="context">The plugin context that is the context of the event.</param>
        public PluginContextEventArgs(PluginContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the plugin context that is the context of the event.
        /// </summary>
        public PluginContext Context
        {
            get { return _context; }
        }
    }
}