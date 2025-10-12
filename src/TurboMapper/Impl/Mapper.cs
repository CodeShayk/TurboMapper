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
        /// Holds mapping configurations indexed by source type and target type.
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
        /// Caches factory functions for creating instances of types to optimize object creation performance.
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
            _propertyCache = new Dictionary<Type, System.Reflection.PropertyInfo[]>();
            _propertyPathCache = new Dictionary<string, System.Reflection.PropertyInfo>();
            _factoryCache = new Dictionary<Type, Func<object>>();
            _getterCache = new Dictionary<string, Func<object, object>>();
            _setterCache = new Dictionary<string, Action<object, object>>();
        }

        /// <summary>
        /// Creates a mapping configuration between TSource and TTarget types with optional property mappings.
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
        /// Creates a mapping configuration between TSource and TTarget types with specified property mappings and an option to enable default mapping for unmapped properties.
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
        /// Maps an instance of TSource to a new instance of TTarget, applying any configured property mappings, including support for nested properties, conditional mapping, and transformation functions.
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

            var target = (TTarget)CreateInstance(targetType);

            // Check for custom mapping configuration
            if (TryGetConfiguration(sourceType, targetType, out var config))
            {
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
        /// Applies custom mappings with default mapping enabled. Ignores properties marked as ignored and applies conditions if specified.
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
            // First, apply all non-ignored custom mappings that meet conditions
            foreach (var mapping in mappings)
            {
                if (!mapping.IsIgnored && (mapping.Condition == null || mapping.Condition(source)))
                {
                    var sourceValue = GetNestedValue(source, mapping.SourcePropertyPath);

                    // Apply transformation if available
                    if (mapping.TransformFunction != null && sourceValue != null)
                    {
                        // Use reflection to call the transformation function
                        var transformFunc = (Delegate)mapping.TransformFunction;
                        var transformedValue = transformFunc.DynamicInvoke(sourceValue);
                        SetNestedValue(target, mapping.TargetPropertyPath, transformedValue);
                    }
                    else
                    {
                        SetNestedValue(target, mapping.TargetPropertyPath, sourceValue);
                    }
                }
            }

            // Then apply default name-based mapping for unmapped properties
            ApplyDefaultNameBasedMapping(source, target, mappings);
        }

        /// <summary>
        /// Applies only custom mappings with default mapping disabled. Ignores properties marked as ignored and applies conditions if specified.
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
            // Apply only non-ignored custom mappings that meet conditions, no default mappings
            foreach (var mapping in mappings)
            {
                if (!mapping.IsIgnored && (mapping.Condition == null || mapping.Condition(source)))
                {
                    var sourceValue = GetNestedValue(source, mapping.SourcePropertyPath);

                    // Apply transformation if available
                    if (mapping.TransformFunction != null && sourceValue != null)
                    {
                        // Use reflection to call the transformation function
                        var transformFunc = (Delegate)mapping.TransformFunction;
                        var transformedValue = transformFunc.DynamicInvoke(sourceValue);
                        SetNestedValue(target, mapping.TargetPropertyPath, transformedValue);
                    }
                    else
                    {
                        SetNestedValue(target, mapping.TargetPropertyPath, sourceValue);
                    }
                }
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
        /// Applies default name-based mapping for properties not explicitly mapped, ignoring any properties that are marked as ignored in custom mappings.
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
            var sourceProps = GetTypeProperties(typeof(TSource));
            var targetProps = GetTypeProperties(typeof(TTarget));

            foreach (var sourceProp in sourceProps)
            {
                // For default mapping, check if this source property maps to any target property in custom mappings
                // by checking if there's a custom mapping that targets a property with the same name as sourceProp.Name
                var targetProp = targetProps.FirstOrDefault(p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite);

                if (targetProp != null && !IsTargetedInCustomMappings(targetProp.Name, customMappings))
                {
                    ProcessPropertyMapping(source, target, sourceProp, targetProp);
                }
            }
        }

        /// <summary>
        /// Checks if a target property is explicitly targeted in custom mappings (not ignored).
        /// </summary>
        /// <param name="targetPropertyName"></param>
        /// <param name="customMappings"></param>
        /// <returns></returns>
        private bool IsTargetedInCustomMappings(string targetPropertyName, List<PropertyMapping> customMappings)
        {
            return customMappings.Exists(m =>
                m.TargetPropertyPath.Split('.').Last() == targetPropertyName && !m.IsIgnored);
        }

        /// <summary>
        /// Checks if a target property is marked as ignored in custom mappings.
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
        /// Holds custom converters registered for specific source-target type pairs.
        /// </summary>
        private readonly Dictionary<string, Delegate> _converters = new Dictionary<string, Delegate>();

        /// <summary>
        /// Creates an instance of the specified type using a cached factory function for performance.
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
                            if (!PropertyExists(sourceType, mapping.SourcePropertyPath))
                            {
                                errors.Add($"Source property '{mapping.SourcePropertyPath}' does not exist on type '{sourceType.Name}'");
                            }
                        }

                        // Validate target property exists
                        if (!mapping.IsIgnored && !mapping.IsNested && !string.IsNullOrEmpty(mapping.TargetPropertyPath))
                        {
                            if (!PropertyExists(targetType, mapping.TargetPropertyPath))
                            {
                                errors.Add($"Target property '{mapping.TargetPropertyPath}' does not exist on type '{targetType.Name}'");
                            }
                        }
                    }
                }
            }

            var isValid = errors.Count == 0;
            return new ValidationResult(isValid, errors);
        }

        /// <summary>
        /// Checks if a property exists on a type, supporting nested properties using dot notation.
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
        /// Gets or creates a factory function for the specified type to optimize object creation.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        internal void ApplyNameBasedMapping<TSource, TTarget>(TSource source, TTarget target)
        {
            var sourceProps = GetTypeProperties(typeof(TSource));
            var targetProps = GetTypeProperties(typeof(TTarget));

            foreach (var sourceProp in sourceProps)
            {
                var targetProp = targetProps.FirstOrDefault(
                    p => p.Name == sourceProp.Name && p.CanWrite);

                if (targetProp != null)
                {
                    ProcessPropertyMapping(source, target, sourceProp, targetProp);
                }
            }
        }

        /// <summary>
        /// Processes the mapping of a single property from source to target, handling both simple and complex types, including nested objects.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="sourceProp"></param>
        /// <param name="targetProp"></param>
        private void ProcessPropertyMapping<TSource, TTarget>(
            TSource source,
            TTarget target,
            System.Reflection.PropertyInfo sourceProp,
            System.Reflection.PropertyInfo targetProp)
        {
            var sourceGetter = GetOrCreateGetter(typeof(TSource), sourceProp.Name);
            var targetSetter = GetOrCreateSetter(typeof(TTarget), targetProp.Name);

            var sourceValue = sourceGetter?.Invoke(source);

            if (IsComplexType(sourceProp.PropertyType) && IsComplexType(targetProp.PropertyType))
            {
                HandleComplexTypeMapping<TSource, TTarget>(sourceValue, target, targetProp, targetSetter);
            }
            else
            {
                HandleSimpleTypeMapping(sourceValue, target, targetProp, targetSetter);
            }
        }

        /// <summary>
        /// Handles the mapping of complex type properties, creating nested target objects as needed and recursively applying name-based mapping.
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
        /// Handles the mapping of simple type properties, applying type conversion as needed and gracefully handling conversion failures.
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
        /// Get or create nested object for complex type properties, creating the instance if it does not already exist.
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
        /// Get nested property value using dot notation for property paths.
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

                var type = currentObject.GetType();
                var getter = GetOrCreateGetter(type, property);

                if (getter == null)
                    return null;

                currentObject = getter(currentObject);
            }

            return currentObject;
        }

        /// <summary>
        /// Set nested property value using dot notation for property paths, creating intermediate objects as needed.
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
                var type = currentObject.GetType();
                var getter = GetOrCreateGetter(type, properties[i]);

                if (getter == null)
                    return;

                var nestedValue = getter(currentObject);
                if (nestedValue == null)
                {
                    nestedValue = CreateInstance(type.GetProperty(properties[i]).PropertyType);
                    var innerSetter = GetOrCreateSetter(type, properties[i]); // Renamed from 'setter' to 'innerSetter'
                    if (innerSetter != null)
                        innerSetter(currentObject, nestedValue);
                }

                currentObject = nestedValue;
            }

            var lastType = currentObject.GetType();
            var lastPropertyName = properties[properties.Length - 1];
            var setter = GetOrCreateSetter(lastType, lastPropertyName);

            if (setter != null)
            {
                var lastPropInfo = lastType.GetProperty(lastPropertyName);
                var convertedValue = ConvertValue(value, lastPropInfo.PropertyType);
                setter(currentObject, convertedValue);
            }
        }

        /// <summary>
        /// Is Complex Type (i.e., class but not string, array, or collection)
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
        /// Checks if the specified type is a nullable type (e.g., Nullable<int>).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Convert value to the specified target type, handling nullable types, enums, and using custom converters if available.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return null;

            // First, try custom converters
            if (TryConvertWithCustomConverter(value, targetType, out var customResult))
            {
                return customResult;
            }

            // Handle nullable types
            if (IsNullableType(targetType))
            {
                if (value == null)
                    return null;

                var underlyingType = Nullable.GetUnderlyingType(targetType);
                var convertedValue = ConvertValue(value, underlyingType);
                return convertedValue;
            }

            // If source is nullable and target is not, extract the value
            if (IsNullableType(value.GetType()))
            {
                var underlyingType = Nullable.GetUnderlyingType(value.GetType());
                if (underlyingType != null)
                {
                    var property = value.GetType().GetProperty("Value");
                    if (property != null)
                    {
                        value = property.GetValue(value);
                    }
                }
            }

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, value.ToString(), ignoreCase: true);
                }
                catch (ArgumentException ex)  // This catches invalid enum values
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to convert '{value}' to enum type '{targetType.Name}': {ex.Message}", ex);
                }
            }

            if (targetType == typeof(string))
            {
                return value?.ToString();
            }

            if (targetType == typeof(Guid))
            {
                try
                {
                    return Guid.Parse(value.ToString());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to convert '{value}' to Guid: {ex.Message}", ex);
                }
            }

            if (targetType.IsValueType)
            {
                try
                {
                    // Enhanced type conversion with more specific handling
                    if (targetType == typeof(int))
                    {
                        if (value is double doubleValue)
                            return (int)doubleValue; // Explicit truncation for double to int
                        else if (value is float floatValue)
                            return (int)floatValue; // Explicit truncation for float to int
                        else if (value is decimal decimalValue)
                            return (int)decimalValue;
                        else
                            return Convert.ToInt32(value);
                    }
                    else if (targetType == typeof(long))
                    {
                        return Convert.ToInt64(value);
                    }
                    else if (targetType == typeof(float))
                    {
                        if (value is double doubleValue)
                            return (float)doubleValue;
                        else if (value is decimal decimalValue)
                            return (float)decimalValue;
                        else
                            return Convert.ToSingle(value);
                    }
                    else if (targetType == typeof(double))
                    {
                        if (value is float floatValue)
                            return (double)floatValue;
                        else if (value is decimal decimalValue)
                            return (double)decimalValue;
                        else
                            return Convert.ToDouble(value);
                    }
                    else if (targetType == typeof(decimal))
                    {
                        if (value is double doubleValue)
                            return (decimal)doubleValue;
                        else if (value is float floatValue)
                            return (decimal)floatValue;
                        else
                            return Convert.ToDecimal(value);
                    }
                    else if (targetType == typeof(DateTime))
                    {
                        if (value is string stringValue)
                            return DateTime.Parse(stringValue);
                        else if (value is long longValue) // Assuming timestamp
                            return DateTime.FromBinary(longValue);
                        else
                            return (DateTime)Convert.ChangeType(value, targetType);
                    }
                    else if (targetType == typeof(TimeSpan))
                    {
                        if (value is string stringValue)
                            return TimeSpan.Parse(stringValue);
                        else if (value is long ticksValue)
                            return TimeSpan.FromTicks(ticksValue);
                        else
                            return (TimeSpan)Convert.ChangeType(value, targetType);
                    }
                    else
                    {
                        return Convert.ChangeType(value, targetType);
                    }
                }
                catch (FormatException ex)
                {
                    throw new InvalidOperationException($"Failed to convert '{value}' (type: {value.GetType().Name}) to '{targetType.Name}': Format exception - {ex.Message}", ex);
                }
                catch (InvalidCastException ex)
                {
                    throw new InvalidOperationException($"Failed to convert '{value}' (type: {value.GetType().Name}) to '{targetType.Name}': Invalid cast - {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to convert '{value}' (type: {value.GetType().Name}) to '{targetType.Name}': {ex.Message}", ex);
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
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to map complex type '{value.GetType().Name}' to '{targetType.Name}': {ex.Message}", ex);
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
        /// Gets the properties of a type, using a cache to optimize repeated access.
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
        /// Gets or creates a factory function for the specified type to optimize object creation.
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
        /// Gets or creates a getter function for the specified property of the given type, using a cache to optimize repeated access.
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
        /// Gets or creates a setter function for the specified property of the given type, using a cache to optimize repeated access.
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