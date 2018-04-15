using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying the version for a plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginVersionAttribute : Attribute
    {
        private readonly Version _version;

        /// <summary>
        /// Initializes a new instance of the PluginVersionAttribute class.
        /// </summary>
        /// <param name="version">The version of the plugin.</param>
        public PluginVersionAttribute(string version)
        {
            _version = new Version(version);
        }

        /// <summary>
        /// Returns the plugin's version.
        /// </summary>
        public Version Version
        {
            get { return _version; }
        }
    }
}