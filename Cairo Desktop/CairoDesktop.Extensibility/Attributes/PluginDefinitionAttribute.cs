using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying the type of plugin exported from an assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class PluginDefinitionAttribute : Attribute
    {
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the PluginDefinitionAttribute class.
        /// </summary>
        /// <param name="type">The Type that defines the plugin class exported from an assembly.</param>
        public PluginDefinitionAttribute(Type type)
        {
            _type = type;
        }

        /// <summary>
        /// Returns the Type that defines the plugin class exported from an assembly.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }
    }
}
