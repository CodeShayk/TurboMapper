using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TurboMapper.Tests")]

namespace TurboMapper.Impl
{
    /// <summary>
    /// Holds the mapping configuration between a source and target type, including custom property mappings and default mapping settings.
    /// </summary>
    internal class MapperConfiguration
    {
        /// <summary>
        /// A list of custom property mappings defined for this configuration.
        /// </summary>
        public List<PropertyMapping> Mappings { get; set; }
        /// <summary>
        /// Indicates whether default name-based mapping is enabled for properties not explicitly mapped.
        /// </summary>
        public bool EnableDefaultMapping { get; set; }
        /// <summary>
        /// Initializes a new instance of the MapperConfiguration class with specified property mappings and default mapping setting.
        /// </summary>
        /// <param name="mappings"></param>
        /// <param name="enableDefaultMapping"></param>
        public MapperConfiguration(List<PropertyMapping> mappings, bool enableDefaultMapping)
        {
            Mappings = mappings ?? new List<PropertyMapping>();
            EnableDefaultMapping = enableDefaultMapping;
        }
    }
    /// <summary>
    /// Implements the object mapping functionality, allowing for configuration of mappings between source and target types, including support for custom property mappings, nested properties, conditional mapping, and transformation functions.
    /// </summary>
    internal class Mapper : IMapper, IObjectMap
    {
        /// <summary>
        /// Holds mapping configurations between source and target types.
        /// </summary>
        private readonly Dictionary<Type, Dictionary<Type, MapperConfiguration>> _configurations;
        /// <summary>
        /// Caches property info arrays for types to optimize reflection performance.
        /// </summary>
        private readonly Dictionary<Type, System.Reflection.PropertyInfo[]> _propertyCache;
        /// <summary>
        /// Caches property info for nested property paths to optimize reflection performance.
        /// </summary>
        private readonly Dictionary<string, System.Reflection.PropertyInfo> _propertyPathCache;
        /// <summary>
        /// Caches factory functions for creating instances of types to optimize object instantiation performance.
        /// </summary>
        private readonly Dictionary<Type, Func<object>> _factoryCache;
        /// <summary>
        /// Caches getter functions for properties to optimize property access performance.
        /// </summary>
        private readonly Dictionary<string, Func<object, object>> _getterCache;
        /// <summary>
        /// Caches setter functions for properties to optimize property assignment performance.
        /// </summary>
        private readonly Dictionary<string, Action<object, object>> _setterCache;
        /// <summary>
        /// Initializes a new instance of the Mapper class.
        /// </summary>
        public Mapper()
        {
            _configurations = new Dictionary<Type, Dictionary<Type, MapperConfiguration>>();
        }
        /// <summary>
        /// Creates a mapping configuration between the specified source and target types, with optional custom property mappings.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="mappings"></param>
        public void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings = null)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_configurations.ContainsKey(sourceType))
                _configurations[sourceType] = new Dictionary<Type, MapperConfiguration>();

            _configurations[sourceType][targetType] = new MapperConfiguration(mappings, true);
        }
        /// <summary>
        /// Creates a mapping configuration between the specified source and target types, with specified custom property mappings and an option to enable or disable default name-based mapping for unmapped properties.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="mappings"></param>
        /// <param name="enableDefaultMapping"></param>
        public void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings, bool enableDefaultMapping)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_configurations.ContainsKey(sourceType))
                _configurations[sourceType] = new Dictionary<Type, MapperConfiguration>();

            _configurations[sourceType][targetType] = new MapperConfiguration(mappings, enableDefaultMapping);
        }
        /// <summary>
        /// Maps an instance of the source type to an instance of the target type, applying any configured property mappings, including support for nested properties, conditional mapping, and transformation functions.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TTarget Map<TSource, TTarget>(TSource source)
        {
            if (source == null)
                return default;

            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var target = Activator.CreateInstance<TTarget>();

            // Check for custom mapping configuration
            if (_configurations.ContainsKey(sourceType) &&
                _configurations[sourceType].ContainsKey(targetType))
            {
                var config = _configurations[sourceType][targetType];
                if (config.EnableDefaultMapping)
                    ApplyCustomMappings(source, target, config.Mappings);
                else
                    ApplyCustomMappingsWithDefaultDisabled(source, target, config.Mappings);
            }
            else
                // Default name-based mapping
                ApplyNameBasedMapping(source, target);

            return target;
        }
        /// <summary>
        /// Tries to retrieve the mapping configuration for the specified source and target types.
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private bool TryGetConfiguration(Type sourceType, Type targetType, out MapperConfiguration config)
        {
            config = null;

            if (_configurations.TryGetValue(sourceType, out var targetConfigs) &&
                targetConfigs.TryGetValue(targetType, out config))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Applies custom property mappings from the source object to the target object, including support for nested properties, conditional mapping, and transformation functions. Also applies default name-based mapping for unmapped properties if enabled in the configuration.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="mappings"></param>
        internal void ApplyCustomMappings<TSource, TTarget>(
            TSource source,
            TTarget target,
            List<PropertyMapping> mappings)
        {
            // First, apply all custom mappings
            foreach (var mapping in mappings)
            {
                var sourceValue = GetNestedValue(source, mapping.SourcePropertyPath);
                SetNestedValue(target, mapping.TargetPropertyPath, sourceValue);
            }

            // Then apply default name-based mapping for unmapped properties
            ApplyDefaultNameBasedMapping(source, target, mappings);
        }
        /// <summary>
        /// Applies custom property mappings from the source object to the target object, including support for nested properties, conditional mapping, and transformation functions. Does not apply default name-based mapping for unmapped properties.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="mappings"></param>
        internal void ApplyCustomMappingsWithDefaultDisabled<TSource, TTarget>(
            TSource source,
            TTarget target,
            List<PropertyMapping> mappings)
        {
            // Apply only custom mappings, no default mappings
            foreach (var mapping in mappings)
            {
                var sourceValue = GetNestedValue(source, mapping.SourcePropertyPath);
                SetNestedValue(target, mapping.TargetPropertyPath, sourceValue);
            }
        }
        /// <summary>
        /// Registers a custom converter function to convert from TSource to TDestination types.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="converter"></param>
        public void RegisterConverter<TSource, TDestination>(Func<TSource, TDestination> converter)
        {
            var key = $"{typeof(TSource).FullName}_{typeof(TDestination).FullName}";
            _converters[key] = converter;
        }
        /// <summary>
        /// Applies default name-based mapping for properties that have not been explicitly mapped in custom mappings.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="customMappings"></param>
        private void ApplyDefaultNameBasedMapping<TSource, TTarget>(
            TSource source,
            TTarget target,
            List<PropertyMapping> customMappings)
        {
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                // For default mapping, check if this source property maps to any target property in custom mappings
                // by checking if there's a custom mapping that targets a property with the same name as sourceProp.Name
                var targetProp = targetProps.FirstOrDefault(p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite);

                if (targetProp != null)
                {
                    // Check if this target property is already targeted by any custom mapping
                    var isTargeted = customMappings.Exists(m =>
                        m.TargetPropertyPath.Split('.').Last() == targetProp.Name);
        /// <summary>
        /// Checks if a target property is already targeted in custom mappings (not ignored).
        /// </summary>
        /// <param name="targetPropertyName"></param>
        /// <param name="customMappings"></param>
        /// <returns></returns>
                    if (!isTargeted)
                    {
                        var sourceValue = sourceProp.GetValue(source);
        /// <summary>
        /// Checks if a target property is ignored in custom mappings.
        /// </summary>
        /// <param name="targetPropertyName"></param>
        /// <param name="customMappings"></param>
        /// <returns></returns>
        private bool IsIgnoredInCustomMappings(string targetPropertyName, List<PropertyMapping> customMappings)
        {
            return customMappings.Exists(m =>
                m.TargetPropertyPath.Split('.').Last() == targetPropertyName && m.IsIgnored);
        }

        /// <summary>
        /// Holds custom converters registered in the mapper.
        /// </summary>
        private readonly Dictionary<string, Delegate> _converters = new Dictionary<string, Delegate>();
        /// <summary>
        /// Validates the mapping configuration between the specified source and target types, ensuring that all specified source and target properties exist. Returns a ValidationResult indicating whether the configuration is valid and any errors found.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        public ValidationResult ValidateMapping<TSource, TTarget>()
        {
            var errors = new List<string>();
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            // Check if mapping configuration exists
            if (TryGetConfiguration(sourceType, targetType, out var config))
            {
                if (config.Mappings != null)
                {
                    foreach (var mapping in config.Mappings)
                    {
                        // Validate source property exists
                        if (!mapping.IsIgnored && !mapping.IsNested && !string.IsNullOrEmpty(mapping.SourcePropertyPath))
                        {
                            // Handle nested object mapping
                            if (sourceValue != null)
                            {
                                var nestedTargetValue = targetProp.GetValue(target);
                                if (nestedTargetValue == null)
                                {
                                    nestedTargetValue = Activator.CreateInstance(targetProp.PropertyType);
                                    targetProp.SetValue(target, nestedTargetValue);
                                }

                                var nestedSourceValue = sourceValue;
                                // Use reflection to call the right generic method for nested mapping
                                var genericMethod = typeof(Mapper).GetMethod(nameof(ApplyNameBasedMapping),
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                var specificMethod = genericMethod.MakeGenericMethod(sourceProp.PropertyType, targetProp.PropertyType);
                                specificMethod.Invoke(this, new object[] { nestedSourceValue, nestedTargetValue });
                            }
                            else
                            {
                                targetProp.SetValue(target, null);
                            }
                        }
                        else
                        {
                            // Handle simple types or type conversion
                            var convertedValue = ConvertValue(sourceValue, targetProp.PropertyType);
                            targetProp.SetValue(target, convertedValue);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Checks if a property (including nested properties) exists on the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyPath"></param>
        /// <returns></returns>
        private bool PropertyExists(Type type, string propertyPath)
        {
            var properties = propertyPath.Split('.');
            Type currentType = type;

            foreach (var prop in properties)
            {
                var propertyInfo = currentType.GetProperty(prop);
                if (propertyInfo == null)
                    return false;

                currentType = propertyInfo.PropertyType;
            }

            return true;
        }
        /// <summary>
        /// Creates an instance of the specified type using a cached factory function for performance.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryConvertWithCustomConverter(object value, Type targetType, out object result)
        {
            result = null;

            if (value == null)
                return false;

            var key = $"{value.GetType().FullName}_{targetType.FullName}";

            if (_converters.TryGetValue(key, out var converter))
            {
                var funcType = typeof(Func<,>).MakeGenericType(value.GetType(), targetType);
                if (converter.GetType() == funcType || converter.GetType().IsSubclassOf(typeof(MulticastDelegate)))
                {
                    // Use reflection to invoke the appropriate converter function
                    result = converter.DynamicInvoke(value);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Creates an instance of the specified type using a cached factory function for performance.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        internal void ApplyNameBasedMapping<TSource, TTarget>(TSource source, TTarget target)
        {
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                var targetProp = targetProps.FirstOrDefault(
                    p => p.Name == sourceProp.Name && p.CanWrite);

                if (targetProp != null)
                {
                    var sourceValue = sourceProp.GetValue(source);
        /// <summary>
        /// Gets or creates a cached factory function for creating instances of the specified type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="sourceProp"></param>
        /// <param name="targetProp"></param>
                    if (IsComplexType(sourceProp.PropertyType) && IsComplexType(targetProp.PropertyType))
                    {
                        // Handle nested object mapping
                        if (sourceValue != null)
                        {
                            var nestedTargetValue = targetProp.GetValue(target);
                            if (nestedTargetValue == null)
                            {
                                nestedTargetValue = Activator.CreateInstance(targetProp.PropertyType);
                                targetProp.SetValue(target, nestedTargetValue);
                            }

                            // Recursively map the nested object properties using reflection
                            var genericMethod = typeof(Mapper).GetMethod(nameof(ApplyNameBasedMapping),
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var specificMethod = genericMethod.MakeGenericMethod(sourceProp.PropertyType, targetProp.PropertyType);
                            specificMethod.Invoke(this, new object[] { sourceValue, nestedTargetValue });
                        }
                        else
                        {
                            targetProp.SetValue(target, null);
                        }
                    }
                    else
                    {
                        // Handle simple types or type conversion
                        var convertedValue = ConvertValue(sourceValue, targetProp.PropertyType);
                        targetProp.SetValue(target, convertedValue);
                    }
                }
            }
        }
        /// <summary>
        /// Handles the mapping of complex type properties, including creating nested target objects as needed and recursively mapping their properties.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="sourceValue"></param>
        /// <param name="target"></param>
        /// <param name="targetProp"></param>
        /// <param name="targetSetter"></param>
        private void HandleComplexTypeMapping<TSource, TTarget>(
            object sourceValue,
            TTarget target,
            System.Reflection.PropertyInfo targetProp,
            Action<object, object> targetSetter)
        {
            if (sourceValue != null)
            {
                var nestedTargetValue = GetOrCreateNestedObject(target, targetProp);
                // Recursively map the nested object properties using reflection
                var genericMethod = typeof(Mapper).GetMethod(nameof(ApplyNameBasedMapping),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var specificMethod = genericMethod.MakeGenericMethod(sourceValue.GetType(), targetProp.PropertyType);
                specificMethod.Invoke(this, new object[] { sourceValue, nestedTargetValue });
            }
            else
            {
                targetSetter?.Invoke(target, null);
            }
        }
        /// <summary>
        /// Handles the mapping of simple type properties, including type conversion as needed.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="sourceValue"></param>
        /// <param name="target"></param>
        /// <param name="targetProp"></param>
        /// <param name="targetSetter"></param>
        private void HandleSimpleTypeMapping<TTarget>(
            object sourceValue,
            TTarget target,
            System.Reflection.PropertyInfo targetProp,
            Action<object, object> targetSetter)
        {
            try
            {
                var convertedValue = ConvertValue(sourceValue, targetProp.PropertyType);
                targetSetter?.Invoke(target, convertedValue);
            }
            catch
            {
                // If conversion fails, skip the property mapping (leave target property at its default value)
                // This allows for graceful handling of incompatible types
            }
        }
        /// <summary>
        /// Gets or creates a cached getter function for the specified property of the given type.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="target"></param>
        /// <param name="targetProp"></param>
        /// <returns></returns>
        private object GetOrCreateNestedObject<TTarget>(
            TTarget target,
            System.Reflection.PropertyInfo targetProp)
        {
            var targetGetter = GetOrCreateGetter(typeof(TTarget), targetProp.Name);
            var targetSetter = GetOrCreateSetter(typeof(TTarget), targetProp.Name);

            var nestedTargetValue = targetGetter?.Invoke(target);
            if (nestedTargetValue == null)
            {
                nestedTargetValue = CreateInstance(targetProp.PropertyType);
                targetSetter?.Invoke(target, nestedTargetValue);
            }
            return nestedTargetValue;
        }
        /// <summary>
        /// Creates an instance of the specified type using a cached factory function for performance.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyPath"></param>
        /// <returns></returns>
        private object GetNestedValue(object obj, string propertyPath)
        {
            var properties = propertyPath.Split('.');
            var currentObject = obj;

            foreach (var property in properties)
            {
                if (currentObject == null)
                    return null;

                var propInfo = currentObject.GetType().GetProperty(property);
                if (propInfo == null)
                    return null;

                currentObject = propInfo.GetValue(currentObject);
            }

            return currentObject;
        }
        /// <summary>
        /// Sets a nested property value on the target object, creating intermediate objects as needed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyPath"></param>
        /// <param name="value"></param>
        private void SetNestedValue(object obj, string propertyPath, object value)
        {
            var properties = propertyPath.Split('.');
            var currentObject = obj;

            for (var i = 0; i < properties.Length - 1; i++)
            {
                var propInfo = currentObject.GetType().GetProperty(properties[i]);
                if (propInfo == null)
                    return;

                var nestedValue = propInfo.GetValue(currentObject);
                if (nestedValue == null)
                {
                    nestedValue = Activator.CreateInstance(propInfo.PropertyType);
                    propInfo.SetValue(currentObject, nestedValue);
                }

                currentObject = nestedValue;
            }

            var lastPropInfo = currentObject.GetType().GetProperty(properties[properties.Length - 1]);
            if (lastPropInfo != null)
            {
                var convertedValue = ConvertValue(value, lastPropInfo.PropertyType);
                lastPropInfo.SetValue(currentObject, convertedValue);
            }
        }
        /// <summary>
        /// Gets or creates a cached getter function for the specified property of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsComplexType(Type type)
        {
            if (type == typeof(string))
                return false;
            if (type.IsValueType)
                return false;
            if (type.IsArray)
                return false;
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
                return false;
            return true;
        }
        /// <summary>
        /// Determines if the specified type is a nullable type (e.g., Nullable<T>).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        /// <summary>
        /// Gets or creates a cached getter function for the specified property of the given type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return null;
            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString(), ignoreCase: true);

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(Guid))
                return Guid.Parse(value.ToString());

            if (targetType.IsValueType)
            {
                try
                {
                    if (targetType == typeof(int) && value is double doubleValue)
                        return (int)doubleValue; // Explicit truncation for double to int
                    else if (targetType == typeof(int) && value is float floatValue)
                        return (int)floatValue; // Explicit truncation for float to int
                    else
                        return Convert.ChangeType(value, targetType);
                }
                catch (FormatException)
                {
                    // If conversion fails, return default value for the target type
                    return Activator.CreateInstance(targetType);
                }
                catch (InvalidCastException)
                {
                    // If conversion fails, return default value for the target type
                    return Activator.CreateInstance(targetType);
                }
            }

            // Handle complex types recursively - this is for when we need to convert
            // an object of one type to another (e.g., assigning Address object to AddressWithConfig property)
            if (IsComplexType(targetType) && value != null && !targetType.IsAssignableFrom(value.GetType()))
            {
                try
                {
                    // Use the main Map method to convert the object from one type to another
                    var sourceType = value.GetType();
                    var genericMapMethod = typeof(Mapper).GetMethod(nameof(Map),
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                    if (genericMapMethod != null)
                    {
                        var specificMapMethod = genericMapMethod.MakeGenericMethod(sourceType, targetType);
                        return specificMapMethod.Invoke(this, new object[] { value });
                    }
                }
                catch
                {
                    // If mapping fails, return the original value
                    return value;
                }
            }

            return value;
        }
        /// <summary>
        /// Maps an object of unknown type to the specified target type using reflection to invoke the generic Map method.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private object Map(object source, Type targetType)
        {
            var sourceType = source.GetType();
            // Get the generic Map method (TTarget Map<TSource, TTarget>(TSource source))
            var genericMapMethod = typeof(Mapper).GetMethod(nameof(Map),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (genericMapMethod == null)
            {
                throw new InvalidOperationException($"Could not find Map method for source type {sourceType}");
            }

            var specificMapMethod = genericMapMethod.MakeGenericMethod(sourceType, targetType);
            return specificMapMethod.Invoke(this, new object[] { source });
        }
        /// <summary>
        /// Gets the properties of the specified type, using a cache for performance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private System.Reflection.PropertyInfo[] GetTypeProperties(Type type)
        {
            if (_propertyCache.TryGetValue(type, out var properties))
            {
                return properties;
            }

            properties = type.GetProperties();
            _propertyCache[type] = properties;
            return properties;
        }
        /// <summary>
        /// Gets or creates a cached factory function for creating instances of the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Func<object> GetOrCreateFactory(Type type)
        {
            if (_factoryCache.TryGetValue(type, out var factory))
            {
                return factory;
            }

            // Create factory using reflection
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                // If no parameterless constructor, return null or throw exception
                throw new InvalidOperationException($"Type {type} does not have a parameterless constructor");
            }

            // Create factory delegate using expression trees for better performance
            var newExpr = System.Linq.Expressions.Expression.New(constructor);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<object>>(newExpr);
            factory = lambda.Compile();

            _factoryCache[type] = factory;
            return factory;
        }
        /// <summary>
        /// Creates an instance of the specified type using a cached factory function for performance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private object CreateInstance(Type type)
        {
            var factory = GetOrCreateFactory(type);
            return factory();
        }
        /// <summary>
        /// Gets or creates a cached getter function for the specified property of the given type.
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private Func<object, object> GetOrCreateGetter(Type objType, string propertyName)
        {
            var cacheKey = $"{objType.FullName}.{propertyName}";

            if (_getterCache.TryGetValue(cacheKey, out var getter))
            {
                return getter;
            }

            var propertyInfo = objType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return null;
            }

            var param = System.Linq.Expressions.Expression.Parameter(typeof(object), "obj");
            var convertParam = System.Linq.Expressions.Expression.Convert(param, objType);
            var property = System.Linq.Expressions.Expression.Property(convertParam, propertyInfo);
            var convertResult = System.Linq.Expressions.Expression.Convert(property, typeof(object));

            var lambda = System.Linq.Expressions.Expression.Lambda<Func<object, object>>(convertResult, param);
            getter = lambda.Compile();

            _getterCache[cacheKey] = getter;
            return getter;
        }
        /// <summary>
        /// Gets or creates a cached setter function for the specified property of the given type.
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private Action<object, object> GetOrCreateSetter(Type objType, string propertyName)
        {
            var cacheKey = $"{objType.FullName}.{propertyName}";

            if (_setterCache.TryGetValue(cacheKey, out var setter))
            {
                return setter;
            }

            var propertyInfo = objType.GetProperty(propertyName);
            if (propertyInfo == null || !propertyInfo.CanWrite)
            {
                return null;
            }

            var objParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "obj");
            var valueParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "value");

            var convertObj = System.Linq.Expressions.Expression.Convert(objParam, objType);
            var convertValue = System.Linq.Expressions.Expression.Convert(valueParam, propertyInfo.PropertyType);
            var property = System.Linq.Expressions.Expression.Property(convertObj, propertyInfo);
            var assign = System.Linq.Expressions.Expression.Assign(property, convertValue);

            var lambda = System.Linq.Expressions.Expression.Lambda<Action<object, object>>(assign, objParam, valueParam);
            setter = lambda.Compile();

            _setterCache[cacheKey] = setter;
            return setter;
        }
        /// <summary>
        /// Maps a source object of type TSource to a new instance of type TDestination using the configured mappings.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
        {
            if (source == null)
                return null;

            var result = new List<TDestination>();
            foreach (var item in source)
            {
                result.Add(Map<TSource, TDestination>(item));
            }
            return result;
        }
    }
}