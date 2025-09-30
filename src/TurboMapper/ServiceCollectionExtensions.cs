using System;
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
            // Register ObjectMapper as singleton
            services.AddSingleton<IMapper, Mapper>(serviceProvider =>
            {
                var mapper = new Mapper();

                // Get all loaded assemblies in the AppDomain
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Discover and register mapping modules from all assemblies
                foreach (var assembly in loadedAssemblies)
                {
                    // Skip assemblies that might not be accessible
                    if (!assembly.IsDynamic && !assembly.GlobalAssemblyCache)
                    {
                        try
                        {
                            var mappingModules = assembly.GetTypes()
                                .Where(t => t.IsClass && !t.IsAbstract && typeof(IMappingModule).IsAssignableFrom(t))
                                .Select(t => Activator.CreateInstance(t) as IMappingModule);

                            foreach (var module in mappingModules)
                            {
                                module.CreateMap(mapper);
                            }
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            // Skip assemblies that can't be loaded
                            continue;
                        }
                        catch (Exception)
                        {
                            // Skip assemblies that cause other errors
                            continue;
                        }
                    }
                }

                return mapper;
            });

            return services;
        }
    }
}