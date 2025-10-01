using System;
using System.Collections.Generic;
using TurboMapper.Impl;

namespace TurboMapper.Tests
{
    [TestFixture]
    public class EdgeCaseAndErrorHandlingTests
    {
        private Mapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new Mapper();
        }

        #region Null and Empty Tests

        [Test]
        public void Map_NullSource_ReturnsDefault()
        {
            // Arrange
            SourceModel source = null;

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Map_EmptyString_MapsCorrectly()
        {
            // Arrange
            var source = new SourceModel { Name = "", Age = 0 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("", result.Name);
            Assert.AreEqual(0, result.Age);
        }

        [Test]
        public void Map_WhitespaceString_MapsCorrectly()
        {
            // Arrange
            var source = new SourceModel { Name = "   ", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("   ", result.Name);
        }

        [Test]
        public void Map_AllPropertiesNull_CreatesTargetWithNulls()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = null,
                Address = null,
                Phone = null
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
            Assert.IsNull(result.Address);
        }

        #endregion Null and Empty Tests

        #region Type Conversion Edge Cases

        [Test]
        public void Map_LargeIntegerValue_HandlesCorrectly()
        {
            // Arrange
            var source = new SourceWithLargeInt { Value = int.MaxValue };

            // Act
            var result = _mapper.Map<SourceWithLargeInt, TargetWithLargeInt>(source);

            // Assert
            Assert.AreEqual(int.MaxValue, result.Value);
        }

        [Test]
        public void Map_NegativeIntegerValue_HandlesCorrectly()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = -1 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual(-1, result.Age);
        }

        [Test]
        public void Map_DecimalPrecision_MaintainsPrecision()
        {
            // Arrange
            var source = new SourceWithDecimal { Amount = 123.456789m };

            // Act
            var result = _mapper.Map<SourceWithDecimal, TargetWithDecimal>(source);

            // Assert
            Assert.AreEqual(123.456789m, result.Amount);
        }

        [Test]
        public void Map_BooleanTrue_MapsCorrectly()
        {
            // Arrange
            var source = new SourceWithBool { IsActive = true };

            // Act
            var result = _mapper.Map<SourceWithBool, TargetWithBool>(source);

            // Assert
            Assert.IsTrue(result.IsActive);
        }

        [Test]
        public void Map_BooleanFalse_MapsCorrectly()
        {
            // Arrange
            var source = new SourceWithBool { IsActive = false };

            // Act
            var result = _mapper.Map<SourceWithBool, TargetWithBool>(source);

            // Assert
            Assert.IsFalse(result.IsActive);
        }

        [Test]
        public void Map_DateTimeValue_MapsCorrectly()
        {
            // Arrange
            var dateTime = new DateTime(2024, 1, 15, 14, 30, 0);
            var source = new SourceWithDateTime { CreatedDate = dateTime };

            // Act
            var result = _mapper.Map<SourceWithDateTime, TargetWithDateTime>(source);

            // Assert
            Assert.AreEqual(dateTime, result.CreatedDate);
        }

        [Test]
        public void Map_DateTimeMinValue_HandlesCorrectly()
        {
            // Arrange
            var source = new SourceWithDateTime { CreatedDate = DateTime.MinValue };

            // Act
            var result = _mapper.Map<SourceWithDateTime, TargetWithDateTime>(source);

            // Assert
            Assert.AreEqual(DateTime.MinValue, result.CreatedDate);
        }

        [Test]
        public void Map_DateTimeMaxValue_HandlesCorrectly()
        {
            // Arrange
            var source = new SourceWithDateTime { CreatedDate = DateTime.MaxValue };

            // Act
            var result = _mapper.Map<SourceWithDateTime, TargetWithDateTime>(source);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, result.CreatedDate);
        }

        #endregion Type Conversion Edge Cases

        #region String Edge Cases

        [Test]
        public void Map_VeryLongString_MapsCorrectly()
        {
            // Arrange
            var longString = new string('A', 10000);
            var source = new SourceModel { Name = longString, Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual(longString, result.Name);
        }

        [Test]
        public void Map_StringWithSpecialCharacters_MapsCorrectly()
        {
            // Arrange
            var specialString = "Test!@#$%^&*()_+-=[]{}|;':\",./<>?";
            var source = new SourceModel { Name = specialString, Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual(specialString, result.Name);
        }

        [Test]
        public void Map_StringWithUnicodeCharacters_MapsCorrectly()
        {
            // Arrange
            var unicodeString = "Hello 世界 مرحبا Привет";
            var source = new SourceModel { Name = unicodeString, Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual(unicodeString, result.Name);
        }

        [Test]
        public void Map_StringWithNewlines_MapsCorrectly()
        {
            // Arrange
            var stringWithNewlines = "Line 1\nLine 2\r\nLine 3";
            var source = new SourceModel { Name = stringWithNewlines, Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual(stringWithNewlines, result.Name);
        }

        #endregion String Edge Cases

        #region Nested Object Edge Cases

        [Test]
        public void Map_DeeplyNestedNull_HandlesGracefully()
        {
            // Arrange
            var source = new DeepNestedSource
            {
                Name = "Test",
                Level1 = new Level1Source
                {
                    Value = "L1",
                    Level2 = null
                }
            };

            // Act
            var result = _mapper.Map<DeepNestedSource, DeepNestedTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test", result.Name);
            Assert.IsNotNull(result.Level1);
            Assert.AreEqual("L1", result.Level1.Value);
            Assert.IsNull(result.Level1.Level2);
        }

        [Test]
        public void Map_NestedObjectWithNullProperties_CreatesObjectWithNulls()
        {
            // Arrange
            var source = new SourceWithNested
            {
                Name = "Test",
                Address = new Address { Street = null, City = null, ZipCode = null }
            };

            // Act
            var result = _mapper.Map<SourceWithNested, TargetWithNested>(source);

            // Assert
            Assert.IsNotNull(result.Address);
            Assert.IsNull(result.Address.Street);
            Assert.IsNull(result.Address.City);
            Assert.IsNull(result.Address.ZipCode);
        }

        #endregion Nested Object Edge Cases

        #region Configuration Edge Cases

        [Test]
        public void CreateMap_EmptyMappingList_UsesDefaultMapping()
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
        public void CreateMap_DuplicateSourceTarget_OverwritesPrevious()
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
                    SourcePropertyPath = "Name",
                    TargetPropertyPath = "Name"
                }
            };

            // Act
            _mapper.CreateMap<SourceModel, TargetModel>(mappings1);
            _mapper.CreateMap<SourceModel, TargetModel>(mappings2);

            var source = new SourceModel { Name = "Test", Age = 25 };
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.AreEqual("Test", result.Name);
            Assert.IsNull(result.FullName);
        }

        [Test]
        public void Map_WithInvalidPropertyPath_HandlesGracefully()
        {
            // Arrange
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Invalid.Property.Path",
                    TargetPropertyPath = "Name"
                }
            };
            _mapper.CreateMap<SourceModel, TargetModel>(mappings);
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
        }

        #endregion Configuration Edge Cases

        #region Enum Edge Cases

        [Test]
        public void Map_EnumInvalidString_ThrowsArgumentException()
        {
            // Arrange
            var source = new SourceWithEnum { Status = "InvalidStatus" };
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Status",
                    TargetPropertyPath = "Status"
                }
            };
            _mapper.CreateMap<SourceWithEnum, TargetWithEnum>(mappings);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                _mapper.Map<SourceWithEnum, TargetWithEnum>(source);
            });
        }

        [Test]
        public void Map_EnumCaseInsensitive_ConvertsCorrectly()
        {
            // Arrange
            var source = new SourceWithEnum { Status = "active" };
            var mappings = new List<PropertyMapping>
            {
                new PropertyMapping
                {
                    SourcePropertyPath = "Status",
                    TargetPropertyPath = "Status"
                }
            };
            _mapper.CreateMap<SourceWithEnum, TargetWithEnum>(mappings);

            // Act
            var result = _mapper.Map<SourceWithEnum, TargetWithEnum>(source);

            // Assert
            Assert.AreEqual(StatusEnum.Active, result.Status);
        }

        #endregion Enum Edge Cases

        #region Property Access Edge Cases

        [Test]
        public void Map_WriteOnlyTargetProperty_SkipsMapping()
        {
            // Arrange
            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = _mapper.Map<SourceModel, TargetWithWriteOnly>(source);

            // Assert
            Assert.IsNotNull(result);
            // WriteOnly property cannot be verified
        }

        [Test]
        public void Map_ReadOnlyTargetProperty_SkipsMapping()
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

        #endregion Property Access Edge Cases

        #region Special Value Tests

        [Test]
        public void Map_GuidValue_MapsCorrectly()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var source = new SourceWithGuid { Id = guid };

            // Act
            var result = _mapper.Map<SourceWithGuid, TargetWithGuid>(source);

            // Assert
            Assert.AreEqual(guid, result.Id);
        }

        [Test]
        public void Map_EmptyGuid_MapsCorrectly()
        {
            // Arrange
            var source = new SourceWithGuid { Id = Guid.Empty };

            // Act
            var result = _mapper.Map<SourceWithGuid, TargetWithGuid>(source);

            // Assert
            Assert.AreEqual(Guid.Empty, result.Id);
        }

        [Test]
        public void Map_NullableIntWithValue_MapsCorrectly()
        {
            // Arrange
            var source = new SourceWithNullableInt { Value = 42 };

            // Act
            var result = _mapper.Map<SourceWithNullableInt, TargetWithNullableInt>(source);

            // Assert
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void Map_NullableIntWithNull_MapsCorrectly()
        {
            // Arrange
            var source = new SourceWithNullableInt { Value = null };

            // Act
            var result = _mapper.Map<SourceWithNullableInt, TargetWithNullableInt>(source);

            // Assert
            Assert.IsNull(result.Value);
        }

        #endregion Special Value Tests
    }

    #region Test Models for Edge Cases

    public class SourceWithLargeInt
    {
        public int Value { get; set; }
    }

    public class TargetWithLargeInt
    {
        public int Value { get; set; }
    }

    public class SourceWithDecimal
    {
        public decimal Amount { get; set; }
    }

    public class TargetWithDecimal
    {
        public decimal Amount { get; set; }
    }

    public class SourceWithBool
    {
        public bool IsActive { get; set; }
    }

    public class TargetWithBool
    {
        public bool IsActive { get; set; }
    }

    public class SourceWithDateTime
    {
        public DateTime CreatedDate { get; set; }
    }

    public class TargetWithDateTime
    {
        public DateTime CreatedDate { get; set; }
    }

    public class TargetWithWriteOnly
    {
        private string _name;

        public string Name
        {
            set { _name = value; }
        }
    }

    public class SourceWithGuid
    {
        public Guid Id { get; set; }
    }

    public class TargetWithGuid
    {
        public Guid Id { get; set; }
    }

    public class SourceWithNullableInt
    {
        public int? Value { get; set; }
    }

    public class TargetWithNullableInt
    {
        public int? Value { get; set; }
    }

    #endregion Test Models for Edge Cases
}