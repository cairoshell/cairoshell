using System;

namespace CairoDesktop.Common.DesignPatterns
{
    /// <summary>
	/// Defines a strongly-typed collection of Provider instances.
	/// This class is thread-safe.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough()]
    public abstract class ProviderCollection : DisposableCollection
    {
        #region ProviderAlreadyExistsException

        /// <summary>
        /// Defines an exception that is generated when a Provider is added to the collection and another
        /// Provider already exists in the collection using the same name. The Providers should be uniquely
        /// named in the App.config file so that we can refer to them by name.
        /// </summary>
        public sealed class ProviderAlreadyExistsException : ApplicationException
        {
            private readonly string _name;

            /// <summary>
            /// Initializes a new instance of the ProviderAlreadyExistsException class
            /// </summary>
            /// <param name="name">The name of the Provider that already exists in the collection</param>
            internal ProviderAlreadyExistsException(string name) :
                base(string.Format("A Provider with the name '{0}' already exists in the collection.", name))
            {
                _name = name;
            }

            /// <summary>
            /// Returns the name of the Provider that already exists in the collection
            /// </summary>
            public string ProviderName
            {
                get
                {
                    return _name;
                }
            }
        }

        #endregion

        protected override void DisposeOfManagedResources()
        {
            base.DisposeOfManagedResources();

            lock (base.SyncRoot)
            {
                foreach (Provider provider in base.InnerList)
                    provider.Dispose();
                base.InnerList.Clear();
            }
        }

        /// <summary>
        /// Adds the Provider to the collection
        /// </summary>
        /// <param name="provider">The provider to add</param>
        protected void Add(Provider provider)
        {
            if (this.Contains(provider))
                throw new ProviderAlreadyExistsException(provider.Name);

            lock (base.SyncRoot)
            {
                base.InnerList.Add(provider);
            }
        }

        /// <summary>
        /// Removes the Provider from the collection
        /// </summary>
        /// <param name="provider"></param>
        protected void Remove(Provider provider)
        {
            if (this.Contains(provider))
                lock (base.SyncRoot)
                {
                    base.InnerList.Remove(provider);
                }
        }

        /// <summary>
        /// Determines if the collection contains another Provider with the same name
        /// </summary>
        /// <param name="provider">The Provider to look for</param>
        /// <returns></returns>
        protected bool Contains(Provider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            return (this[provider.Name] != null);
        }

        /// <summary>
        /// Returns the Provider from collection that has the specified name
        /// </summary>
        protected Provider this[string name]
        {
            get
            {
                lock (base.SyncRoot)
                {
                    foreach (Provider provider in base.InnerList)
                        if (string.Compare(provider.Name, name, true) == 0)
                            return provider;
                    return null;
                }
            }
        }
    }
}
