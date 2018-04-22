using CairoDesktop.Common;
using CairoDesktop.Common.Helpers;
using CairoDesktop.Common.Logging;
using CairoDesktop.Extensibility.Providers;
using System;

namespace CairoDesktop.Extensibility.Plugins
{
    /// <summary>
    /// Provides methods for managing Plugins.
    /// </summary>
    //	[System.Diagnostics.DebuggerStepThrough()]
    internal static class PluginManager
    {
        #region My Public Static Methods

        /// <summary>
        /// Uses the PluginProviders specified to load Plugin Types that will be used to create Plugins.
        /// </summary>
        /// <param name="pluginProviders">The collection of PluginProviders that will be used to load the Plugin Types from their various sources.</param>
        /// <returns></returns>
        public static TypeCollection LoadPluginTypes(PluginProviderCollection pluginProviders)
        {
            var pluginTypes = new TypeCollection();
            foreach (PluginProvider provider in pluginProviders)
            {
                try
                {
                    CairoLogger.Instance.Debug(string.Format("Loading Plugin Types. PluginProvider: '{0}'.", provider.Name));

                    TypeCollection types = provider.LoadPluginTypes();
                    if (types != null)
                    {
                        pluginTypes.AddRange(types);
                    }
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Debug(ex.Message, ex);
                }
            }
            return pluginTypes;
        }

        /// <summary>
        /// Sorts the collection of PluginDescriptors according to their dependency chain.
        /// </summary>
        /// <param name="descriptorCollection">The collection of descriptors to sort.</param>
        /// <param name="leastDependentFirst">A flag that determines how the descriptors are sorted.</param>
        /// <returns></returns>
        public static PluginDescriptorCollection Sort(PluginDescriptorCollection descriptorCollection, bool leastDependentFirst)
        {
            CairoLogger.Instance.Debug(string.Format("Sorting PluginDescriptor Collection. LeastDependentFirst: '{0}'.", leastDependentFirst.ToString()));



            PluginDescriptor[] descriptors = descriptorCollection.ToArray();
            PluginDescriptor.Sort(descriptors, leastDependentFirst);

            descriptorCollection.Clear();
            descriptorCollection.Add(descriptors);

            return descriptorCollection;
        }

        /// <summary>
        /// Creates PluginDescriptors from each Plugin Type specified.
        /// </summary>
        /// <param name="types">The collection of Plugin Types to create descriptors for.</param>
        /// <returns></returns>
        public static PluginDescriptorCollection CreatePluginDescriptors(TypeCollection types)
        {
            var descriptors = new PluginDescriptorCollection();
            foreach (Type type in types)
            {
                try
                {
                    CairoLogger.Instance.Debug(string.Format("Creating PluginDescriptor, Type: '{0}'.", type.FullName));

                    var descriptor = new PluginDescriptor(type);

                    descriptors.Add(descriptor);
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Debug(ex.Message, ex);
                }
            }
            return descriptors;
        }

        /// <summary>
        /// Validates the dependencies for each of the PluginDescriptors
        /// </summary>
        /// <param name="descriptors">The collection of PluginDescriptors that describe the Plugins to be loaded.</param>
        public static void ValidatePluginDependencies(PluginDescriptorCollection descriptors)
        {
            /*
			 * Validation Phases
			 * Phase 1: Direct		(First level dependencies)
			 * Phase 2: Indirect	(Second level dependencies. i.e., dependencies of dependencies. Requires that Phase 1 already executed)
			 * Phase 3: Extended	(Provider Validation)
			 * */

            // Phase 1: Checks descriptors for missing dependencies and circular references. (direct)
            foreach (PluginDescriptor descriptor in descriptors)
            {
                try
                {
                    // check for missing dependencies
                    //					MarkDescriptorIfMissingDependency(descriptor, descriptors);
                    if (!descriptor.IsMissingDependency)
                    {
                        // check for circular references between plugins (direct, does not check dependency chains)
                        MarkDescriptorIfCircularlyDependent(descriptor, descriptors);
                    }
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Debug(ex.Message, ex);
                }
            }

            // Phase 2: Checks depencencies for missing dependencies and circular references. (indirect)
            foreach (PluginDescriptor descriptor in descriptors)
            {
                try
                {
                    // 
                    if (!descriptor.IsMissingDependency && !descriptor.IsCircularlyDependent)
                    {
                        MarkDescriptorIfDependenciesAreMissingDependencyOrAreCircularlyDependent(descriptor, descriptors);
                    }
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Debug(ex.Message, ex);
                }
            }

            // Phase 3: Allow for Provider based validation?	

            /*
			 * Here we have an extension point. 
			 * If we created another provider who's sole purpose was to validate a PluginDescriptor,
			 * we could move this logic away from the core, and allow for validation to be extended.
			 * Possible reasons for doing this would be to prevent Plugins from being loaded based 
			 * on some other criteria. We could provide descriptions of why a particular descriptor failed validation.
			 * */
        }

        /// <summary>
        /// Creates instances of the Plugin type defined by each PluginDescriptor.
        /// </summary>
        /// <param name="descriptors">The collection of PluginDescriptors that describe the Plugins to be loaded.</param>
        public static void CreatePluginInstances(PluginDescriptorCollection descriptors)
        {
            CairoLogger.Instance.Debug(string.Format("Creating Plugins. # of Plugins: '{0}'.", descriptors.Count.ToString()));

            foreach (PluginDescriptor descriptor in descriptors)
            {
                if (descriptor.IsStartable)
                {
                    if (AreDependenciesCreated(descriptor, descriptors))
                    {
                        CreatePluginInstance(descriptor);
                    }
                }
            }
        }

        /// <summary>
        /// Starts the plugins defined in the collection that have been created.
        /// </summary>
        /// <param name="descriptors">The collection of PluginDescriptors that describe the Plugins to be loaded.</param>
        public static void StartPlugins(PluginDescriptorCollection descriptors)
        {
            CairoLogger.Instance.Debug(string.Format("Starting Plugins. # of Plugins: '{0}'.", descriptors.Count.ToString()));

            // start all of the plugins
            foreach (PluginDescriptor descriptor in descriptors)
            {
                if (descriptor.PluginInstance != null)
                {
                    StartPlugin(descriptor);
                }
                else
                {
                    CairoLogger.Instance.Debug(string.Format("Skipped Plugin: '{0}' was not created.", descriptor.PluginName));
                }
            }

            // fire the AfterPluginsStarted event of the PluginContext 
            PluginContext.Current.OnAfterPluginsStarted(new PluginContextEventArgs(PluginContext.Current));
        }

        /// <summary>
        /// Stops the plugins defined in the collection that have been created.
        /// </summary>
        /// <param name="descriptors">The collection of PluginDescriptors that describe the Plugins to be loaded.</param>
        public static void StopPlugins(PluginDescriptorCollection descriptors)
        {
            CairoLogger.Instance.Debug(string.Format("Stopping Plugins. # of Plugins: '{0}'.", descriptors.Count.ToString()));

            // fire the BeforePluginsStopped event of the PluginContext
            PluginContext.Current.OnBeforePluginsStopped(new PluginContextEventArgs(PluginContext.Current));

            // stop all of the plugins
            foreach (PluginDescriptor descriptor in descriptors)
            {
                if (descriptor.PluginInstance != null)
                {
                    StopPlugin(descriptor);
                }
                else
                {
                    CairoLogger.Instance.Debug(string.Format("Skipped Plugin: '{0}' was not created.", descriptor.PluginName));
                }
            }
        }

        #endregion

        #region My Private Static Methods

        /// <summary>
        /// Marks a descriptor if is is circularly dependent with any other descriptor
        /// </summary>
        /// <param name="descriptor">The descriptor to check</param>
        /// <param name="descriptors">The collection of PluginDescriptors to check against</param>
        private static void MarkDescriptorIfCircularlyDependent(PluginDescriptor descriptor, PluginDescriptorCollection descriptors)
        {
            // check each dependency in that descriptor depends on
            foreach (Type type in descriptor.PluginDependencies)
            {
                // against all the other descriptors
                foreach (PluginDescriptor otherDescriptor in descriptors)
                {
                    // when we find a descriptor that describes the Type the first descriptor needs
                    if (otherDescriptor.PluginType == type)
                    {
                        // it better not depend on the first
                        if (otherDescriptor.DependsOn(descriptor))
                        {
                            // if it does, it's a circular dependency which we cannot have
                            descriptor.IsCircularlyDependent = true;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Marks a descriptor if it has dependencies that themselves are missing dependencies or are circularly dependent
        /// </summary>
        /// <param name="descriptor">The descriptor to check</param>
        /// <param name="descriptors">The collection of PluginDescriptors to check against</param>
        private static void MarkDescriptorIfDependenciesAreMissingDependencyOrAreCircularlyDependent(PluginDescriptor descriptor, PluginDescriptorCollection descriptors)
        {
            // check each dependency in that descriptor depends on
            foreach (Type type in descriptor.PluginDependencies)
            {
                // against all the other descriptors
                foreach (PluginDescriptor otherDescriptor in descriptors)
                {
                    // when we find a descriptor that describes the Type the first descriptor needs
                    if (otherDescriptor.PluginType == type)
                    {
                        // the other dependency better not be missing a dependency
                        if (otherDescriptor.IsMissingDependency)
                        {
                            // if it does, the whole chain is jacked 
                            descriptor.IsDependentOnTypeThatIsMissingDependency = true;
                            return;
                        }

                        // the other dependency better not be circularly dependent
                        if (otherDescriptor.IsCircularlyDependent)
                        {
                            // if it does, the whole chain is jacked 
                            descriptor.IsDependentOnTypeThatIsCircularlyDependent = true;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the dependencies for a PluginDescriptor are created.
        /// </summary>
        /// <param name="descriptor">The descriptor to check dependencies for.</param>
        /// <param name="descriptors">The collection of PluginDescriptor(s) to check against.</param>
        /// <returns></returns>
        private static bool AreDependenciesCreated(PluginDescriptor descriptor, PluginDescriptorCollection descriptors)
        {
            foreach (Type type in descriptor.PluginDependencies)
                if (descriptors[type].PluginInstance == null)
                    return false;
            return true;
        }

        /// <summary>
        /// Creates an instance of the Type described by the PluginDescriptor and asserts that it derives from Plugin.
        /// </summary>
        /// <param name="descriptor">The PluginDescriptor that contains the Type to create.</param>
        private static void CreatePluginInstance(PluginDescriptor descriptor)
        {
            try
            {
                TypeHelper.AssertTypeIsSubclassOfBaseType(descriptor.PluginType, typeof(Plugin));

                CairoLogger.Instance.Debug(string.Format("Creating Plugin: '{0}'.", descriptor.PluginName));

                var plugin = (Plugin)TypeHelper.CreateInstanceOfType(descriptor.PluginType, Type.EmptyTypes, new object[] { });

                descriptor.AttachPluginInstance(plugin);
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Debug(ex.Message, ex);
            }
        }

        /// <summary>
        /// Starts the specified plugin.
        /// </summary>
        /// <param name="descriptor">The descriptor that contains the plugin to start.</param>
        private static void StartPlugin(PluginDescriptor descriptor)
        {
            // start the plugin
            descriptor.PluginInstance.OnStart(PluginContext.Current, new PluginDescriptorEventArgs(descriptor));
        }

        /// <summary>
        /// Stops the specified plugin.
        /// </summary>
        /// <param name="descriptor">The descriptor that contains the plugin to stop.</param>
        private static void StopPlugin(PluginDescriptor descriptor)
        {
            // stop the plugin
            descriptor.PluginInstance.OnStop(PluginContext.Current, new PluginDescriptorEventArgs(descriptor));
        }

        #endregion
    }
}