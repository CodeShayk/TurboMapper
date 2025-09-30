using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TurboMapper.Tests")]

namespace TurboMapper.Impl
{
    internal class Mapper : IMapper, IObjectMap
    {
        private readonly Dictionary<Type, Dictionary<Type, List<PropertyMapping>>> _configurations;

        public Mapper()
        {
            _configurations = new Dictionary<Type, Dictionary<Type, List<PropertyMapping>>>();
        }

        public void CreateMap<TSource, TTarget>(List<PropertyMapping> mappings = null)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            if (!_configurations.ContainsKey(sourceType))
                _configurations[sourceType] = new Dictionary<Type, List<PropertyMapping>>();

            _configurations[sourceType][targetType] = mappings ?? new List<PropertyMapping>();
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
                var mappings = _configurations[sourceType][targetType];
                ApplyCustomMappings(source, target, mappings);
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

        private void ApplyDefaultNameBasedMapping<TSource, TTarget>(
            TSource source,
            TTarget target,
            List<PropertyMapping> customMappings)
        {
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                // Check if this property is already mapped in custom mappings
                var isMapped = customMappings.Exists(m =>
                    m.SourcePropertyPath.Split('.').Last() == sourceProp.Name);

                if (!isMapped)
                {
                    var targetProp = targetProps.FirstOrDefault(p =>
                        p.Name == sourceProp.Name &&
                        p.CanWrite &&
                        p.PropertyType.IsAssignableFrom(sourceProp.PropertyType));

                    if (targetProp != null)
                    {
                        var sourceValue = sourceProp.GetValue(source);
                        var convertedValue = ConvertValue(sourceValue, targetProp.PropertyType);
                        targetProp.SetValue(target, convertedValue);
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
                            ApplyNameBasedMapping(nestedSourceValue, nestedTargetValue);
                        }
                        else
                        {
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
            if (type.GetInterface("IEnumerable") != null)
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
                return Enum.Parse(targetType, value.ToString());

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType.IsValueType)
                return Convert.ChangeType(value, targetType);

            // Handle complex types recursively
            if (IsComplexType(targetType) && value != null)
                return Map(value, targetType);

            return value;
        }

        private object Map(object source, Type targetType)
        {
            var sourceType = source.GetType();
            var mapMethod = typeof(Mapper).GetMethod(nameof(Map), new[] { sourceType, targetType });
            var genericMapMethod = mapMethod.MakeGenericMethod(sourceType, targetType);
            return genericMapMethod.Invoke(this, new object[] { source });
        }
    }
}