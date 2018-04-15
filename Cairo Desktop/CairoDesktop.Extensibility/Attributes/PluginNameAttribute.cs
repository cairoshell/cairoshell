using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying the name for a plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginNameAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the PluginNameAttribute class
        /// </summary>
        /// <param name="name">The name of the plugin.</param>
        public PluginNameAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns the name of the plugin.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
    }
}