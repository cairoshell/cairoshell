using CairoDesktop.Common;
using CairoDesktop.Common.DesignPatterns;

namespace CairoDesktop.Extensibility.Providers
{
    /// <summary>
    /// Defines a class responsible for loading Plug-ins.
    /// </summary>
    public abstract class PluginProvider : Provider
    {
        /// <summary>
        /// Initializes a new instance of the PluginProvider class.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        protected PluginProvider(string name) :
            base(name)
        {
        }

        /// <summary>
        /// Returns a collection of Types that are defined as Plug-ins.
        /// </summary>
        /// <returns></returns>
        public abstract TypeCollection LoadPluginTypes();
    }
}
