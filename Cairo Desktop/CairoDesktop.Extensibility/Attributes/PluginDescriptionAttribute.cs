using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying a description for a plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginDescriptionAttribute : Attribute
    {
        private readonly string _description;

        /// <summary>
        /// Initializes a new instance of the PluginDescriptionAttribute class.
        /// </summary>
        /// <param name="description">A description of the plugin.</param>
        public PluginDescriptionAttribute(string description)
        {
            _description = description;
        }

        /// <summary>
        /// Returns a description of the plugin.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }
    }

}