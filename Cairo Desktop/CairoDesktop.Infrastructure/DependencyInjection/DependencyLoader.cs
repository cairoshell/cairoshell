using CairoDesktop.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CairoDesktop.Infrastructure.DependencyInjection
{
    public static class DependencyLoader
    {
        public static IServiceCollection LoadDependencies(this IServiceCollection serviceCollection, string path, string pattern = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return serviceCollection;
            }

            if (!Directory.Exists(path))
            {
                return serviceCollection;
            }

            var directoryCatalog = string.IsNullOrWhiteSpace(pattern)
                ? new DirectoryCatalog(path)
                : new DirectoryCatalog(path, pattern);

            var importDefinition = BuildImportDefinition();
            try
            {
                using (var aggregateCatalog = new AggregateCatalog())
                {
                    aggregateCatalog.Catalogs.Add(directoryCatalog);
                    using (var compositionContainer = new CompositionContainer(aggregateCatalog))
                    {
                        IEnumerable<Export> exports = compositionContainer.GetExports(importDefinition);
                        IEnumerable<IDependencyRegistrant> modules = exports.Select(export => export.Value as IDependencyRegistrant).Where(m => m != null);
                        var registerComponent = new DependencyRegistrar(serviceCollection);
                        foreach (IDependencyRegistrant module in modules)
                        {
                            module.Register(registerComponent);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                var builder = new StringBuilder();
                foreach (Exception loaderException in typeLoadException.LoaderExceptions)
                {
                    builder.AppendFormat("{0}\n", loaderException.Message);
                }

                throw new TypeLoadException(builder.ToString(), typeLoadException);
            }

            return serviceCollection;
        }

        private static ImportDefinition BuildImportDefinition()
        {
            return new ImportDefinition(
                exportDefinition => true, typeof(IDependencyRegistrant).FullName, ImportCardinality.ZeroOrMore, isRecomposable: false, isPrerequisite: false);
        }
    }
}