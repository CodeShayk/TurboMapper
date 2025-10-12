using System;
using System.Collections.Generic;
using System.Linq;

namespace TurboMapper
{
    /// <summary>
    /// Base class for defining mapping modules between source and target types.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public abstract class MappingModule<TSource, TTarget> : IMappingModule
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Action<MappingExpression<TSource, TTarget>> _configAction;
        /// <summary>
        /// Indicates whether default property mappings should be enabled for unmapped properties.
        /// </summary>
        private readonly bool _enableDefaultMapping;
        /// <summary>
        /// Holds custom converters registered within this module.
        /// </summary>
        private readonly Dictionary<string, Delegate> _converters;

        /// <summary>
        /// Initializes a new instance of the MappingModule class.
        /// </summary>
        /// <param name="enableDefaultMapping"></param>
        public MappingModule(bool enableDefaultMapping = true)
        {
            _configAction = CreateMappings();
            _enableDefaultMapping = enableDefaultMapping;
            _converters = new Dictionary<string, Delegate>();
        }
        /// <summary>
        /// Creates the mapping configuration between TSource and TTarget types.
        /// </summary>
        /// <param name="mapper"></param>
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
        }
        /// <summary>
        /// When implemented in a derived class, this method should return an action that configures the property mappings between TSource and TTarget types.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Registers all custom converters defined in this module with the provided IObjectMap instance.
        /// </summary>
        /// <param name="mapper"></param>
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