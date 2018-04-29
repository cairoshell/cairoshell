using CairoDesktop.Common;
using CairoDesktop.Common.Logging;
using CairoDesktop.Extensibility.Attributes;
using CairoDesktop.Extensibility.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CairoDesktop.Extensibility.Providers.Custom
{
    /// <summary>
    /// Provides a PluginProvider that loads plugins from the application's startup path.
    /// </summary>
    public sealed class FileSystemPluginProvider : PluginProvider
    {
        #region TypeLoader

        /// <summary>
        /// Provides a class that can load Types from assemblies in the application's startup path.
        /// </summary>
        internal sealed class TypeLoader : MarshalByRefObject
        {
            /// <summary>
            /// Searches for plugins in the application's startup path
            /// </summary>
            /// <returns>null if no plugins were found</returns>
            private TypeCollection InternalSearchForPluginTypes()
            {
                TypeCollection types = null;

                // starting in the startup path
                // var directoryInfo = new DirectoryInfo(Application.StartupPath);
                var directoryInfo = new DirectoryInfo(_CairoShell.StartupPath);

                // look for all the dlls
                FileInfo[] files = directoryInfo.GetFiles("*.dll");

                // see if we can find any plugins defined in each assembly
                foreach (FileInfo file in files.Where(f => f.IsManagedAssembly()))
                {
                    // try and load the assembly
                    Assembly assembly = LoadAssembly(file.FullName);
                    if (assembly != null)
                    {
                        // see if the assembly has any plugins defined in it
                        TypeCollection typesInAssembly = LoadPluginTypesFromAssembly(assembly);
                        if (typesInAssembly != null)
                        {
                            if (types == null)
                                types = new TypeCollection();

                            // add the types defined as plugins to the master list
                            types.AddRange(typesInAssembly);
                        }
                    }
                }

                return types;
            }

            /// <summary>
            /// Loads an Assembly from the specified filename
            /// </summary>
            /// <param name="filename">The name of the file to load as an assembly</param>
            /// <returns>null if the file is not a valid .NET assembly</returns>
            private Assembly LoadAssembly(string filename)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(filename);
                }
                catch (BadImageFormatException)
                {
                    /*
					 * HACK: Normally you would never eat an exception, however 
					 * unmanaged dlls may be included in our results which will
					 * throw this exception indicating that the dll is not a 
					 * .NET assembly, in which case it will not contain plugins
					 * and is probably just some external dll that our app 
					 * references. In this case just ignore the exception.
					 * */

                    /*
					 * SIDENOTE: I really wish there was a way to easily specify a different
					 * extension for the plugins, because then we wouldn't have to 
					 * scan for dlls, and potentially try and load dlls that aren't
					 * assemblies. Unfortunately it's a real pain to do so with build
					 * events, and the pdb files get jacked up when they are supposed
					 * to contain info for some dll that is no longer there. 
					 * */
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Debug(ex.Message, ex);
                }
                return assembly;
            }

            /// <summary>
            /// Loads a TypeCollection with the plugins defined in the assembly
            /// </summary>
            /// <param name="assembly">The assembly to check for plugin definitions</param>
            /// <returns></returns>
            private TypeCollection LoadPluginTypesFromAssembly(Assembly assembly)
            {
                TypeCollection types = null;

                try
                {
                    object[] value = assembly.GetCustomAttributes(typeof(PluginDefinitionAttribute), false);

                    if (value.Length > 0)
                    {
                        var attributes = (PluginDefinitionAttribute[])value;

                        foreach (PluginDefinitionAttribute attribute in attributes)
                        {
                            if (types == null)
                                types = new TypeCollection();

                            types.Add(attribute.Type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Debug(ex.Message, ex);
                }

                return types;
            }

            /// <summary>
            /// Searches for plugin types from assemblies in the application's startup path in a second AppDomain
            /// </summary>
            /// <returns></returns>
            internal static TypeCollection SearchForPluginTypes()
            {
                // create a new appdomain where we'll try and load the plugins
                AppDomain domain = AppDomain.CreateDomain(Guid.NewGuid().ToString());

                string typeName = typeof(TypeLoader).FullName ?? string.Empty;

                // create an instance of the plugin loader in the new appdomain
                var loader = (TypeLoader)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeName);

                // use the loader to search for plugins inside the second appdomain
                TypeCollection types = loader.InternalSearchForPluginTypes();

                // unload the appdomain
                AppDomain.Unload(domain);

                // return the plugin descriptors that were found
                return types;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the FileSystemPluginProvider class
        /// </summary>
        /// <param name="name">The name of the provider</param>
        public FileSystemPluginProvider(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Loads the plugin types this provider brings to the system.
        /// </summary>
        /// <returns></returns>
        public override TypeCollection LoadPluginTypes()
        {
            return TypeLoader.SearchForPluginTypes();
        }
    }


    public static class FileInfoExtensions
    {
        public static bool IsManagedAssembly(this FileInfo fileInfo)
        {
            string fileName = fileInfo.FullName;
            using (Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                if (fileStream.Length < 64)
                {
                    return false;
                }

                //PE Header starts @ 0x3C (60). Its a 4 byte header.
                fileStream.Position = 0x3C;
                uint peHeaderPointer = binaryReader.ReadUInt32();
                if (peHeaderPointer == 0)
                {
                    peHeaderPointer = 0x80;
                }

                // Ensure there is at least enough room for the following structures:
                //     24 byte PE Signature & Header
                //     28 byte Standard Fields         (24 bytes for PE32+)
                //     68 byte NT Fields               (88 bytes for PE32+)
                // >= 128 byte Data Dictionary Table
                if (peHeaderPointer > fileStream.Length - 256)
                {
                    return false;
                }

                // Check the PE signature.  Should equal 'PE\0\0'.
                fileStream.Position = peHeaderPointer;
                uint peHeaderSignature = binaryReader.ReadUInt32();
                if (peHeaderSignature != 0x00004550)
                {
                    return false;
                }

                // skip over the PEHeader fields
                fileStream.Position += 20;

                const ushort PE32 = 0x10b;
                const ushort PE32Plus = 0x20b;

                // Read PE magic number from Standard Fields to determine format.
                var peFormat = binaryReader.ReadUInt16();
                if (peFormat != PE32 && peFormat != PE32Plus)
                {
                    return false;
                }

                // Read the 15th Data Dictionary RVA field which contains the CLI header RVA.
                // When this is non-zero then the file contains CLI data otherwise not.
                ushort dataDictionaryStart = (ushort)(peHeaderPointer + (peFormat == PE32 ? 232 : 248));
                fileStream.Position = dataDictionaryStart;

                uint cliHeaderRva = binaryReader.ReadUInt32();
                if (cliHeaderRva == 0)
                {
                    return false;
                }

                return true;
            }
        }
    }
}