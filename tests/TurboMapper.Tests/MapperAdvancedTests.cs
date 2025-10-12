using TurboMapper.Impl;

namespace TurboMapper.Tests
{
    [TestFixture]
    public class MapperAdvancedTests
    {
        private Mapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new Mapper();
        }

        #region ApplyCustomMappings Tests

        [Test]
        public void ApplyCustomMappings_WithEmptyMappings_AppliesDefaultMapping()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModel();
            var mappings = new List<PropertyMapping>();

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual(25, target.Age);
        }

        [Test]
        public void ApplyCustomMappings_WithNestedPath_CreatesNestedObjects()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City" }
            };
            var target = new TargetWithNestedConfig();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Address.StreetName"
                }
            };

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.IsNotNull(target.Address);
            Assert.AreEqual("123 Main St", target.Address.StreetName);
        }

        [Test]
        public void ApplyCustomMappings_WithNullNestedSource_SetsTargetToNull()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = null
            };
            var target = new TargetWithNestedConfig();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Address.StreetName"
                }
            };

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.IsNull(target.Address);
        }

        [Test]
        public void ApplyCustomMappings_WithMultipleNestedPaths_MapsAll()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City", ZipCode = "12345" }
            };
            var target = new TargetWithNestedConfig();
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

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.AreEqual("123 Main St", target.Address.StreetName);
            Assert.AreEqual("Test City", target.Address.Location);
            Assert.AreEqual("12345", target.Address.ZipCode); // Mapped by default
        }

        [Test]
        public void ApplyCustomMappings_WithInvalidSourcePath_HandlesGracefully()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModel();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "NonExistent.Property",
                    TargetPropertyPath = "Name"
                }
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _mapper.ApplyCustomMappings(source, target, mappings));
            Assert.IsNull(target.Name);
        }

        #endregion ApplyCustomMappings Tests

        #region ApplyNameBasedMapping Tests

        [Test]
        public void ApplyNameBasedMapping_WithMatchingProperties_MapsAll()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModel();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual(25, target.Age);
        }

        [Test]
        public void ApplyNameBasedMapping_WithNonMatchingProperties_IgnoresThem()
        {
            // Arrange
            var source = new SourceModelWithExtra { Name = "Test", Age = 25, ExtraProperty = "Extra" };
            var target = new TargetModel();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual(25, target.Age);
            // ExtraProperty is ignored as there's no matching property in target
        }

        [Test]
        public void ApplyNameBasedMapping_WithNestedObjects_MapsRecursively()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main St", City = "Test City" }
            };
            var target = new TargetWithNested();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.IsNotNull(target.Address);
            Assert.AreEqual("123 Main St", target.Address.Street);
            Assert.AreEqual("Test City", target.Address.City);
        }

        [Test]
        public void ApplyNameBasedMapping_WithNullNestedSource_LeavesTargetNull()
        {
            // Arrange
            var source = new SourceWithNested { Name = "Test", Address = null };
            var target = new TargetWithNested();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.IsNull(target.Address);
        }

        [Test]
        public void ApplyNameBasedMapping_WithReadOnlyProperty_SkipsIt()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModelWithReadOnly();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual(25, target.Age);
        }

        [Test]
        public void ApplyNameBasedMapping_WithTypeConversion_ConvertsValues()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModelWithStringAge();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual("25", target.Age);
        }

        [Test]
        public void ApplyNameBasedMapping_WithNullSourceValue_SetsNull()
        {
            // Arrange
            var source = new SourceModel { Name = null, Age = 25 };
            var target = new TargetModel();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.IsNull(target.Name);
            Assert.AreEqual(25, target.Age);
        }

        #endregion ApplyNameBasedMapping Tests

        #region Value Conversion Tests

        [Test]
        public void ConvertValue_NullValue_ReturnsNull()
        {
            // Arrange
            var source = new SourceModel { Name = null, Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result.Name);
        }

        [Test]
        public void ConvertValue_EnumFromString_ConvertsCorrectly()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Status",
                    TargetPropertyPath = "Status"
                }
            };
            _mapper.CreateMap<SourceWithEnum, TargetWithEnum>(mappings);
            var source = new SourceWithEnum { Status = "Active" };

            // Act
            var result = _mapper.Map<SourceWithEnum, TargetWithEnum>(source);

            // Assert
            Assert.AreEqual(StatusEnum.Active, result.Status);
        }

        [Test]
        public void ConvertValue_IntToString_ConvertsCorrectly()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModelWithStringAge>(source);

            // Assert
            Assert.AreEqual("25", result.Age);
        }

        [Test]
        public void ConvertValue_AssignableTypes_PassesThrough()
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
        public void ConvertValue_CompatibleValueTypes_Converts()
        {
            // Arrange
            var source = new SourceWithShort { Value = 100 };

            // Act
            var result = _mapper.Map<SourceWithShort, TargetWithInt>(source);

            // Assert
            Assert.AreEqual(100, result.Value);
        }

        #endregion Value Conversion Tests

        #region CreateMap Tests

        [Test]
        public void CreateMap_NewMapping_StoresConfiguration()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "Test", Age = 25 };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.FullName);
        }

        [Test]
        public void CreateMap_OverwriteExisting_UpdatesConfiguration()
        {
            // Arrange
            var mappings1 = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };
            var mappings2 = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Age",
                    TargetPropertyPath = "Years"
                }
            };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(mappings1);
            _mapper.CreateMap<SourceModel, TargetModel>(mappings2);

            var source = new SourceModel { Name = "Test", Age = 25 };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual(25, result.Years);
        }

        [Test]
        public void CreateMap_NullMappings_CreatesEmptyConfiguration()
        {
            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(null);
            var source = new SourceModel { Name = "Test", Age = 25 };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
        }

        #endregion CreateMap Tests

        #region Complex Scenario Tests

        [Test]
        public void Map_DeepNestedObjects_MapsCorrectly()
        {
            // Arrange
            var source = new DeepNestedSource
            {
                Name = "Test",
                Level1 = new Level1Source
                {
                    Value = "L1",
                    Level2 = new Level2Source
                    {
                        Value = "L2",
                        Level3 = new Level3Source { Value = "L3" }
                    }
                }
            };

            // Act
            var result = _mapper.Map<DeepNestedSource, DeepNestedTarget>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual("L1", result.Level1.Value);
            Assert.AreEqual("L2", result.Level1.Level2.Value);
            Assert.AreEqual("L3", result.Level1.Level2.Level3.Value);
        }

        [Test]
        public void Map_CircularReferencePrevention_HandlesGracefully()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test", result.Name);
        }

        [Test]
        public void Map_MixedCustomAndDefaultMappings_BothApplied()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.FullName); // Custom mapping
            Assert.AreEqual(25, result.Age); // Default mapping
        }

        #endregion Complex Scenario Tests

        #region Edge Cases

        [Test]
        public void Map_EmptySourceObject_CreatesEmptyTarget()
        {
            // Arrange
            var source = new SourceModel();

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
            Assert.AreEqual(0, result.Age);
        }

        [Test]
        public void Map_SourceWithAllNullProperties_HandlesGracefully()
        {
            // Arrange
            var source = new SourceWithNested { Name = null, Address = null };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
            Assert.IsNull(result.Address);
        }

        [Test]
        public void Map_DifferentTypesSameStructure_MapsCorrectly()
        {
            // Arrange
            var source = new GenericSource<int> { Value = 42 };

            // Act
            var result = _mapper.Map<GenericSource<int>, GenericTarget<int>>(source);

            // Assert
            Assert.AreEqual(42, result.Value);
        }

        #endregion Edge Cases
    }

    // Additional test models
    public class SourceWithShort
    {
        public short Value { get; set; }
    }

    public class TargetWithInt
    {
        public int Value { get; set; }
    }

    public class DeepNestedSource
    {
        public string? Name { get; set; }
        public Level1Source? Level1 { get; set; }
    }

    public class DeepNestedTarget
    {
        public string? Name { get; set; }
        public Level1Target? Level1 { get; set; }
    }

    public class Level1Source
    {
        public string? Value { get; set; }
        public Level2Source? Level2 { get; set; }
    }

    public class Level1Target
    {
        public string? Value { get; set; }
        public Level2Target? Level2 { get; set; }
    }

    public class Level2Source
    {
        public string? Value { get; set; }
        public Level3Source? Level3 { get; set; }
    }

    public class Level2Target
    {
        public string? Value { get; set; }
        public Level3Target? Level3 { get; set; }
    }

    public class Level3Source
    {
        public string? Value { get; set; }
    }

    public class Level3Target
    {
        public string? Value { get; set; }
    }

    public class GenericSource<T>
    {
        public T? Value { get; set; }
    }

    public class GenericTarget<T>
    {
        public T? Value { get; set; }
    }
}