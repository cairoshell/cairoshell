namespace CairoDesktop.Common
{
    /// <summary>
    /// Defines an enumeration of the various actions that can be taken on an object.
    /// The actions may apply to the object directly, or to the object as it exists in a container.
    /// </summary>
    public enum ObjectActions
    {
        /// <summary>
        /// Specifies no action was taken on the object.
        /// </summary>
        None,

        /// <summary>
        /// Specifies one or more properties or subproperties of the object have changed.
        /// </summary>
        Changed,

        /// <summary>
        /// Specifies that the object was added to some container.
        /// </summary>
        Added,

        /// <summary>
        /// Specifies that the object was removed from some container.
        /// </summary>
        Removed
    }
}