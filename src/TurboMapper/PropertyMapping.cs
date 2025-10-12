using System.Collections.Generic;

namespace TurboMapper
{
    /// <summary>
    /// Defines a property mapping between source and target properties, including support for nested properties, conditional mapping, and transformation functions.
    /// </summary>
    internal class PropertyMapping
    {
        /// <summary>
        /// The name of the source property.
        /// </summary>
        public string SourceProperty { get; set; }
        /// <summary>
        /// The name of the target property.
        /// </summary>
        public string TargetProperty { get; set; }
        /// <summary>
        /// The full path of the source property, supporting nested properties (e.g., "Address.Street").
        /// </summary>
        public string SourcePropertyPath { get; set; }
        /// <summary>
        /// The full path of the target property, supporting nested properties (e.g., "Location.City").
        /// </summary>
        public string TargetPropertyPath { get; set; }
        /// <summary>
        /// Indicates whether the property is a nested object requiring its own mapping configuration.
        /// </summary>
        public bool IsNested { get; set; }
        /// <summary>
        /// Indicates whether the property should be ignored during the mapping process.
        /// </summary>
        public bool IsIgnored { get; set; }
        /// <summary>
        /// A condition function that determines whether the property should be mapped based on the source object.
        /// </summary>
        public Func<object, bool> Condition { get; set; }  
        /// <summary>
        /// A transformation function that converts the source property value to the target property value.
        /// </summary>
        public object TransformFunction { get; set; }
        /// <summary>
        /// A list of nested property mappings for complex types.
        /// </summary>
        public List<PropertyMapping> NestedMappings { get; set; } = new List<PropertyMapping>();
    }
}