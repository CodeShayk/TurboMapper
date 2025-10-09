# TurboMapper Implementation Guide

## Overview
TurboMapper is a high-performance object mapping library designed to simplify the process of copying data between objects with similar structures. It provides automatic property mapping, supports complex nested objects, and offers flexible configuration options.

## Core Concepts

### 1. Basic Mapping
The fundamental operation in TurboMapper is mapping properties from a source object to a target object based on property names.

```csharp
var source = new SourceObject { Name = "John", Age = 30 };
var target = mapper.Map<SourceObject, TargetObject>(source);
```

### 2. Complex Type Handling
TurboMapper automatically identifies complex types (custom classes) and handles their mapping recursively.

### 3. Property Matching
Properties are matched by name and write accessibility. Only properties with matching names and compatible types are mapped.

## Architecture

### Core Components

#### Mapper Class
The main entry point for all mapping operations.

**Key Methods:**
- `Map<TSource, TTarget>(TSource source)`: Creates a new target instance and maps all properties
- `ApplyNameBasedMapping<TSource, TTarget>(TSource source, TTarget target)`: Maps properties from source to existing target
- `CreateMap<TSource, TTarget>()`: Configures custom mapping rules

#### Mapping Process

1. **Type Analysis**: Determine source and target property types
2. **Property Matching**: Match properties by name
3. **Value Conversion**: Convert values between compatible types
4. **Recursive Mapping**: Handle nested complex objects
5. **Assignment**: Set values on target properties

### Complex Type Detection

The `IsComplexType(Type type)` method determines whether a type should be treated as a complex object:

```csharp
private bool IsComplexType(Type type)
{
    // Strings are not considered complex types
    if (type == typeof(string))
        return false;
        
    // Value types (int, bool, etc.) are not complex
    if (type.IsValueType)
        return false;
        
    // Arrays are not considered complex for mapping purposes
    if (type.IsArray)
        return false;
        
    // Collections are not considered complex for mapping purposes
    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        return false;
        
    // Everything else is considered a complex type
    return true;
}
```

### Nested Object Mapping

When mapping complex types, TurboMapper handles nested objects through recursive calls:

1. Check if source property value is not null
2. Get or create target property instance
3. Recursively map nested object properties
4. Assign mapped instance to target property

### Value Conversion

The `ConvertValue(object value, Type targetType)` method handles type conversions:

- Null value handling
- Direct assignment when types are compatible
- Enum parsing from strings
- String conversion for all types
- GUID parsing from strings
- Numeric type conversions with special handling for truncation
- Complex type mapping through recursive calls

## Advanced Features

### Custom Mapping Configuration

TurboMapper supports custom mapping configurations through the `MappingModule` system:

```csharp
public class UserMappingModule : MappingModule<UserSource, UserTarget>
{
    public override Action<IMappingExpression<UserSource, UserTarget>> CreateMappings()
    {
        return config => config
            .ForMember(dest => dest.FullName, src => src.FirstName + " " + src.LastName)
            .ForMember(dest => dest.YearsOld, src => src.Age);
    }
}
```

### Property Path Mapping

Support for mapping deeply nested properties using dot notation:

```csharp
config.ForMember(dest => dest.Address.PostalCode, src => src.ZipCode);
```

## Error Handling

### Null Source Handling
When a source object is null, TurboMapper returns the default value for the target type.

### Type Conversion Errors
When type conversion fails, TurboMapper returns the default value for the target type rather than throwing exceptions.

### Circular Reference Prevention
TurboMapper includes built-in protection against infinite recursion in circular object graphs.

## Performance Considerations

### Caching
Mapped property information is cached to avoid repeated reflection calls.

### Memory Efficiency
TurboMapper minimizes object allocation during mapping operations.

### Thread Safety
All mapping operations are thread-safe and can be used concurrently.

## Extension Points

### Custom Value Resolvers
Implement custom logic for calculating property values during mapping.

### Conditional Mapping
Apply mappings only when certain conditions are met.

### Pre/Post Processing
Execute custom code before or after mapping operations.

## Best Practices

### 1. Use Nullable Reference Types
Enable nullable reference types to catch potential null reference issues at compile time.

### 2. Configure Mappings at Startup
Register all mapping modules during application initialization for optimal performance.

### 3. Handle Exceptions Appropriately
While TurboMapper minimizes exceptions, always validate mapped data when business rules require it.

### 4. Profile Performance
For high-volume mapping scenarios, profile your application to ensure optimal performance.

## Common Patterns

### Simple DTO Mapping
Map between similar domain objects and data transfer objects.

### Flattening Objects
Map nested properties to flat target structures.

### Reverse Mapping
Create bidirectional mappings for round-trip data transformations.

### Collection Mapping
Map collections of objects using the same mapping configurations.

## Troubleshooting

### Properties Not Mapping
- Check property names match exactly (case-sensitive)
- Verify properties have public getters/setters
- Ensure target properties are writable
- Confirm property types are compatible or convertible

### Nested Objects Always Null
- Verify complex type detection is working correctly
- Check that source nested objects are not null
- Ensure target object constructors properly initialize nested properties

### Performance Issues
- Review mapping configurations for complexity
- Consider caching mapped objects when appropriate
- Profile reflection usage in hot paths

## Integration

### Dependency Injection
TurboMapper integrates seamlessly with DI containers through extension methods:

```csharp
services.AddTurboMapper();
```

### ASP.NET Core
Use TurboMapper in controllers for view model transformation:

```csharp
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    
    public UserController(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetUser(int id)
    {
        var user = _userRepository.GetById(id);
        var dto = _mapper.Map<User, UserDto>(user);
        return Ok(dto);
    }
}
```

## Testing Strategies

### Unit Testing Mappings
Create dedicated tests for each mapping configuration to ensure correctness.

### Performance Testing
Benchmark mapping operations for critical paths in your application.

### Integration Testing
Test end-to-end scenarios that involve multiple mapping operations.

## Limitations

### Dynamic Properties
TurboMapper does not support mapping to dynamic properties or ExpandoObjects.

### Private Members
Only public properties are considered for mapping.

### Indexers
Properties with indexers are not supported.

## Future Enhancements

### Expression-Based Mapping
Compile mapping expressions for even better performance.

### Convention-Based Mapping
Support configurable naming conventions (camelCase, PascalCase, etc.).

### Validation Integration
Built-in validation during mapping operations.

## API Reference

### IMapper Interface

#### Methods:
- `TTarget Map<TSource, TTarget>(TSource source)`: Primary mapping method
- `void CreateMap<TSource, TTarget>()`: Configure mapping between types

### IMappingExpression Interface

#### Methods:
- `IMappingExpression<TSource, TTarget> ForMember<TMember>(Expression<Func<TTarget, TMember>> destination, Func<TSource, TMember> source)`: Configure property mapping

### MappingModule<TSrc, TDest> Abstract Class

#### Methods:
- `abstract Action<IMappingExpression<TSrc, TDest>> CreateMappings()`: Define custom mapping rules

## Examples

### Basic Mapping
```csharp
public class PersonSource
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

public class PersonTarget
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

// Usage
var source = new PersonSource 
{ 
    FirstName = "John", 
    LastName = "Doe", 
    Age = 30 
};

var target = mapper.Map<PersonSource, PersonTarget>(source);
```

### Nested Object Mapping
```csharp
public class OrderSource
{
    public string OrderId { get; set; }
    public CustomerSource Customer { get; set; }
}

public class CustomerSource
{
    public string Name { get; set; }
    public AddressSource Address { get; set; }
}

public class AddressSource
{
    public string Street { get; set; }
    public string City { get; set; }
}

// Target classes follow the same structure
var source = new OrderSource 
{
    OrderId = "123",
    Customer = new CustomerSource 
    {
        Name = "John Doe",
        Address = new AddressSource 
        {
            Street = "123 Main St",
            City = "Anytown"
        }
    }
};

var target = mapper.Map<OrderSource, OrderTarget>(source);
// target.Customer.Address.Street will be "123 Main St"
```

### Custom Mapping Module
```csharp
public class OrderMappingModule : MappingModule<OrderSource, OrderTarget>
{
    public override Action<IMappingExpression<OrderSource, OrderTarget>> CreateMappings()
    {
        return config => config
            .ForMember(dest => dest.OrderNumber, src => src.OrderId)
            .ForMember(dest => dest.CustomerName, src => src.Customer.Name);
    }
}
```