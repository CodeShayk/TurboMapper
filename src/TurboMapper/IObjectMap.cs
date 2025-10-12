using System.Collections.Generic;

namespace TurboMapper
{
    /// <summary>
    /// Defines methods for configuring object mappings.
    /// </summary>
    internal interface IObjectMap
    {
        /// <summary>
        /// Creates a mapping configuration between TSource and TTarget types with optional property mappings.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="mappings"></param>
        void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings = null);
        /// <summary>
        /// Creates a mapping configuration between TSource and TTarget types with specified property mappings and an option to enable default mapping for unmapped properties.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="mappings"></param>
        /// <param name="enableDefaultMapping"></param>
        void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings, bool enableDefaultMapping);
        /// <summary>
        /// Registers a custom converter function to convert from TSource to TDestination types.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="converter"></param>
        void RegisterConverter<TSource, TDestination>(Func<TSource, TDestination> converter);
    }
}