using System;
using System.Collections.Generic;

namespace TurboMapper
{
    internal interface IObjectMap
    {
        void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings = null);

        void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings, bool enableDefaultMapping);

        void RegisterConverter<TSource, TDestination>(Func<TSource, TDestination> converter);
    }
}