# Roadmap - Planned Releases

## Overview
This document outlines the consolidated release plan for TurboMapper enhancements, grouping features from versions 1.1.0 to 2.1.0 into three major releases with enhanced custom conversion feature support in mapping modules. Each release contains a collection of related features and improvements with details on compatibility and impact.

## Release 1: Enhanced Core & Mapping Features (v1.2.0)

### Version: 1.2.0 (Minor Release)
**Release Date**: TBA
**Type**: Minor Release (backward compatible new features and improvements)

### Included Tasks:
- Task 1.1: Refactor Duplicated GetMemberPath Methods
- Task 1.2: Implement Caching for Reflection Metadata
- Task 1.3: Optimize Object Creation
- Task 1.4: Simplify Complex Methods
- Task 2.1: Implement Compiled Expression Trees
- Task 2.2: Add Configuration Caching
- Task 3.1: Collection Mapping Support
- Task 3.4: Ignored Properties Option
- Task 5.1: Custom Type Converters Registration System
- Task 5.2: Improved Nullable Type Handling
- Task 6.1: Improved Error Messages for Mapping Failures
- Task 3.2: Conditional Mapping
- Task 3.3: Mapping with Transformation Functions (Enhanced with custom conversion support in MappingModule)
- Task 5.4: Comprehensive Built-in Type Conversions
- Task 6.2: Configuration Validation

### Enhanced Custom Conversion Feature in Mapping Module:
- **New Functionality**: Support for custom property conversion functions directly in MappingModule configuration
- **API Enhancement**: Added `ConvertProperty` method to mapping expressions allowing inline conversion functions
- **Module Integration**: MappingModule now supports conversion function registration for properties
- **Usage Example**: `mapping.ConvertProperty(x => x.Property, value => ConvertToDesiredType(value))`

### Release Notes:
- **Performance Improvements**: Significant performance enhancements through compiled expression trees and metadata caching, with expected 2x+ improvement in mapping speed
- **Code Quality**: Consolidated duplicate code in GetMemberPath methods and simplified complex mapping methods
- **Internal Optimizations**: Replaced Activator.CreateInstance with optimized factory delegates and added configuration caching for faster lookup
- **New Feature**: Collection mapping support for IEnumerable, IList, and arrays
- **New Feature**: Ability to ignore properties during mapping using the Ignore() method
- **New Feature**: Custom type converter registration system for handling complex type conversions
- **Enhancement**: Enhanced nullable type handling with configurable behavior
- **Improvement**: Much more descriptive error messages for debugging mapping issues
- **New Feature**: Conditional mapping allowing properties to be mapped based on conditions using When() method
- **Enhanced Feature**: Transformation functions during mapping using MapWith() method with integrated custom conversion in MappingModule
- **Improvement**: More comprehensive built-in type conversions including DateTime, TimeSpan, and decimal/float conversions
- **Improvement**: Configuration validation at registration time to catch potential mapping issues early

### Backward Compatibility:
- ✅ Fully backward compatible
- No breaking changes to public APIs
- All existing mapping configurations will continue to work
- Performance improvements are transparent to users
- New features are opt-in

### Impact:
- **Performance**: Significant improvement in mapping speed
- **Memory**: More efficient memory usage through caching
- **Functionality**: Added support for mapping collections and complex type conversions
- **Debuggability**: Much better error messages help identify mapping issues quickly
- **Flexibility**: More control over mapping process with ignore, custom converters, conditions and transformations
- **Reliability**: Early detection of configuration issues
- **Coverage**: Better handling of various data types

---

## Release 2: Advanced Mapping & Type Features (v1.4.0)

### Version: 1.4.0 (Minor Release)
**Release Date**: TBA
**Type**: Minor Release (backward compatible new features)

### Prerequisites:
- Completion of Release 1.2.0

### Included Tasks:
- Task 4.1: Support for Complex Nested Mapping Scenarios
- Task 4.2: Circular Reference Handling
- Task 4.4: Better Handling of Null Nested Objects
- Task 6.3: Performance Diagnostics and Profiling
- Task 8.1: Generic Collection Interface Support
- Task 4.3: Improved Collection Element Mapping
- Task 5.3: Interface-to-Concrete Type Mapping
- Task 8.2: Dictionary Mapping Capabilities
- Task 10.1: .NET Standard Compatibility

### Enhanced Custom Conversion Feature in Mapping Module:
- **Advanced Functionality**: Support for complex type conversions including nested objects and collections
- **Module Integration**: Enhanced MappingModule with conversion chain support for complex scenarios
- **Type Safety**: Improved type safety for custom conversions with compile-time validation
- **Performance**: Optimized conversion execution pathways

### Release Notes:
- **Improvement**: Enhanced support for complex nested mapping scenarios with performance optimization
- **New Feature**: Detection and handling of circular references in objects to prevent infinite recursion
- **Improvement**: Better configurable null handling for nested objects
- **New Feature**: Optional performance diagnostics and profiling capabilities
- **Improvement**: Comprehensive support for generic collection interfaces (IEnumerable, IList, IReadOnlyList, etc.)
- **Improvement**: Enhanced mapping for collections with different element types
- **New Feature**: Support for mapping from interface to concrete implementation
- **New Feature**: Comprehensive mapping support for dictionary types with key and value transformation
- **Improvement**: Better .NET Standard compatibility across target frameworks
- **Enhanced Custom Conversion**: Advanced type conversion features including complex nested conversions in MappingModule

### Backward Compatibility:
- ✅ Fully backward compatible
- New circular reference handling is optional/configurable
- Performance diagnostics are opt-in
- Collection interface support enhances existing functionality
- Enhanced conversions are additional capabilities

### Impact:
- **Robustness**: Better handling of complex object graphs
- **Safety**: Prevention of infinite recursion with circular references
- **Observability**: Performance diagnostics for optimization
- **Compatibility**: Better support for various collection types
- **Flexibility**: More options for mapping complex collection structures
- **Type Support**: Better handling of interface implementations
- **Data Structures**: Full support for dictionary mappings
- **Compatibility**: Improved cross-platform support
- **Conversion**: Advanced type conversion capabilities in MappingModule

---

## Release 3: Next-Gen Features & Breaking Changes (v2.1.0)

### Version: 2.1.0 (Major/Minor Release)
**Release Date**: TBA
**Type**: Major Release (includes breaking changes)

### Prerequisites:
- Completion of Release 1.4.0

### Included Tasks:
- Task 2.3: Implement Pre-compilation of Mappings
- Task 3.5: Reverse Mapping Capabilities
- Task 6.4: Detailed Mapping Execution Tracing
- Task 9.1: Asynchronous Transformation Support
- Task 9.2: Async Collection Processing
- Task 7.1: LINQ Expression Support
- Task 7.2: Projection Support

### Enhanced Custom Conversion Feature in Mapping Module:
- **Async Conversion Support**: Asynchronous custom conversion functions for I/O bound transformations
- **LINQ Integration**: Custom conversions now work within LINQ expressions for database projections
- **Performance Optimization**: Pre-compiled custom conversions for better runtime performance
- **Module Enhancement**: Advanced configuration options for conversion behavior in MappingModule

### Release Notes:
- **New Feature**: Pre-compilation of mappings for better runtime performance with compile-time validation
- **New Feature**: Reverse mapping capabilities from TTarget to TSource
- **New Feature**: Detailed optional mapping execution tracing for debugging
- **New Feature**: Asynchronous transformation functions and async collection processing
- **New Feature**: Support for mapping in LINQ expressions with expression tree transformation
- **New Feature**: Projection support for efficient database queries compatible with Entity Framework and similar ORMs
- **Breaking Change**: New async mapping methods added to IMapper interface
- **Improvement**: Enhanced performance with pre-compiled mappings
- **Integration**: Enhanced compatibility with data access technologies
- **Enhanced Custom Conversion**: Full async support and LINQ integration for custom conversions in MappingModule

### Backward Compatibility:
- ❌ Contains breaking changes
- New async methods added to IMapper interface (requires implementation)
- Reverse mapping configuration might affect existing mapping behaviors
- Pre-compilation introduces new configuration patterns
- Custom conversions maintain compatibility but add new async capabilities

### Impact:
- **Performance**: Significant improvement with pre-compiled mappings
- **Functionality**: Bidirectional mapping capabilities
- **Async Support**: Full async/await patterns for complex operations
- **Debugging**: Detailed tracing for complex debugging scenarios
- **Data Access**: Better integration with ORMs and database queries
- **Performance**: Efficient projection for database operations
- **Capability**: Expression tree transformation for complex scenarios
- **Conversion**: Advanced async and LINQ-enabled custom conversions in MappingModule

### Migration Guide:
- Update IMapper implementations to include new async methods
- Review mapping configurations for compatibility with reverse mapping
- Consider adopting pre-compilation for performance benefits
- Update custom conversion functions to support async operations if needed

---

## Summary of Consolidated Release Strategy

### Version Progression:
1. **1.2.0**: Enhanced Core & Mapping Features (no breaking changes) - Performance, collections, custom conversions, transformations, and validation
2. **1.4.0**: Advanced Mapping & Type Features (no breaking changes) - Nested mapping, circular references, interface mapping, and enhanced conversions
3. **2.1.0**: Next-Gen Features & Breaking Changes (includes breaking changes) - Async operations, LINQ integration, and advanced custom conversions

### Compatibility Summary:
- Version 1.2.0 is fully backward compatible
- Version 1.4.0 maintains backward compatibility
- Version 2.1.0 introduces breaking changes (interface modifications including async methods)

### Enhanced Custom Conversion Feature Summary:
- **Release 1.2.0**: Basic custom conversion with integrated MappingModule support
- **Release 1.4.0**: Advanced conversion for complex scenarios in MappingModule
- **Release 2.1.0**: Async and LINQ-enabled custom conversions in MappingModule

### Priority Features by Release:
- **1.2.0**: Performance improvements, custom conversions in MappingModule, and essential mapping features
- **1.4.0**: Advanced mapping capabilities and enhanced type conversions
- **2.1.0**: Modern async patterns and data access integration with enhanced conversions

This consolidated release strategy groups related features into cohesive releases while maintaining logical progression.
