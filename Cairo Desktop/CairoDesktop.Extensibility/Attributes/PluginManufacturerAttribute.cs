using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying the name of a plugin's manufacturer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginManufacturerAttribute : Attribute
    {
        private readonly string _manufacturer;

        /// <summary>
        /// Initializes a new instance of the PluginManufacturerAttribute class.
        /// </summary>
        /// <param name="manufacturer">The name of the plugin's manufacturer.</param>
        public PluginManufacturerAttribute(string manufacturer)
        {
            _manufacturer = manufacturer;
        }

        /// <summary>
        /// Returns the name of the plugin's manufacturer.
        /// </summary>
        public string Manufacturer
        {
            get { return _manufacturer; }
        }
    }
}