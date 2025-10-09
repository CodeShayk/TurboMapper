using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TurboMapper.Tests")]

namespace TurboMapper.Impl
{
    internal class MapperConfiguration
    {
        public List<PropertyMapping> Mappings { get; set; }
        public bool EnableDefaultMapping { get; set; }

        public MapperConfiguration(List<PropertyMapping> mappings, bool enableDefaultMapping)
        {
            Mappings = mappings ?? new List<PropertyMapping>();
            EnableDefaultMapping = enableDefaultMapping;
        }
    }

    internal class Mapper : IMapper, IObjectMap
    {
        private readonly Dictionary<Type, Dictionary<Type, MapperConfiguration>> _configurations;

        public Mapper()
        {
            _configurations = new Dictionary<Type, Dictionary<Type, MapperConfiguration>>();
        }

        public void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings = null)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_configurations.ContainsKey(sourceType))
                _configurations[sourceType] = new Dictionary<Type, MapperConfiguration>();

            _configurations[sourceType][targetType] = new MapperConfiguration(mappings, true);
        }

        public void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings, bool enableDefaultMapping)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_configurations.ContainsKey(sourceType))
                _configurations[sourceType] = new Dictionary<Type, MapperConfiguration>();

            _configurations[sourceType][targetType] = new MapperConfiguration(mappings, enableDefaultMapping);
        }

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

                    if (!isTargeted)
                    {
                        var sourceValue = sourceProp.GetValue(source);

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
    }
}