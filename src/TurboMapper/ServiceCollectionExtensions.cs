using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TurboMapper.Impl;

namespace TurboMapper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTurboMapper(
            this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
                
            // Register ObjectMapper as singleton
            services.AddSingleton<IMapper, Mapper>(serviceProvider =>
            {
                var mapper = new Mapper();

                // Try to discover and register mapping modules using multiple strategies
                DiscoverAndRegisterMappingModules(mapper);

                return mapper;
            });

            return services;
        }

        private static void DiscoverAndRegisterMappingModules(Mapper mapper)
        {
            // Strategy 1: Get all currently loaded assemblies
            var loadedAssemblies = new HashSet<Assembly>();
            
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.IsDynamic && !assembly.GlobalAssemblyCache)
                    {
                        loadedAssemblies.Add(assembly);
                    }
                }
            }
            catch
            {
                // If we can't access the AppDomain assemblies, continue with empty set
            }

            // Strategy 2: Add important assemblies that might not be loaded yet
            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null && !loadedAssemblies.Contains(entryAssembly))
                {
                    loadedAssemblies.Add(entryAssembly);
                }
            }
            catch
            {
                // Continue if entry assembly can't be accessed
            }
            
            try
            {
                var callingAssembly = Assembly.GetCallingAssembly();
                if (callingAssembly != null && !loadedAssemblies.Contains(callingAssembly))
                {
                    loadedAssemblies.Add(callingAssembly);
                }
            }
            catch
            {
                // Continue if calling assembly can't be accessed
            }
                
            try
            {
                var executingAssembly = Assembly.GetExecutingAssembly();
                if (executingAssembly != null && !loadedAssemblies.Contains(executingAssembly))
                {
                    loadedAssemblies.Add(executingAssembly);
                }
            }
            catch
            {
                // Continue if executing assembly can't be accessed
            }

            // Process all discovered assemblies
            foreach (var assembly in loadedAssemblies)
            {
                try
                {
                    var mappingTypes = GetMappingTypesFromAssembly(assembly);
                    foreach (var type in mappingTypes)
                    {
                        try
                        {
                            var module = Activator.CreateInstance(type) as IMappingModule;
                            if (module != null)
                            {
                                module.CreateMap(mapper);
                            }
                        }
                        catch
                        {
                            // Skip modules that can't be instantiated
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip assemblies that cause errors during processing
                    continue;
                }
            }
        }
        
        private static List<Type> GetMappingTypesFromAssembly(Assembly assembly)
        {
            try
            {
                if (assembly.IsDynamic || assembly.GlobalAssemblyCache) 
                {
                    return new List<Type>();
                }
                
                return assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IMappingModule).IsAssignableFrom(t))
                    .ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // If types can't be loaded, try to work with the ones that loaded
                var types = new List<Type>();
                if (ex.Types != null)
                {
                    types = ex.Types.Where(t => t != null && !t.IsAbstract && typeof(IMappingModule).IsAssignableFrom(t)).ToList();
                }
                return types;
            }
            catch (Exception)
            {
                // For any other error, return an empty list
                return new List<Type>();
            }
        }
    }
}