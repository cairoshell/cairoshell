using System;

namespace CairoDesktop.Extensibility.Attributes
{
    /// <summary>
    /// Defines a plugin attribute for specifying the names the authors for a plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginAuthorsAttribute : Attribute
    {
        private readonly string[] _authors;

        /// <summary>
        /// Initializes a new instance of the PluginAuthorsAttribute class.
        /// </summary>
        /// <param name="authors">An array of strings that specify the names of the plugin's authors.</param>
        public PluginAuthorsAttribute(params string[] authors)
        {
            _authors = authors;
        }

        /// <summary>
        /// Initializes a new instance of the PluginAuthorsAttribute class.
        /// </summary>
        /// <param name="author">The name of the plugin's author</param>
        public PluginAuthorsAttribute(string author)
        {
            _authors = new[] { author };
        }

        /// <summary>
        /// Returns an array of strings the specify the names of the plugin's authors.
        /// </summary>
        public string[] Authors
        {
            get { return _authors; }
        }
    }
}