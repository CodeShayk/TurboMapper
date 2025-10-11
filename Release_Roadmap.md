# TurboMapper Release Roadmap

## Overview
This document provides a summary of TurboMapper releases with key details about each release.

| Release Version | Release Date | Key Features | Backward Compatibility | Primary Focus |
|----------------|--------------|--------------|----------------------|---------------|
| 1.2.0 | TBA | Performance improvements (2x+ speed), collection mapping, custom type converters, conditional mapping, transformation functions, configuration validation, improved error messages | ✅ Fully backward compatible | Core improvements, mapping features, custom conversions |
| 1.4.0 | TBA | Complex nested mapping, circular reference handling, performance diagnostics, generic collection interfaces, interface-to-concrete mapping, dictionary mapping, .NET Standard compatibility | ✅ Fully backward compatible | Advanced mapping, type features, enhanced conversions |
| 2.1.0 | TBA | Pre-compiled mappings, reverse mapping, async transformations, async collection processing, LINQ expressions, projection support, detailed tracing | ❌ Contains breaking changes (new async methods in IMapper) | Next-gen features, async operations, data access integration |

## Detailed Release Notes

### Release 1.2.0 - Enhanced Core & Mapping Features
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

### Release 1.4.0 - Advanced Mapping & Type Features
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

### Release 2.1.0 - Next-Gen Features & Breaking Changes
- **New Feature**: Pre-compilation of mappings for better runtime performance with compile-time validation
- **New Feature**: Reverse mapping capabilities from TTarget to TSource
- **New Feature**: Detailed optional mapping execution tracing for debugging
- **New Feature**: Asynchronous transformation functions and async collection processing
- **New Feature**: Support for mapping in LINQ expressions with expression tree transformation
- **New Feature**: Projection support for efficient database queries compatible with Entity Framework and similar ORMs
- **Breaking Change**: New async mapping methods added to IMapper interface
- **Improvement**: Enhanced performance with pre-compiled mappings
- **Integration**: Enhanced compatibility with data access technologies
- **Enhanced Custom Conversion**: Full async and LINQ-enabled custom conversions in MappingModule

## Migration Notes
- **1.2.0 to 1.4.0**: Fully backward compatible, no migration required
- **1.4.0 to 2.1.0**: Contains breaking changes requiring updates to IMapper implementations to include new async methods
- **Custom Conversion Evolution**: Enhanced across all releases with increasing capabilities (basic → advanced → async/LINQ-enabled)

## Impact Summary
- **Performance**: Significant improvements in releases 1.2.0 and 2.1.0
- **Functionality**: New features added in each release with advanced capabilities
- **Compatibility**: Careful attention to maintaining compatibility where possible
- **Conversion**: Enhanced custom conversion features integrated throughout the MappingModule system
