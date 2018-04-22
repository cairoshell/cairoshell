using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Extensibility.Attributes;
using System;
using System.IO;
using System.Linq;

namespace CairoDesktop.Extensibility.Plugins
{
    /// <summary>
    /// Provides a class that describes a single Plugin instance using meta-data and contains the active instance of the Plugin type.
    /// </summary>
    public sealed class PluginDescriptor : DisposableObject
    {
        private readonly Type[] _dependencies;
        private readonly Type _type;
        private bool _isCircularlyDependent;
        private bool _isDependentOnTypeThatIsCircularlyDependent;
        private bool _isDependentOnTypeThatIsMissingDependency;
        private bool _isMissingDependency;
        private Plugin _plugin;

        /// <summary>
        /// Initializes a new instance of the PluginDescriptor class
        /// </summary>
        /// <param name="type"></param>
        public PluginDescriptor(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            _type = type;
            _dependencies = ExtractTypesThatThisTypeDependsOn();
        }

        #region My Private Methods

        /// <summary>
        /// Extracts the Types that the specified as dependencies via attributes on the specified Type.
        /// </summary>
        /// <returns></returns>
        private Type[] ExtractTypesThatThisTypeDependsOn()
        {
            try
            {
                object[] attributes = _type.GetCustomAttributes(typeof(PluginDependencyAttribute), true);
                if (attributes.Length > 0)
                {
                    var dependencies = new Type[attributes.Length];
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        try
                        {
                            var attribute = (PluginDependencyAttribute)attributes[i];
                            dependencies[i] = attribute.Type;
                        }
                        catch (Exception ex)
                        {
                            CairoLogger.Instance.Debug(ex.Message, ex);
                        }
                    }
                    return dependencies;
                }
            }
            catch (FileNotFoundException ex)
            {
                // if this fails, we know that there is a type listed in this assembly's
                // attributes, who's containing assembly couldn't be found, so this is therefore
                // missing a dependency. 
                IsMissingDependency = true;

                CairoLogger.Instance.Debug(string.Format("Plugin: '{0}' is missing a dependency. Additional Info: '{1}'", PluginName, ex.Message), ex);

            }
            return new Type[] { };
        }

        #endregion

        #region My Public Properties

        /// <summary>
        /// Returns the plugin's Type
        /// </summary>
        public Type PluginType
        {
            get { return _type; }
        }

        /// <summary>
        /// Returns the object instance implementing the IPlugin interface created by the Type described by this PluginDescriptor
        /// </summary>
        public Plugin PluginInstance
        {
            get { return _plugin; }
        }

        /// <summary>
        /// Returns an array of strings that contain the names of the plugin's authors
        /// </summary>
        public string[] PluginAuthors
        {
            get
            {
                object[] value = _type.GetCustomAttributes(typeof(PluginAuthorsAttribute), false);
                return value.Length > 0
                    ? ((PluginAuthorsAttribute)value[0]).Authors
                    : new[] { string.Empty };
            }
        }

        /// <summary>
        /// Returns an array of Types that define external Plugin Types that this Plugin Type depends upon
        /// </summary>
        public Type[] PluginDependencies
        {
            get { return _dependencies; }
        }

        /// <summary>
        /// Returns the plugin's description
        /// </summary>
        public string PluginDescription
        {
            get
            {
                object[] value = _type.GetCustomAttributes(typeof(PluginDescriptionAttribute), false);
                return value.Length > 0
                    ? ((PluginDescriptionAttribute)value[0]).Description
                    : string.Empty;
            }
        }

        /// <summary>
        /// Returns the plugin's Id
        /// </summary>
        public string PluginId
        {
            get
            {
                object[] value = _type.GetCustomAttributes(typeof(PluginIdAttribute), false);
                return value.Length > 0
                    ? ((PluginIdAttribute)value[0]).Id
                    : string.Empty;
            }
        }
        
        /// <summary>
        /// Returns the plugin's manufacturer
        /// </summary>
        public string PluginManufacturer
        {
            get
            {
                object[] value = _type.GetCustomAttributes(typeof(PluginManufacturerAttribute), false);
                return value.Length > 0 ? ((PluginManufacturerAttribute)value[0]).Manufacturer : string.Empty;
            }
        }

        /// <summary>
        /// Returns the plugin's name
        /// </summary>
        public string PluginName
        {
            get
            {
                object[] value = _type.GetCustomAttributes(typeof(PluginNameAttribute), false);
                return value.Length > 0 ? ((PluginNameAttribute)value[0]).Name : _type.FullName;
            }
        }

        /// <summary>
        /// Returns the plugin's version
        /// </summary>
        public Version PluginVersion
        {
            get
            {
                object[] value = _type.GetCustomAttributes(typeof(PluginVersionAttribute), false);
                return value.Length > 0 ? ((PluginVersionAttribute)value[0]).Version : new Version("0.0.0");
            }
        }

        public override string ToString()
        {
            return PluginName;
        }

        #endregion

        #region My Internal Methods

        /// <summary>
        /// Attaches a live instance of a class implementing the IPlugin interface to this PluginDescriptor object.
        /// </summary>
        /// <param name="plugin">The IPlugin interface that is described by this descriptor</param>
        internal void AttachPluginInstance(Plugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Determines if this Plugin depends upon the PluginType described by the specified PluginDescriptor.
        /// </summary>
        /// <param name="descriptor">The PluginDescriptor to check against this PluginDescriptor's dependencies</param>
        /// <returns>true if this descriptor depends on the specified descriptor's Type</returns>
        internal bool DependsOn(PluginDescriptor descriptor)
        {
            try
            {
                if (_dependencies.Contains(descriptor.PluginType))
                    return true;
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Debug(ex.Message, ex);
            }

            return false;
        }

        #endregion

        #region My Internal Properties

        /// <summary>
        /// Gets or sets a flag that indicates if the Plugin is missing a Plugin dependency.
        /// </summary>
        internal bool IsMissingDependency
        {
            get { return _isMissingDependency; }
            set { _isMissingDependency = value; }
        }

        /// <summary>
        /// Gets or sets a flag that indicates if the Plugin is circularly dependent with another Plugin
        /// </summary>
        internal bool IsCircularlyDependent
        {
            get { return _isCircularlyDependent; }
            set { _isCircularlyDependent = value; }
        }

        /// <summary>
        /// Gets or sets a flag that indicates if the Plugin is dependent on a Plugin that is circularly dependent
        /// </summary>
        internal bool IsDependentOnTypeThatIsCircularlyDependent
        {
            get { return _isDependentOnTypeThatIsCircularlyDependent; }
            set { _isDependentOnTypeThatIsCircularlyDependent = value; }
        }

        /// <summary>
        /// Gets or sets a flag that indicates if the Plugin is dependent on a Plugin that is missing a dependency
        /// </summary>
        internal bool IsDependentOnTypeThatIsMissingDependency
        {
            get { return _isDependentOnTypeThatIsMissingDependency; }
            set { _isDependentOnTypeThatIsMissingDependency = value; }
        }

        /// <summary>
        /// Returns a flag that indicates if the Plugin looks Ok to start.
        /// This flag is calculated by checking the Plugin's dependencies, and determining if it should be Ok to start,
        /// or whether it is impossible to start it because of a dependency problem.
        /// </summary>
        internal bool IsStartable
        {
            get { return !_isMissingDependency && !_isCircularlyDependent && !_isDependentOnTypeThatIsCircularlyDependent && !_isDependentOnTypeThatIsMissingDependency; }
        }

        #endregion

        /// <summary>
        /// Sorts an array of PluginDescriptors from least dependent first to most dependent last, or vice versa.
        /// </summary>
        /// <param name="descriptors">The array of descriptors to sort</param>
        /// <param name="leastDependentFirst">The order in which to sort them</param>
        /// <returns></returns>
        public static bool Sort(PluginDescriptor[] descriptors, bool leastDependentFirst)
        {
            try
            {
                for (int i = 0; i < descriptors.Length - 1; i++)
                {
                    for (int j = i + 1; j < descriptors.Length; j++)
                    {
                        bool dependsOn = descriptors[i].DependsOn(descriptors[j]);
                        if ((leastDependentFirst ? dependsOn : !dependsOn))
                        {
                            // swap i with j, where i=1 and j=2
                            PluginDescriptor descriptor = descriptors[j];
                            descriptors[j] = descriptors[i];
                            descriptors[i] = descriptor;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Debug(ex.Message, ex);
            }
            return false;
        }
    }
}