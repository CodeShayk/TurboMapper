# <img src="https://github.com/CodeShayk/TurboMapper/blob/master/Images/ninja-icon-16.png" alt="ninja" style="width:30px;"/> TurboMapper v1.2.0
[![NuGet version](https://badge.fury.io/nu/TurboMapper.svg)](https://badge.fury.io/nu/TurboMapper) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/CodeShayk/TurboMapper/blob/master/LICENSE.md) 
[![GitHub Release](https://img.shields.io/github/v/release/CodeShayk/TurboMapper?logo=github&sort=semver)](https://github.com/CodeShayk/TurboMapper/releases/latest)
[![master-build](https://github.com/CodeShayk/TurboMapper/actions/workflows/Master-Build.yml/badge.svg)](https://github.com/CodeShayk/TurboMapper/actions/workflows/Master-Build.yml)
[![master-codeql](https://github.com/CodeShayk/TurboMapper/actions/workflows/Master-CodeQL.yml/badge.svg)](https://github.com/CodeShayk/TurboMapper/actions/workflows/Master-CodeQL.yml)
[![.Net 9.0](https://img.shields.io/badge/.Net-9.0-blue)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![.Net Framework 4.6.4](https://img.shields.io/badge/.Net-4.6.2-blue)](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net46)
[![.Net Standard 2.0](https://img.shields.io/badge/.NetStandard-2.0-blue)](https://github.com/dotnet/standard/blob/v2.0.0/docs/versions/netstandard2.0.md)

## Introduction
### What is TurboMapper?
`TurboMapper` is a lightweight, high-performance object mapper for .NET that provides both shallow and deep mapping capabilities. It serves as a free alternative to AutoMapper with a simple, intuitive API.

## Getting Started
### i. Installation
Install the latest version of TurboMapper nuget package with command below. 

```
NuGet\Install-Package TurboMapper 
```

### ii. Quick Start Example
```csharp
using TurboMapper;
using Microsoft.Extensions.DependencyInjection;

// Setup
var services = new ServiceCollection();
services.AddTurboMapper();
var serviceProvider = services.BuildServiceProvider();
var mapper = serviceProvider.GetService<IMapper>();

// Define models
public class Source
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Target
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Map single object
var source = new Source { Name = "John Doe", Age = 30 };
var target = mapper.Map<Source, Target>(source);

// Map collections
var sources = new List<Source>
{
    new Source { Name = "Alice", Age = 25 },
    new Source { Name = "Bob", Age = 32 }
};

// Map to IEnumerable<T>
IEnumerable<Target> targets = mapper.Map<Source, Target>(sources);

// Convert to list if needed
List<Target> targetList = targets.ToList();
```

### iii. Developer Guide
This comprehensive guide provides detailed information on TurboMapper, covering everything from basic concepts to advanced implementations and troubleshooting guidelines.

Please click on [Developer Guide](https://github.com/CodeShayk/TurboMapper/wiki) for complete details.

## Release Roadmap
This section provides the summary of planned releases with key details about each release.

| Release Version | Release Date | Key Features | Backward Compatibility | Primary Focus |
|----------------|--------------|--------------|----------------------|---------------|
| 1.2.0 | October 2025 | Performance improvements (2x+ speed), enhanced collection mapping API, custom type converters, conditional mapping, transformation functions, configuration validation, improved error messages | ✅ Fully backward compatible | Core improvements, mapping features, custom conversions |
| 1.4.0 | Jan 2026 | Complex nested mapping, circular reference handling, performance diagnostics, generic collection interfaces, interface-to-concrete mapping, dictionary mapping, .NET Standard compatibility | ✅ Fully backward compatible | Advanced mapping, type features, enhanced conversions |
| 2.1.0 | Mid 2026 | Pre-compiled mappings, reverse mapping, async transformations, async collection processing, LINQ expressions, projection support, detailed tracing | ❌ Contains breaking changes (new async methods in IMapper) | Next-gen features, async operations, data access integration |

Please see [Release Roadmap](https://github.com/CodeShayk/TurboMapper/blob/master/Release_Roadmap.md) for more details.

## Key Features in Release 1.2.0
- **Performance Improvements**: Significant performance enhancements (2x+) through compiled expression trees and metadata caching
- **Enhanced Collection Mapping**: Simplified API with Map method now supporting both single objects and collections
- **Ignored Properties Option**: Added Ignore method to IMappingExpression to skip properties during mapping
- **Custom Type Converters Registration**: Added RegisterConverter method to IMapper for custom type conversion functions
- **Improved Nullable Type Handling**: Enhanced ConvertValue method to handle nullable types properly
- **Conditional Mapping**: Added When method to IMappingExpression for conditional property mapping
- **Mapping Transformations**: Added MapWith method for transformation functions during mapping
- **Comprehensive Type Conversions**: Enhanced ConvertValue with DateTime, TimeSpan, and other common type conversions
- **Configuration Validation**: Added ValidateMapping and GetMappingErrors methods to IMapper for early validation
- **Improved Error Messages**: Better debugging information for conversion failures

## Contributing
We welcome contributions! Please see our Contributing Guide for details.
- 🐛 Bug Reports - If you are having problems, please let me know by raising a [new issue](https://github.com/CodeShayk/TurboMapper/issues/new/choose).
- 💡 Feature Requests - Start a [discussion](https://github.com/CodeShayk/TurboMapper/discussions)
- 📝 Documentation - Help improve our [docs](https://github.com/CodeShayk/TurboMapper/wiki)
- 💻 Code - Submit [pull](https://github.com/CodeShayk/TurboMapper/pulls) requests

## License
This project is licensed with the [MIT license](LICENSE).

## Credits
Thank you for reading. Please fork, explore, contribute and report. Happy Coding !! :)
