using System.Collections.Generic;

namespace TurboMapper
{
    /// <summary>
    /// Defines methods for mapping objects and validating mappings.
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        /// Maps an object of type TSource to an object of type TTarget.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        TTarget Map<TSource, TTarget>(TSource source);

        /// <summary>
        /// Maps a collection of objects of type TSource to a collection of objects of type TDestination.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source);

        /// <summary>
        /// Validates the mapping configuration between TSource and TTarget types.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        ValidationResult ValidateMapping<TSource, TTarget>();
    }
}