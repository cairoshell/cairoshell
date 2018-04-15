using CairoDesktop.Common.DesignPatterns;

namespace CairoDesktop.Extensibility.Providers
{
    /// <summary>
	/// Provides a strongly-typed collection of PluginProvider instances.
	/// This class is thread safe.
	/// </summary>
	public sealed class PluginProviderCollection : ProviderCollection
    {
        /// <summary>
        /// Initializes a new instance of the PluginProviderCollection class.
        /// </summary>
        public PluginProviderCollection()
        {
        }

        /// <summary>
        /// Returns the provider from the collection with the specified name.
        /// </summary>
        public new PluginProvider this[string name]
        {
            get { return (PluginProvider)base[name]; }
        }

        /// <summary>
        /// Adds a provider to the collection.
        /// </summary>
        /// <param name="provider">The provider to add.</param>
        public void Add(PluginProvider provider)
        {
            base.Add(provider);
        }

        /// <summary>
        /// Removes a provider from the collection.
        /// </summary>
        /// <param name="provider">The provider to remove.</param>
        public void Remove(PluginProvider provider)
        {
            base.Remove(provider);
        }

        /// <summary>
        /// Determines if the provider exists in the collection.
        /// </summary>
        /// <param name="provider">The provider to look for.</param>
        /// <returns></returns>
        public bool Contains(PluginProvider provider)
        {
            return base.Contains(provider);
        }
    }
}