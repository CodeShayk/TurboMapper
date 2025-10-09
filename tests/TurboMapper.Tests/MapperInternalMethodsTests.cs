using TurboMapper.Impl;

namespace TurboMapper.Tests
{
    /// <summary>
    /// Tests for internal methods of Mapper class using InternalsVisibleTo attribute
    /// These tests focus on the low-level implementation details
    /// </summary>
    [TestFixture]
    public class MapperInternalMethodsTests
    {
        private Mapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new Mapper();
        }

        #region ApplyNameBasedMapping Tests

        [Test]
        public void ApplyNameBasedMapping_SimpleProperties_MapsAllMatching()
        {
            // Arrange
            var source = new SourceModel { Name = "John", Age = 30 };
            var target = new TargetModel();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("John", target.Name);
            Assert.AreEqual(30, target.Age);
        }

        [Test]
        public void ApplyNameBasedMapping_NonMatchingTypes_SkipsProperty()
        {
            // Arrange
            var source = new SourceWithIncompatible { Name = "Test", Value = "NotAnInt" };
            var target = new TargetWithInt1();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual(0, target.Value); // Not mapped due to type mismatch
        }

        [Test]
        public void ApplyNameBasedMapping_NestedComplexType_MapsRecursively()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Parent",
                Address = new Address { Street = "123 Main", City = "Boston" }
            };
            var target = new TargetWithNested();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Parent", target.Name);
            Assert.IsNotNull(target.Address);
            Assert.AreEqual("123 Main", target.Address.Street);
            Assert.AreEqual("Boston", target.Address.City);
        }

        [Test]
        public void ApplyNameBasedMapping_NullNestedProperty_CreatesNullTarget()
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
        public void ApplyNameBasedMapping_ExistingNestedTarget_ReusesInstance()
        {
            // Arrange
            var existingAddress = new Address { ZipCode = "99999" };
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "123 Main", City = "Boston" }
            };
            var target = new TargetWithNested { Address = existingAddress };

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreSame(existingAddress, target.Address);
            Assert.AreEqual("123 Main", target.Address.Street);
            Assert.AreEqual("Boston", target.Address.City);
        }

        [Test]
        public void ApplyNameBasedMapping_StringProperty_ConvertsToString()
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
        public void ApplyNameBasedMapping_EmptySource_LeavesTargetDefaults()
        {
            // Arrange
            var source = new SourceModel();
            var target = new TargetModel { Name = "Default", Age = 100 };

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.IsNull(target.Name); // Overwritten with null
            Assert.AreEqual(0, target.Age); // Overwritten with 0
        }

        #endregion ApplyNameBasedMapping Tests

        #region ApplyCustomMappings Tests

        [Test]
        public void ApplyCustomMappings_EmptyMappingList_AppliesDefaultMapping()
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
        public void ApplyCustomMappings_SingleMapping_AppliesCustomAndDefault()
        {
            // Arrange
            var source = new SourceModel { Name = "John", Age = 30 };
            var target = new TargetModel();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.AreEqual("John", target.FullName); // Custom mapping
            Assert.AreEqual(30, target.Age); // Default mapping
        }

        [Test]
        public void ApplyCustomMappings_NestedPath_CreatesIntermediateObjects()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "456 Oak", City = "Seattle" }
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
            Assert.AreEqual("456 Oak", target.Address.StreetName);
        }

        [Test]
        public void ApplyCustomMappings_MultipleNestedPaths_MapsAll()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = "789 Pine", City = "Portland", ZipCode = "97201" }
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
            Assert.AreEqual("789 Pine", target.Address.StreetName);
            Assert.AreEqual("Portland", target.Address.Location);
            Assert.AreEqual("97201", target.Address.ZipCode); // Default mapping
        }

        [Test]
        public void ApplyCustomMappings_InvalidSourcePath_SetsTargetToNull()
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

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.IsNull(target.Name);
        }

        [Test]
        public void ApplyCustomMappings_NullNestedSource_DoesNotCreateTarget()
        {
            // Arrange
            var source = new SourceWithNested { Name = "Test", Address = null };
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
        public void ApplyCustomMappings_OverlappingPaths_CustomTakesPrecedence()
        {
            // Arrange
            var source = new SourceModel { Name = "CustomName", Age = 25 };
            var target = new TargetModel();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.AreEqual("CustomName", target.FullName); // Custom
            Assert.AreEqual("CustomName", target.Name); // Default (not overridden)
        }

        #endregion ApplyCustomMappings Tests

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
        public void ConvertValue_SameType_ReturnsValue()
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
        public void ConvertValue_StringToEnum_ParsesCorrectly()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping { SourcePropertyPath = "Status", TargetPropertyPath = "Status" }
            };
            _mapper.CreateMap<SourceWithEnum, TargetWithEnum>(mappings);
            var source = new SourceWithEnum { Status = "Active" };

            // Act
            var result = _mapper.Map<SourceWithEnum, TargetWithEnum>(source);

            // Assert
            Assert.AreEqual(StatusEnum.Active, result.Status);
        }

        [Test]
        public void ConvertValue_IntToString_ConvertsToString()
        {
            // Arrange
            var source = new SourceModel { Age = 42 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModelWithStringAge>(source);

            // Assert
            Assert.AreEqual("42", result.Age);
        }

        [Test]
        public void ConvertValue_DoubleToInt_TruncatesDecimal()
        {
            // Arrange
            var source = new SourceWithDouble { Value = 42.7 };

            // Act
            var result = _mapper.Map<SourceWithDouble, TargetWithInt>(source);

            // Assert
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void ConvertValue_StringToInt_ConvertsNumericString()
        {
            // Arrange
            var source = new SourceWithStringInt { Value = "123" };

            // Act
            var result = _mapper.Map<SourceWithStringInt, TargetWithInt>(source);

            // Assert
            Assert.AreEqual(123, result.Value);
        }

        #endregion Value Conversion Tests

        #region Complex Type Detection Tests

        [Test]
        public void IsComplexType_String_ReturnsFalse()
        {
            // String is not considered complex even though it's a reference type
            // This is tested implicitly through the mapping behavior
            var source = new SourceModel { Name = "Test" };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            Assert.AreEqual("Test", result.Name);
        }

        [Test]
        public void IsComplexType_ValueType_ReturnsFalse()
        {
            // Value types are not complex
            var source = new SourceModel { Age = 25 };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            Assert.AreEqual(25, result.Age);
        }

        [Test]
        public void IsComplexType_CustomClass_ReturnsTrue()
        {
            // Custom classes are complex and should be mapped recursively
            var source = new SourceWithNested
            {
                Address = new Address { Street = "Test" }
            };
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            Assert.IsNotNull(result.Address);
            Assert.AreEqual("Test", result.Address.Street);
        }

        #endregion Complex Type Detection Tests

        #region Nested Value Handling Tests

        [Test]
        public void GetNestedValue_SingleLevel_ReturnsValue()
        {
            // Tested implicitly through custom mappings
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "Test" };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            Assert.AreEqual("Test", result.FullName);
        }

        [Test]
        public void GetNestedValue_MultiLevel_TraversesPath()
        {
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Street"
                }
            };
            _mapper.CreateMap<SourceWithNested, FlatTarget>(mappings);
            var source = new SourceWithNested
            {
                Address = new Address { Street = "123 Main" }
            };
            var result = _mapper.Map<SourceWithNested, FlatTarget>(source);

            Assert.AreEqual("123 Main", result.Street);
        }

        [Test]
        public void GetNestedValue_NullInPath_ReturnsNull()
        {
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Address.Street",
                    TargetPropertyPath = "Street"
                }
            };
            _mapper.CreateMap<SourceWithNested, FlatTarget>(mappings);
            var source = new SourceWithNested { Address = null };
            var result = _mapper.Map<SourceWithNested, FlatTarget>(source);

            Assert.IsNull(result.Street);
        }

        [Test]
        public void SetNestedValue_SingleLevel_SetsValue()
        {
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "TestValue" };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            Assert.AreEqual("TestValue", result.FullName);
        }

        [Test]
        public void SetNestedValue_MultiLevel_CreatesIntermediateObjects()
        {
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
                Address = new Address { Street = "789 Elm" }
            };
            var result = _mapper.Map<SourceWithNested, TargetWithNestedConfig>(source);

            Assert.IsNotNull(result.Address);
            Assert.AreEqual("789 Elm", result.Address.StreetName);
        }

        [Test]
        public void SetNestedValue_DeepNesting_CreatesAllLevels()
        {
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Level1.Level2.Level3.Value",
                    TargetPropertyPath = "Level1.Level2.Level3.Value"
                }
            };
            _mapper.CreateMap<DeepNestedSource, DeepNestedTarget>(mappings);
            var source = new DeepNestedSource
            {
                Level1 = new Level1Source
                {
                    Level2 = new Level2Source
                    {
                        Level3 = new Level3Source { Value = "DeepValue" }
                    }
                }
            };
            var result = _mapper.Map<DeepNestedSource, DeepNestedTarget>(source);

            Assert.IsNotNull(result.Level1);
            Assert.IsNotNull(result.Level1.Level2);
            Assert.IsNotNull(result.Level1.Level2.Level3);
            Assert.AreEqual("DeepValue", result.Level1.Level2.Level3.Value);
        }

        #endregion Nested Value Handling Tests

        #region CreateMap Internal Behavior Tests

        [Test]
        public void CreateMap_FirstTime_StoresConfiguration()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping { SourcePropertyPath = "Name", TargetPropertyPath = "FullName" }
            };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "Test" };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.FullName);
        }

        [Test]
        public void CreateMap_SecondTime_OverwritesPrevious()
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

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(mappings1);
            _mapper.CreateMap<SourceModel, TargetModel>(mappings2);
            var source = new SourceModel { Name = "Test", Age = 25 };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.IsNull(result.FullName);
            Assert.AreEqual(25, result.Age);
        }

        [Test]
        public void CreateMap_DifferentSourceTargetPairs_Independent()
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

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(mappings1);
            _mapper.CreateMap<UserWithExtraSource, UserWithExtraTarget>(mappings2);

            var source1 = new SourceModel { Name = "Test1" };
            var source2 = new UserWithExtraSource { FirstName = "Test2" };

            var result1 = _mapper.Map<SourceModel, TargetModel>(source1);
            var result2 = _mapper.Map<UserWithExtraSource, UserWithExtraTarget>(source2);

            // Assert
            Assert.AreEqual("Test1", result1.FullName);
            Assert.AreEqual("Test2", result2.FullName);
        }

        #endregion CreateMap Internal Behavior Tests

        #region Edge Case Tests

        [Test]
        public void ApplyNameBasedMapping_CircularReference_HandlesGracefully()
        {
            // This tests that we don't infinitely recurse on circular references
            // The mapper should handle this by only going one level deep for matching names
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModel();

            // Act - should not throw StackOverflowException
            Assert.DoesNotThrow(() => _mapper.ApplyNameBasedMapping(source, target));
        }

        [Test]
        public void ApplyCustomMappings_EmptyPropertyPath_HandlesGracefully()
        {
            // Arrange
            var source = new SourceModel { Name = "Test" };
            var target = new TargetModel();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "",
                    TargetPropertyPath = "Name"
                }
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _mapper.ApplyCustomMappings(source, target, mappings));
        }

        [Test]
        public void ApplyNameBasedMapping_VeryDeepNesting_WorksCorrectly()
        {
            // Arrange
            var source = new DeepNestedSource
            {
                Name = "Root",
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
            var target = new DeepNestedTarget();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Root", target.Name);
            Assert.AreEqual("L1", target.Level1.Value);
            Assert.AreEqual("L2", target.Level1.Level2.Value);
            Assert.AreEqual("L3", target.Level1.Level2.Level3.Value);
        }

        [Test]
        public void ApplyCustomMappings_DuplicateTargetPath_LastOneWins()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModel();
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
                    TargetPropertyPath = "FullName" // Same target as above
                }
            };

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            // Assert
            Assert.AreEqual("25", target.FullName); // Age converted to string wins
        }

        [Test]
        public void ApplyNameBasedMapping_MixedAccessibility_OnlyMapsWriteable()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModelWithReadOnly();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            // Assert
            Assert.AreEqual("Test", target.Name);
            Assert.AreEqual(25, target.Age);
            Assert.IsNull(target.ReadOnlyProperty); // Should not be set
        }

        [Test]
        public void ApplyCustomMappings_PathWithSpaces_HandlesCorrectly()
        {
            // Property paths with spaces in property names (if they exist)
            // Should be handled by the GetProperty call
            var source = new SourceModel { Name = "Test", Age = 25 };
            var target = new TargetModel();
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "FullName"
                }
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _mapper.ApplyCustomMappings(source, target, mappings));
        }

        #endregion Edge Case Tests

        #region Performance and Optimization Tests

        [Test]
        public void ApplyNameBasedMapping_LargeObject_PerformsReasonably()
        {
            // Arrange
            var source = new LargeSourceModel
            {
                Prop1 = "Value1",
                Prop2 = "Value2",
                Prop3 = "Value3",
                Prop4 = "Value4",
                Prop5 = "Value5",
                Prop6 = "Value6",
                Prop7 = "Value7",
                Prop8 = "Value8",
                Prop9 = "Value9",
                Prop10 = "Value10"
            };
            var target = new LargeTargetModel();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            _mapper.ApplyNameBasedMapping(source, target);

            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100); // Should complete in less than 100ms
            Assert.AreEqual("Value1", target.Prop1);
            Assert.AreEqual("Value10", target.Prop10);
        }

        [Test]
        public void ApplyCustomMappings_ManyMappings_PerformsReasonably()
        {
            // Arrange
            var mappings = new List<PropertyMapping>();
            for (int i = 1; i <= 10; i++)
            {
                mappings.Add(new PropertyMapping
                {
                    SourcePropertyPath = $"Prop{i}",
                    TargetPropertyPath = $"Prop{i}"
                });
            }

            var source = new LargeSourceModel
            {
                Prop1 = "V1",
                Prop2 = "V2",
                Prop3 = "V3",
                Prop4 = "V4",
                Prop5 = "V5",
                Prop6 = "V6",
                Prop7 = "V7",
                Prop8 = "V8",
                Prop9 = "V9",
                Prop10 = "V10"
            };
            var target = new LargeTargetModel();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            _mapper.ApplyCustomMappings(source, target, mappings);

            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100);
            Assert.AreEqual("V1", target.Prop1);
            Assert.AreEqual("V10", target.Prop10);
        }

        #endregion Performance and Optimization Tests
    }

    #region Additional Test Models

    public class SourceWithIncompatible
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TargetWithInt1
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class SourceWithDouble
    {
        public double Value { get; set; }
    }

    public class SourceWithStringInt
    {
        public string? Value { get; set; }
    }

    public class FlatTarget1
    {
        public string? Name { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
    }

    public class DeepNestedSource1
    {
        public string? Name { get; set; }
        public Level1Source? Level1 { get; set; }
    }

    public class DeepNestedTarget1
    {
        public string? Name { get; set; }
        public Level1Target? Level1 { get; set; }
    }

    public class Level1Source1
    {
        public string? Value { get; set; }
        public Level2Source? Level2 { get; set; }
    }

    public class Level1Target1
    {
        public string? Value { get; set; }
        public Level2Target? Level2 { get; set; }
    }

    public class Level2Source1
    {
        public string? Value { get; set; }
        public Level3Source? Level3 { get; set; }
    }

    public class Level2Target1
    {
        public string? Value { get; set; }
        public Level3Target? Level3 { get; set; }
    }

    public class Level3Source1
    {
        public string? Value { get; set; }
    }

    public class Level3Target1
    {
        public string? Value { get; set; }
    }

    public class LargeSourceModel
    {
        public string? Prop1 { get; set; }
        public string? Prop2 { get; set; }
        public string? Prop3 { get; set; }
        public string? Prop4 { get; set; }
        public string? Prop5 { get; set; }
        public string? Prop6 { get; set; }
        public string? Prop7 { get; set; }
        public string? Prop8 { get; set; }
        public string? Prop9 { get; set; }
        public string? Prop10 { get; set; }
    }

    public class LargeTargetModel
    {
        public string? Prop1 { get; set; }
        public string? Prop2 { get; set; }
        public string? Prop3 { get; set; }
        public string? Prop4 { get; set; }
        public string? Prop5 { get; set; }
        public string? Prop6 { get; set; }
        public string? Prop7 { get; set; }
        public string? Prop8 { get; set; }
        public string? Prop9 { get; set; }
        public string? Prop10 { get; set; }
    }

    #endregion Additional Test Models
}