using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
	/// Defines a plugin attribute for specifying a unique id for a plugin.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginIdAttribute : Attribute
    {
        private readonly string _id;

        /// <summary>
        /// Initializes a new instance of the PluginIdAttribute class.
        /// </summary>
        /// <param name="id">The unique id to assign to the plugin.</param>
        public PluginIdAttribute(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Returns a unique id for the plugin.
        /// </summary>
        public string Id
        {
            get { return _id; }
        }
    }
}