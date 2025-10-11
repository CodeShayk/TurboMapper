using System;
using System.Collections.Generic;

namespace TurboMapper
{
    internal class PropertyMapping
    {
        public string SourceProperty { get; set; }
        public string TargetProperty { get; set; }
        public string SourcePropertyPath { get; set; }
        public string TargetPropertyPath { get; set; }
        public bool IsNested { get; set; }
        public bool IsIgnored { get; set; }
        public Func<object, bool> Condition { get; set; }  // For conditional mapping
        public object TransformFunction { get; set; }  // For transformation functions
        public List<PropertyMapping> NestedMappings { get; set; } = new List<PropertyMapping>();
    }
}