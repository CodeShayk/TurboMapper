using System;
using System.Collections.Generic;

namespace TurboMapper
{
    public interface IMapper
    {
        TTarget Map<TSource, TTarget>(TSource source);
        IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source);
        bool ValidateMapping<TSource, TTarget>();
        string[] GetMappingErrors<TSource, TTarget>();
    }
}