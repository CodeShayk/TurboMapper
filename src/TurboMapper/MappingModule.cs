using System;
using System.Collections.Generic;
using System.Linq;

namespace TurboMapper
{
    public abstract class MappingModule<TSource, TTarget> : IMappingModule
    {
        private readonly Action<MappingExpression<TSource, TTarget>> _configAction;
        private readonly bool _enableDefaultMapping;
        private readonly Dictionary<string, Delegate> _converters;

        public MappingModule(bool enableDefaultMapping = true)
        {
            _configAction = CreateMappings();
            _enableDefaultMapping = enableDefaultMapping;
            _converters = new Dictionary<string, Delegate>();
        }

        void IMappingModule.CreateMap(IObjectMap mapper)
        {
            var expression = new MappingExpression<TSource, TTarget>();
            _configAction(expression);

            // Add default mappings for unmapped properties if enabled
            if (_enableDefaultMapping)
            {
                var sourceProps = typeof(TSource).GetProperties();
                var targetProps = typeof(TTarget).GetProperties();

                foreach (var sourceProp in sourceProps)
                {
                    // Check if this property is already mapped
                    var isMapped = expression.Mappings.Exists(m =>
                        m.SourcePropertyPath.Split('.').Last() == sourceProp.Name);

                    if (!isMapped)
                    {
                        var targetProp = targetProps.FirstOrDefault(p =>
                            p.Name == sourceProp.Name &&
                            p.CanWrite);

                        if (targetProp != null)
                            // Add default mapping for unmapped properties
                            expression.Mappings.Add(new PropertyMapping
                            {
                                SourceProperty = sourceProp.Name,
                                TargetProperty = targetProp.Name,
                                SourcePropertyPath = sourceProp.Name,
                                TargetPropertyPath = targetProp.Name
                            });
                    }
                }
            }

            mapper.CreateMap<TSource, TTarget>(expression.Mappings, _enableDefaultMapping);

            // Register converters with the mapper if any were defined in this module
            RegisterConverters(mapper);
        }

        public abstract Action<IMappingExpression<TSource, TTarget>> CreateMappings();

        /// <summary>
        /// Registers a custom converter for type mappings within this module
        /// </summary>
        /// <typeparam name="TSourceConverter">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="converter">Function to perform the conversion</param>
        protected void RegisterConverter<TSourceConverter, TDestination>(Func<TSourceConverter, TDestination> converter)
        {
            var key = $"{typeof(TSourceConverter).FullName}_{typeof(TDestination).FullName}";
            _converters[key] = converter;
        }

        private void RegisterConverters(IObjectMap mapper)
        {
            foreach (var kvp in _converters)
            {
                var converter = kvp.Value as Delegate;
                if (converter != null)
                {
                    var sourceType = converter.Method.GetParameters()[0].ParameterType;
                    var destType = converter.Method.ReturnType;

                    // Find and invoke the RegisterConverter method with proper type parameters
                    var method = mapper.GetType().GetMethod("RegisterConverter",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null,
                        new Type[] { converter.GetType() },
                        null);

                    if (method == null)
                    {
                        // Try to get the generic method and make it specific
                        var genericMethod = mapper.GetType().GetMethod("RegisterConverter",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (genericMethod != null && genericMethod.IsGenericMethod)
                        {
                            var specificMethod = genericMethod.MakeGenericMethod(sourceType, destType);
                            specificMethod.Invoke(mapper, new object[] { converter });
                        }
                    }
                    else
                    {
                        method.Invoke(mapper, new object[] { converter });
                    }
                }
            }
        }
    }
}