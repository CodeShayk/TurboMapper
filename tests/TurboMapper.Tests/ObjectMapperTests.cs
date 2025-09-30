using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TurboMapper.Impl;

namespace TurboMapper.Tests
{
    [TestFixture]
    public class ObjectMapperTests
    {
        private Mapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new Mapper();
        }

        [Test]
        public void Map_NameBased_SameProperties()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
        }

        [Test]
        public void Map_NameBased_DifferentProperties()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var config = new List<PropertyMapping>
            {
                new PropertyMapping { SourceProperty = "Name", TargetProperty = "FullName" },
                new PropertyMapping { SourceProperty = "Age", TargetProperty = "Years" }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(config);

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.FullName);
            Assert.AreEqual(25, result.Years);
        }

        [Test]
        public void Map_NameBased_NullSource()
        {
            // Arrange
            SourceModel source = null;

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Map_NameBased_IncompatibleTypes()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var config = new List<PropertyMapping>
            {
                new PropertyMapping { SourceProperty = "Age", TargetProperty = "Name" }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(config);

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("25", result.Name); // Converted to string
        }

        [Test]
        public void Map_NameBased_EnumConversion()
        {
            // Arrange
            var source = new SourceWithEnum { Status = "Active" };
            var config = new List<PropertyMapping>
            {
                new PropertyMapping { SourceProperty = "Status", TargetProperty = "Status" }
            };
            _mapper.CreateMap<SourceWithEnum, TargetWithEnum>(config);

            // Act
            var result = _mapper.Map<SourceWithEnum, TargetWithEnum>(source);

            // Assert
            Assert.AreEqual(StatusEnum.Active, result.Status);
        }

        [Test]
        public void Map_NameBased_MissingProperty()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var config = new List<PropertyMapping>
            {
                new PropertyMapping { SourceProperty = "NonExistent", TargetProperty = "Name" }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(config);

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result.Name);
        }

        [Test]
        public void Map_Nested_NameBased()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City" }
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("123 Main St", result.Address.Street);
            Assert.AreEqual("Test City", result.Address.City);
        }

        [Test]
        public void Map_Nested_ConfigurationBased()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City" }
            };

            var addressMappings = new List<PropertyMapping>
            {
                new PropertyMapping { SourceProperty = "Street", TargetProperty = "StreetName" },
                new PropertyMapping { SourceProperty = "City", TargetProperty = "Location" }
            };

            var config = new List<PropertyMapping>
            {
                new PropertyMapping { SourceProperty = "Name", TargetProperty = "Name" },
                new PropertyMapping
                {
                    SourceProperty = "Address",
                    TargetProperty = "Address",
                    IsNested = true,
                    NestedMappings = addressMappings
                }
            };

            _mapper.CreateMap<SourceWithNested, TargetWithNestedConfig>(config);

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("123 Main St", result.Address.StreetName);
            Assert.AreEqual("Test City", result.Address.Location);
        }

        [Test]
        public void Map_Nested_NullNestedObject()
        {
            // Arrange
            var source = new SourceWithNested { Name = "Test", Address = null };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.IsNull(result.Address);
        }

        [Test]
        public void Map_Nested_ComplexTypes()
        {
            // Arrange
            var source = new ComplexSource
            {
                Id = 1,
                User = new User { Name = "John", Age = 30 },
                Contact = new Contact { Email = "john@example.com", Phone = "123-456-7890" }
            };

            // Act
            var result = _mapper.Map<ComplexSource, ComplexTarget>(source);

            // Assert
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("John", result.User.Name);
            Assert.AreEqual(30, result.User.Age);
            Assert.AreEqual("john@example.com", result.Contact.Email);
            Assert.AreEqual("123-456-7890", result.Contact.Phone);
        }

        [Test]
        public void DI_Registration_SingleAssembly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            // Assert
            Assert.IsNotNull(mapper);
        }

        [Test]
        public void DI_SingletonRegistration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mapper1 = serviceProvider.GetService<IMapper>();
            var mapper2 = serviceProvider.GetService<IMapper>();

            // Assert
            Assert.IsNotNull(mapper1);
            Assert.IsNotNull(mapper2);
            Assert.AreSame(mapper1, mapper2); // Singleton verification
        }

        [Test]
        public void LambdaMapping_SimpleProperty()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModel>();
            expression.ForMember(dest => dest.FullName, src => src.Name);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.FullName);
        }

        [Test]
        public void LambdaMapping_NestedProperty()
        {
            // Arrange
            var expression = new MappingExpression<SourceWithNested, TargetWithNestedConfig>();
            expression.ForMember(dest => dest.Name, src => src.Name)
                     .ForMember(dest => dest.Address.StreetName, src => src.Address.Street)
                     .ForMember(dest => dest.Address.Location, src => src.Address.City);

            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City", ZipCode = "12345" },
                Phone = "555-1234"
            };

            // Act
            _mapper.CreateMap<SourceWithNested, TargetWithNestedConfig>(expression.Mappings);
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("123 Main St", result.Address.StreetName);
            Assert.AreEqual("Test City", result.Address.Location);
            Assert.AreEqual("12345", result.Address.ZipCode); // Mapped by default naming convention
            Assert.AreEqual("555-1234", result.Phone); // Mapped by default naming convention
        }

        [Test]
        public void LambdaMapping_NestedProperty_WithNullSource()
        {
            // Arrange
            var expression = new MappingExpression<SourceWithNested, TargetWithNestedConfig>();
            expression.ForMember(dest => dest.Name, src => src.Name)
                     .ForMember(dest => dest.Address.StreetName, src => src.Address.Street)
                     .ForMember(dest => dest.Address.Location, src => src.Address.City);

            var source = new SourceWithNested
            {
                Name = "Test",
                Address = null
            };

            // Act
            _mapper.CreateMap<SourceWithNested, TargetWithNestedConfig>(expression.Mappings);
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.IsNull(result.Address);
        }

        [Test]
        public void LambdaMapping_NestedProperty_Creation()
        {
            // Arrange
            var expression = new MappingExpression<SourceWithNested, TargetWithNestedConfig>();
            expression.ForMember(dest => dest.Name, src => src.Name)
                     .ForMember(dest => dest.Address.StreetName, src => src.Address.Street)
                     .ForMember(dest => dest.Address.Location, src => src.Address.City);

            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City" }
            };

            // Act
            _mapper.CreateMap<SourceWithNested, TargetWithNestedConfig>(expression.Mappings);
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            // Assert
            Assert.IsNotNull(result.Address); // Object created automatically
            Assert.AreEqual("123 Main St", result.Address.StreetName);
            Assert.AreEqual("Test City", result.Address.Location);
        }

        [Test]
        public void DefaultMapping_WithPartialMapping()
        {
            // Arrange
            var expression = new MappingExpression<UserWithExtraSource, UserWithExtraTarget>();
            expression.ForMember(dest => dest.FullName, src => src.FirstName)
                     .ForMember(dest => dest.AgeInYears, src => src.Age);

            var source = new UserWithExtraSource
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30,
                Email = "john@example.com"
            };

            // Act
            _mapper.CreateMap<UserWithExtraSource, UserWithExtraTarget>(expression.Mappings);
            var result = _mapper.Map<UserWithExtraSource, UserWithExtraTarget>(source);

            // Assert
            Assert.AreEqual("John", result.FullName); // Mapped explicitly
            Assert.AreEqual("Doe", result.LastName); // Mapped by default naming convention
            Assert.AreEqual(30, result.AgeInYears); // Mapped explicitly
            Assert.AreEqual("john@example.com", result.Email); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithNoExplicitMapping()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModel>();

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped by default naming convention
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithNestedObject()
        {
            // Arrange
            var expression = new MappingExpression<UserWithAddressSource, UserWithAddressTarget>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new UserWithAddressSource
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City", ZipCode = "12345" },
                Phone = "555-1234"
            };

            // Act
            _mapper.CreateMap<UserWithAddressSource, UserWithAddressTarget>(expression.Mappings);
            var result = _mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual("123 Main St", result.Address.Street); // Mapped by default naming convention
            Assert.AreEqual("Test City", result.Address.Location); // Mapped by default naming convention
            Assert.AreEqual("12345", result.Address.ZipCode); // Mapped by default naming convention
            Assert.AreEqual("555-1234", result.Phone); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithIncompatibleTypes()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModelWithDifferentTypes>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModelWithDifferentTypes>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModelWithDifferentTypes>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
            Assert.AreEqual(25, result.Years); // Mapped by default naming convention (int to int)
        }

        [Test]
        public void DefaultMapping_WithIncompatibleTypeConversion()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModelWithStringAge>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModelWithStringAge>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModelWithStringAge>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual("25", result.Age); // Mapped by default naming convention with conversion
        }

        [Test]
        public void DefaultMapping_WithMissingTargetProperty()
        {
            // Arrange
            var expression = new MappingExpression<SourceModelWithExtra, TargetModel>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new SourceModelWithExtra { Name = "Test", Age = 25, ExtraProperty = "Extra" };

            // Act
            _mapper.CreateMap<SourceModelWithExtra, TargetModel>(expression.Mappings);
            var result = _mapper.Map<SourceModelWithExtra, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
                                             // ExtraProperty should be ignored as there's no matching property in target
        }

        [Test]
        public void DefaultMapping_WithMissingSourceProperty()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModelWithExtra>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModelWithExtra>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModelWithExtra>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
            Assert.IsNull(result.ExtraProperty); // Target property exists but no source property to map
        }

        [Test]
        public void DefaultMapping_WithReadOnlyTargetProperty()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModelWithReadOnly>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModelWithReadOnly>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModelWithReadOnly>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
                                             // ReadOnlyProperty should not be set as it's read-only
        }

        [Test]
        public void DefaultMapping_WithNullSourceValue()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModel>();
            expression.ForMember(dest => dest.FullName, src => src.Name);

            var source = new SourceModel { Name = null, Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result.FullName); // Mapped explicitly (null value)
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithNullSourceNested()
        {
            // Arrange
            var expression = new MappingExpression<SourceWithNested, TargetWithNested>();
            expression.ForMember(dest => dest.Name, src => src.Name);

            var source = new SourceWithNested { Name = "Test", Address = null };

            // Act
            _mapper.CreateMap<SourceWithNested, TargetWithNested>(expression.Mappings);
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.IsNull(result.Address); // Mapped by default naming convention (null value)
        }

        [Test]
        public void DefaultMapping_WithNoCustomMappings()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModel>();

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped by default naming convention
            Assert.AreEqual(25, result.Age); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithAllPropertiesMappedExplicitly()
        {
            // Arrange
            var expression = new MappingExpression<SourceModel, TargetModel>();
            expression.ForMember(dest => dest.Name, src => src.Name)
                     .ForMember(dest => dest.Age, src => src.Age);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(expression.Mappings);
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped explicitly
            Assert.AreEqual(25, result.Age); // Mapped explicitly
                                             // No default mapping should occur as all properties are explicitly mapped
        }

        [Test]
        public void DefaultMapping_WithMixedCaseProperties()
        {
            // Arrange
            var expression = new MappingExpression<SourceModelWithMixedCase, TargetModelWithMixedCase>();

            var source = new SourceModelWithMixedCase { FirstName = "John", LastName = "Doe" };

            // Act
            _mapper.CreateMap<SourceModelWithMixedCase, TargetModelWithMixedCase>(expression.Mappings);
            var result = _mapper.Map<SourceModelWithMixedCase, TargetModelWithMixedCase>(source);

            // Assert
            Assert.AreEqual("John", result.FirstName); // Mapped by default naming convention
            Assert.AreEqual("Doe", result.LastName); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithEnumConversion()
        {
            // Arrange
            var expression = new MappingExpression<SourceWithEnum, TargetWithEnum>();

            var source = new SourceWithEnum { Status = "Active" };

            // Act
            _mapper.CreateMap<SourceWithEnum, TargetWithEnum>(expression.Mappings);
            var result = _mapper.Map<SourceWithEnum, TargetWithEnum>(source);

            // Assert
            Assert.AreEqual(StatusEnum.Active, result.Status); // Mapped by default naming convention with enum conversion
        }

        [Test]
        public void DefaultMapping_WithComplexTypes()
        {
            // Arrange
            var expression = new MappingExpression<ComplexSource, ComplexTarget>();

            var source = new ComplexSource
            {
                Id = 1,
                User = new User { Name = "John", Age = 30 },
                Contact = new Contact { Email = "john@example.com", Phone = "123-456-7890" }
            };

            // Act
            _mapper.CreateMap<ComplexSource, ComplexTarget>(expression.Mappings);
            var result = _mapper.Map<ComplexSource, ComplexTarget>(source);

            // Assert
            Assert.AreEqual(1, result.Id); // Mapped by default naming convention
            Assert.AreEqual("John", result.User.Name); // Mapped by default naming convention
            Assert.AreEqual(30, result.User.Age); // Mapped by default naming convention
            Assert.AreEqual("john@example.com", result.Contact.Email); // Mapped by default naming convention
            Assert.AreEqual("123-456-7890", result.Contact.Phone); // Mapped by default naming convention
        }

        [Test]
        public void DefaultMapping_WithObjectGraphLambdaAndDefault()
        {
            // Arrange
            var expression = new MappingExpression<UserWithAddressSource, UserWithAddressTarget>();
            expression.ForMember(dest => dest.Address.Street, src => src.Address.Street);

            var source = new UserWithAddressSource
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City", ZipCode = "12345" },
                Phone = "555-1234"
            };

            // Act
            _mapper.CreateMap<UserWithAddressSource, UserWithAddressTarget>(expression.Mappings);
            var result = _mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source);

            // Assert
            Assert.AreEqual("Test", result.Name); // Mapped by default naming convention
            Assert.AreEqual("123 Main St", result.Address.Street); // Mapped explicitly with object graph lambda
            Assert.AreEqual("Test City", result.Address.Location); // Mapped by default naming convention
            Assert.AreEqual("12345", result.Address.ZipCode); // Mapped by default naming convention
            Assert.AreEqual("555-1234", result.Phone); // Mapped by default naming convention
        }

        [Test]
        public void DI_Registration_AppDomain()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            // Assert
            Assert.IsNotNull(mapper);
        }
    }

    // Test Models
    public class SourceModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class TargetModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string FullName { get; set; }
        public int Years { get; set; }
    }

    public class SourceWithEnum
    {
        public string Status { get; set; }
    }

    public class TargetWithEnum
    {
        public StatusEnum Status { get; set; }
    }

    public enum StatusEnum
    {
        Active,
        Inactive
    }

    // Nested Models
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    public class SourceWithNested
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Phone { get; set; }
    }

    public class TargetWithNested
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class TargetWithNestedConfig
    {
        public string Name { get; set; }
        public AddressConfig Address { get; set; }
        public string Phone { get; set; }
    }

    public class AddressConfig
    {
        public string StreetName { get; set; }
        public string Location { get; set; }
        public string ZipCode { get; set; }
    }

    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class ComplexSource
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Contact Contact { get; set; }
    }

    public class ComplexTarget
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Contact Contact { get; set; }
    }

    // Additional models for extended testing
    public class UserWithExtraSource
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
    }

    public class UserWithExtraTarget
    {
        public string FullName { get; set; }
        public string LastName { get; set; }
        public int AgeInYears { get; set; }
        public string Email { get; set; }
    }

    public class TargetModelWithDifferentTypes
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Years { get; set; }
    }

    public class TargetModelWithStringAge
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }

    public class SourceModelWithExtra
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ExtraProperty { get; set; }
    }

    public class TargetModelWithExtra
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ExtraProperty { get; set; }
    }

    public class TargetModelWithReadOnly
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ReadOnlyProperty { get; private set; }
    }

    public class SourceModelWithMixedCase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class TargetModelWithMixedCase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}