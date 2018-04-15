using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying a Plugin dependency for a plugin.
    /// May be defined multiple times for any class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PluginDependencyAttribute : Attribute
    {
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the PluginDependencyAttribute class.
        /// </summary>
        /// <param name="type">The Plugin Type that the class depends upon.</param>
        public PluginDependencyAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            _type = type;
        }

        /// <summary>
        /// Returns the Type of the other Plugin that the class depends upon.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }
    }

}