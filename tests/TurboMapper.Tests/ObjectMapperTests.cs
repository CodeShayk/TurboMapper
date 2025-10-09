using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

        #region Default Name-Based Mapping Tests (No Configuration)

        [Test]
        public void Map_DefaultNaming_SimpleProperties_MapsCorrectly()
        {
            // Arrange
            var source = new SourceModel { Name = "John Doe", Age = 30 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
            Assert.AreEqual(30, result.Age);
        }

        [Test]
        public void Map_DefaultNaming_WithNullSource_ReturnsNull()
        {
            // Arrange
            SourceModel source = null;

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Map_DefaultNaming_WithNullProperties_MapsNullValues()
        {
            // Arrange
            var source = new SourceModel { Name = null, Age = 0 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
            Assert.AreEqual(0, result.Age);
        }

        [Test]
        public void Map_DefaultNaming_SourceWithExtraProperties_IgnoresExtra()
        {
            // Arrange
            var source = new SourceModelWithExtra
            {
                Name = "Test",
                Age = 25,
                ExtraProperty = "Extra Value"
            };

            // Act
            var result = _mapper.Map<SourceModelWithExtra, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
            // ExtraProperty is ignored as target doesn't have it
        }

        [Test]
        public void Map_DefaultNaming_TargetWithExtraProperties_LeavesDefault()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModelWithExtra>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
            Assert.IsNull(result.ExtraProperty); // Not mapped, stays default
        }

        [Test]
        public void Map_DefaultNaming_NestedObjects_MapsRecursively()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "John",
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "Boston",
                    ZipCode = "02101"
                },
                Phone = "555-1234"
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John", result.Name);
            Assert.IsNotNull(result.Address);
            Assert.AreEqual("123 Main St", result.Address.Street);
            Assert.AreEqual("Boston", result.Address.City);
            Assert.AreEqual("02101", result.Address.ZipCode);
        }

        [Test]
        public void Map_DefaultNaming_NullNestedObject_MapsAsNull()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "John",
                Address = null,
                Phone = "555-1234"
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John", result.Name);
            Assert.IsNull(result.Address);
        }

        [Test]
        public void Map_DefaultNaming_DeepNestedObjects_MapsAllLevels()
        {
            // Arrange
            var source = new ComplexSource
            {
                Id = 1,
                User = new User { Name = "Alice", Age = 28 },
                Contact = new Contact { Email = "alice@test.com", Phone = "555-0000" }
            };

            // Act
            var result = _mapper.Map<ComplexSource, ComplexTarget>(source);

            // Assert
            Assert.AreEqual(1, result.Id);
            Assert.IsNotNull(result.User);
            Assert.AreEqual("Alice", result.User.Name);
            Assert.AreEqual(28, result.User.Age);
            Assert.IsNotNull(result.Contact);
            Assert.AreEqual("alice@test.com", result.Contact.Email);
            Assert.AreEqual("555-0000", result.Contact.Phone);
        }

        [Test]
        public void Map_DefaultNaming_TypeConversion_IntToString()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModelWithStringAge>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("25", result.Age);
        }

        [Test]
        public void Map_DefaultNaming_TypeConversion_StringToEnum()
        {
            // Arrange
            var source = new SourceWithEnum { Status = "Active" };

            // Act
            var result = _mapper.Map<SourceWithEnum, TargetWithEnum>(source);

            // Assert
            Assert.AreEqual(StatusEnum.Active, result.Status);
        }

        [Test]
        public void Map_DefaultNaming_ReadOnlyProperty_Skipped()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModelWithReadOnly>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
            Assert.IsNull(result.ReadOnlyProperty);
        }

        #endregion Default Name-Based Mapping Tests (No Configuration)

        #region Explicit Mapping Configuration Tests (Without Modules)

        [Test]
        public void Map_ExplicitConfig_DifferentPropertyNames_MapsCorrectly()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                },
                new PropertyMapping
                {
                    SourcePropertyPath = "Age",
                    TargetPropertyPath = "Years"
                }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "John", Age = 30 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("John", result.FullName);
            Assert.AreEqual(30, result.Years);
        }

        [Test]
        public void Map_ExplicitConfig_PartialMapping_RestUsesDefaultNaming()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "FirstName",
                    TargetPropertyPath = "FullName"
                }
            };
            _mapper.CreateMap<UserWithExtraSource, UserWithExtraTarget>(mappings);
            var source = new UserWithExtraSource
            {
                FirstName = "John",
                LastName = "Doe",
                Age = 30,
                Email = "john@test.com"
            };

            // Act
            var result = _mapper.Map<UserWithExtraSource, UserWithExtraTarget>(source);

            // Assert
            Assert.AreEqual("John", result.FullName); // Explicit mapping
            Assert.AreEqual("Doe", result.LastName); // Default naming
            Assert.AreEqual("john@test.com", result.Email); // Default naming
        }

        [Test]
        public void Map_ExplicitConfig_NestedProperty_FlattensStructure()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Street"
                },
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.City",
                    TargetPropertyPath = "City"
                }
            };
            _mapper.CreateMap<SourceWithNested, FlatTarget>(mappings);
            var source = new SourceWithNested
            {
                Name = "John",
                Address = new Address { Street = "123 Main", City = "Boston" }
            };

            // Act
            var result = _mapper.Map<SourceWithNested, FlatTarget>(source);

            // Assert
            Assert.AreEqual("John", result.Name); // Default naming
            Assert.AreEqual("123 Main", result.Street); // Explicit from nested
            Assert.AreEqual("Boston", result.City); // Explicit from nested
        }

        [Test]
        public void Map_ExplicitConfig_NestedPropertyMapping_CreatesNestedObject()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Address.StreetName"
                },
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.City",
                    TargetPropertyPath = "Address.Location"
                }
            };
            _mapper.CreateMap<SourceWithNested, TargetWithNestedConfig>(mappings);
            var source = new SourceWithNested
            {
                Name = "John",
                Address = new Address
                {
                    Street = "456 Oak",
                    City = "Seattle",
                    ZipCode = "98101"
                },
                Phone = "555-5678"
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            // Assert
            Assert.IsNotNull(result.Address);
            Assert.AreEqual("456 Oak", result.Address.StreetName); // Explicit nested
            Assert.AreEqual("Seattle", result.Address.Location); // Explicit nested
            Assert.AreEqual("98101", result.Address.ZipCode); // Default naming
            Assert.AreEqual("John", result.Name); // Default naming
            Assert.AreEqual("555-5678", result.Phone); // Default naming
        }

        [Test]
        public void Map_ExplicitConfig_NullNestedSource_HandlesGracefully()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Address.StreetName"
                }
            };
            _mapper.CreateMap<SourceWithNested, TargetWithNestedConfig>(mappings);
            var source = new SourceWithNested
            {
                Name = "John",
                Address = null
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            // Assert
            Assert.AreEqual("John", result.Name);
            Assert.IsNull(result.Address);
        }

        [Test]
        public void Map_ExplicitConfig_InvalidSourcePath_SetsTargetToNull()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "NonExistent.Property",
                    TargetPropertyPath = "Name"
                }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result.Name);
            Assert.AreEqual(25, result.Age); // Default naming still works
        }

        [Test]
        public void Map_ExplicitConfig_EmptyMappingList_UsesDefaultNaming()
        {
            // Arrange
            _mapper.CreateMap<SourceModel, TargetModel>(new List<PropertyMapping>());
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
        }

        [Test]
        public void Map_ExplicitConfig_NullMappingList_UsesDefaultNaming()
        {
            // Arrange
            _mapper.CreateMap<SourceModel, TargetModel>(null);
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
        }

        #endregion Explicit Mapping Configuration Tests (Without Modules)

        #region Mapping Module Tests (With Modules)

        [Test]
        public void Map_WithMappingModule_ExplicitAndDefault_BothApply()
        {
            // Arrange
            var module = new UserMappingModule();
            ((IMappingModule)module).CreateMap(_mapper);
            var source = new UserSource
            {
                FirstName = "Jane",
                Age = 28,
                Email = "jane@test.com"
            };

            // Act
            var result = _mapper.Map<UserSource, UserTarget>(source);

            // Assert
            Assert.AreEqual("Jane", result.Name); // Explicit: FirstName -> Name
            Assert.AreEqual(28, result.Years); // Explicit: Age -> Years
            Assert.AreEqual("jane@test.com", result.Email); // Default naming
        }

        [Test, Ignore("DI with test")]
        public void Map_WithMappingModule_NestedMapping_WorksCorrectly()
        {
            // Arrange
            var module = new UserWithAddressMappingModule();
            ((IMappingModule)module).CreateMap(_mapper);
            var source = new UserWithAddressSource
            {
                Name = "Bob",
                Address = new Address
                {
                    Street = "789 Pine St",
                    City = "Portland",
                    ZipCode = "97201"
                },
                Phone = "555-9999"
            };

            // Act
            var result = _mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source);

            // Assert
            Assert.AreEqual("Bob", result.Name);
            Assert.IsNotNull(result.Address);
            Assert.AreEqual("789 Pine St", result.Address.Street); // Explicit
            Assert.AreEqual("Portland", result.Address.Location); // Explicit: City -> Location
            Assert.AreEqual("97201", result.Address.ZipCode); // Default naming
            Assert.AreEqual("555-9999", result.Phone); // Default naming
        }

        [Test]
        public void Map_WithMappingModule_MultipleProperties_AllMapped()
        {
            // Arrange
            var module = new UserWithExtraPropertiesMappingModule();
            ((IMappingModule)module).CreateMap(_mapper);
            var source = new UserWithExtraSource
            {
                FirstName = "Alice",
                LastName = "Smith",
                Age = 32,
                Email = "alice@test.com"
            };

            // Act
            var result = _mapper.Map<UserWithExtraSource, UserWithExtraTarget>(source);

            // Assert
            Assert.AreEqual("Alice", result.FullName); // Explicit: FirstName -> FullName
            Assert.AreEqual(32, result.AgeInYears); // Explicit: Age -> AgeInYears
            Assert.AreEqual("Smith", result.LastName); // Default naming
            Assert.AreEqual("alice@test.com", result.Email); // Default naming
        }

        [Test]
        public void Map_WithMappingModule_DisableDefaultMapping_OnlyExplicitMapped()
        {
            // Arrange
            var module = new UserMappingModuleNoDefault();
            ((IMappingModule)module).CreateMap(_mapper);
            var source = new UserSource
            {
                FirstName = "Charlie",
                Age = 40,
                Email = "charlie@test.com"
            };

            // Act
            var result = _mapper.Map<UserSource, UserTarget>(source);

            // Assert
            Assert.AreEqual("Charlie", result.Name); // Explicit
            Assert.AreEqual(40, result.Years); // Explicit
            Assert.IsNull(result.Email); // Not mapped (default disabled)
        }

        [Test]
        public void Map_WithMappingModule_NullSource_ReturnsNull()
        {
            // Arrange
            var module = new UserMappingModule();
            ((IMappingModule)module).CreateMap(_mapper);
            UserSource source = null;

            // Act
            var result = _mapper.Map<UserSource, UserTarget>(source);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Map_WithMappingModule_NullNestedObject_HandlesGracefully()
        {
            // Arrange
            var module = new UserWithAddressMappingModule();
            ((IMappingModule)module).CreateMap(_mapper);
            var source = new UserWithAddressSource
            {
                Name = "David",
                Address = null,
                Phone = "555-0001"
            };

            // Act
            var result = _mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source);

            // Assert
            Assert.AreEqual("David", result.Name);
            Assert.IsNull(result.Address);
            Assert.AreEqual("555-0001", result.Phone);
        }

        #endregion Mapping Module Tests (With Modules)

        #region Dependency Injection Tests

        [Test]
        public void DI_Registration_RegistersMapperAsSingleton()
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
            Assert.AreSame(mapper1, mapper2);
        }

        [Test, Ignore("DI with test")]
        public void DI_Registration_AutomaticallyRegistersMappingModules()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();
            var source = new UserSource
            {
                FirstName = "DI Test",
                Age = 25,
                Email = "di@test.com"
            };
            var result = mapper.Map<UserSource, UserTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("DI Test", result.Name);
            Assert.AreEqual(25, result.Years);
            Assert.AreEqual("di@test.com", result.Email);
        }

        [Test]
        public void DI_Registration_WorksWithScopedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            using (var scope1 = serviceProvider.CreateScope())
            using (var scope2 = serviceProvider.CreateScope())
            {
                var mapper1 = scope1.ServiceProvider.GetService<IMapper>();
                var mapper2 = scope2.ServiceProvider.GetService<IMapper>();

                // Assert
                Assert.IsNotNull(mapper1);
                Assert.IsNotNull(mapper2);
                Assert.AreSame(mapper1, mapper2); // Singleton across scopes
            }
        }

        #endregion Dependency Injection Tests

        #region Multiple Mapping Tests

        [Test]
        public void Map_MultipleMappingConfigurations_EachWorksIndependently()
        {
            // Arrange
            var mappings1 = new List<PropertyMapping>
            {
                new PropertyMapping { SourcePropertyPath = "Name", TargetPropertyPath = "FullName" }
            };
            var mappings2 = new List<PropertyMapping>
            {
                new PropertyMapping { SourcePropertyPath = "FirstName", TargetPropertyPath = "FullName" }
            };

            _mapper.CreateMap<SourceModel, TargetModel>(mappings1);
            _mapper.CreateMap<UserWithExtraSource, UserWithExtraTarget>(mappings2);

            var source1 = new SourceModel { Name = "Test1", Age = 25 };
            var source2 = new UserWithExtraSource { FirstName = "Test2", Age = 30 };

            // Act
            var result1 = _mapper.Map<SourceModel, TargetModel>(source1);
            var result2 = _mapper.Map<UserWithExtraSource, UserWithExtraTarget>(source2);

            // Assert
            Assert.AreEqual("Test1", result1.FullName);
            Assert.AreEqual(25, result1.Age);
            Assert.AreEqual("Test2", result2.FullName);
        }

        [Test]
        public void Map_OverwriteExistingConfiguration_UsesLatestConfiguration()
        {
            // Arrange
            var mappings1 = new List<PropertyMapping>
            {
                new PropertyMapping { SourcePropertyPath = "Name", TargetPropertyPath = "FullName" }
            };
            var mappings2 = new List<PropertyMapping>
            {
                new PropertyMapping { SourcePropertyPath = "Name", TargetPropertyPath = "Name" }
            };

            _mapper.CreateMap<SourceModel, TargetModel>(mappings1);
            _mapper.CreateMap<SourceModel, TargetModel>(mappings2);

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.IsNull(result.FullName);
        }

        #endregion Multiple Mapping Tests
    }

    #region Test Models

    public class SourceModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class TargetModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? FullName { get; set; }
        public int Years { get; set; }
    }

    public class SourceModelWithExtra
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ExtraProperty { get; set; }
    }

    public class TargetModelWithExtra
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? ExtraProperty { get; set; }
    }

    public class TargetModelWithStringAge
    {
        public string? Name { get; set; }
        public string? Age { get; set; }
    }

    public class TargetModelWithReadOnly
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? ReadOnlyProperty { get; private set; }
    }

    public class SourceWithEnum
    {
        public string? Status { get; set; }
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

    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? ZipCode { get; set; }
    }

    public class SourceWithNested
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
        public string? Phone { get; set; }
    }

    public class TargetWithNested
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
    }

    public class TargetWithNestedConfig
    {
        public string? Name { get; set; }
        public AddressConfig? Address { get; set; }
        public string? Phone { get; set; }
    }

    public class AddressConfig
    {
        public string? StreetName { get; set; }
        public string? Location { get; set; }
        public string? ZipCode { get; set; }
    }

    public class FlatTarget
    {
        public string? Name { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
    }

    public class User
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class Contact
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class ComplexSource
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public Contact? Contact { get; set; }
    }

    public class ComplexTarget
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public Contact? Contact { get; set; }
    }

    public class UserSource
    {
        public string? FirstName { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    public class UserTarget
    {
        public string? Name { get; set; }
        public int Years { get; set; }
        public string? Email { get; set; }
    }

    public class UserWithExtraSource
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    public class UserWithExtraTarget
    {
        public string? FullName { get; set; }
        public string? LastName { get; set; }
        public int AgeInYears { get; set; }
        public string? Email { get; set; }
    }

    public class UserWithAddressSource
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
        public string? Phone { get; set; }
    }

    public class UserWithAddressTarget
    {
        public string? Name { get; set; }
        public AddressWithConfig? Address { get; set; }
        public string? Phone { get; set; }
    }

    public class AddressWithConfig
    {
        public string? Street { get; set; }
        public string? Location { get; set; }
        public string? ZipCode { get; set; }
    }

    #endregion Test Models

    #region Mapping Modules

    internal class UserMappingModule : MappingModule<UserSource, UserTarget>
    {
        public UserMappingModule() : base(enableDefaultMapping: true)
        {
        }

        public override Action<IMappingExpression<UserSource, UserTarget>> CreateMappings()
        {
            return config =>
            {
                config.ForMember(dest => dest.Name, src => src.FirstName)
                      .ForMember(dest => dest.Years, src => src.Age);
            };
        }
    }

    internal class UserMappingModuleNoDefault : MappingModule<UserSource, UserTarget>
    {
        public UserMappingModuleNoDefault() : base(enableDefaultMapping: false)
        {
        }

        public override Action<IMappingExpression<UserSource, UserTarget>> CreateMappings()
        {
            return config =>
            {
                config.ForMember(dest => dest.Name, src => src.FirstName)
                      .ForMember(dest => dest.Years, src => src.Age);
            };
        }
    }

    internal class UserWithAddressMappingModule : MappingModule<UserWithAddressSource, UserWithAddressTarget>
    {
        public UserWithAddressMappingModule() : base(enableDefaultMapping: true)
        {
        }

        public override Action<IMappingExpression<UserWithAddressSource, UserWithAddressTarget>> CreateMappings()
        {
            return config =>
            {
                config.ForMember(dest => dest.Name, src => src.Name)
                      .ForMember(dest => dest.Address.Street, src => src.Address.Street)
                      .ForMember(dest => dest.Address.Location, src => src.Address.City);
            };
        }
    }

    public class UserWithExtraPropertiesMappingModule : MappingModule<UserWithExtraSource, UserWithExtraTarget>
    {
        public UserWithExtraPropertiesMappingModule() : base(enableDefaultMapping: true)
        {
        }

        public override Action<IMappingExpression<UserWithExtraSource, UserWithExtraTarget>> CreateMappings()
        {
            return config =>
            {
                config.ForMember(dest => dest.FullName, src => src.FirstName)
                      .ForMember(dest => dest.AgeInYears, src => src.Age);
            };
        }
    }

    #endregion Mapping Modules
}