namespace CairoDesktop.Common.DesignPatterns
{
    /// <summary>
    /// Defines the base class from which all Provider classes should inherit.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough()]
    public abstract class Provider : DisposableObject
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the Provider class.
        /// </summary>
        /// <param name="name">The name of the Provider.</param>
        protected Provider(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets or sets the name of the Provider
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}