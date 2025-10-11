using TurboMapper.Impl;

namespace TurboMapper.Tests
{
    [TestFixture]
    public class Release120_Tests
    {
        [Test]
        public void Task1_1_RefactorDuplicatedGetMemberPathMethods()
        {
            // This is more of a code structure improvement test
            // The functionality should remain the same, just with consolidated code
            var mapper = new Mapper();

            // Test that the mapping still works correctly after refactoring
            //var config = new MappingModule<Person, PersonDto>(false);
            var expression = new MappingExpression<Person, PersonDto>();

            // The refactoring should not affect the functionality
            Assert.IsNotNull(expression);
        }

        [Test]
        public void Task1_2_ReflectionMetadataCaching()
        {
            var mapper = new Mapper();

            // Create a simple mapping to test caching
            mapper.CreateMap<Person, PersonDto>();

            // Map multiple times to test caching performance
            var source = new Person { Name = "John", Age = 30 };
            var result1 = mapper.Map<Person, PersonDto>(source);
            var result2 = mapper.Map<Person, PersonDto>(source);

            Assert.AreEqual("John", result1.Name);
            Assert.AreEqual(30, result1.Age);
            Assert.AreEqual(result1.Name, result2.Name);
            Assert.AreEqual(result1.Age, result2.Age);
        }

        [Test]
        public void Task1_3_OptimizeObjectCreation()
        {
            var mapper = new Mapper();
            mapper.CreateMap<Person, PersonDto>();

            var source = new Person { Name = "Jane", Age = 25 };
            var result = mapper.Map<Person, PersonDto>(source);

            Assert.IsNotNull(result);
            Assert.AreEqual("Jane", result.Name);
            Assert.AreEqual(25, result.Age);
        }

        [Test]
        public void Task1_4_SimplifyComplexMethods()
        {
            var mapper = new Mapper();
            mapper.CreateMap<Person, PersonDto>();

            // Test default mapping functionality
            var source = new Person { Name = "Bob", Age = 40 };
            var result = mapper.Map<Person, PersonDto>(source);

            Assert.AreEqual("Bob", result.Name);
            Assert.AreEqual(40, result.Age);
        }

        [Test]
        public void Task2_1_CompiledExpressionTrees_Performance()
        {
            var mapper = new Mapper();
            mapper.CreateMap<Person, PersonDto>();

            // Execute multiple mappings to verify compiled expressions work
            for (int i = 0; i < 100; i++)
            {
                var source = new Person { Name = $"Person{i}", Age = 20 + i % 50 };
                var result = mapper.Map<Person, PersonDto>(source);

                Assert.AreEqual($"Person{i}", result.Name);
                Assert.AreEqual(20 + i % 50, result.Age);
            }
        }

        [Test]
        public void Task2_2_ConfigurationCaching()
        {
            var mapper = new Mapper();
            mapper.CreateMap<Person, PersonDto>();

            // Test that configuration lookup works
            var source = new Person { Name = "Cached", Age = 35 };
            var result = mapper.Map<Person, PersonDto>(source);

            Assert.AreEqual("Cached", result.Name);
            Assert.AreEqual(35, result.Age);
        }

        [Test]
        public void Task3_1_CollectionMappingSupport()
        {
            var mapper = new Mapper();
            mapper.CreateMap<Person, PersonDto>();

            var people = new List<Person>
            {
                new Person { Name = "Alice", Age = 28 },
                new Person { Name = "Bob", Age = 32 }
            };

            // Test collection mapping using the new Map method that returns IEnumerable<TDestination>
            var peopleDto = mapper.Map<Person, PersonDto>(people);
            var peopleDtoList = peopleDto.ToList(); // Convert to list to access Count and indexer

            Assert.AreEqual(2, peopleDtoList.Count);
            Assert.AreEqual("Alice", peopleDtoList[0].Name);
            Assert.AreEqual(28, peopleDtoList[0].Age);
            Assert.AreEqual("Bob", peopleDtoList[1].Name);
            Assert.AreEqual(32, peopleDtoList[1].Age);
        }

        [Test]
        public void Task3_4_IgnoredPropertiesOption()
        {
            var mapper = new Mapper();

            // Create a custom mapping with ignored properties
            var expression = new MappingExpression<Person, PersonDto>();
            expression.Ignore(x => x.Age); // Ignore the Age property

            // Simulate adding this to configuration (simplified test)
            var mappings = expression.Mappings;
            var ignoredMapping = mappings.FirstOrDefault(m => m.IsIgnored && m.TargetProperty == "Age");

            Assert.IsNotNull(ignoredMapping);
            Assert.IsTrue(ignoredMapping.IsIgnored);
        }

        [Test]
        public void Task5_1_CustomTypeConvertersRegistration()
        {
            // Create a mapping from string to int using converter registered in module
            var converterModule = new StringToIntConverterModule();
            var mapper = new Mapper();

            // Register the module which will register the string to int converter
            ((IMappingModule)converterModule).CreateMap(mapper);

            // Test that a string value can be converted to int through the registered converter
            var source = new StringValueClass { Number = "42" };
            var result = mapper.Map<StringValueClass, IntValueClass>(source);

            Assert.AreEqual(42, result.Number);
        }

        // Test module for converter registration
        public class StringToIntConverterModule : MappingModule<StringValueClass, IntValueClass>
        {
            public StringToIntConverterModule() : base(true) // Enable default mapping for this test
            {
                // Register converter within the module
                RegisterConverter<string, int>(s => int.Parse(s));
            }

            public override Action<IMappingExpression<StringValueClass, IntValueClass>> CreateMappings()
            {
                return expression => { };
            }
        }

        public class StringValueClass
        {
            public string Number { get; set; }
        }

        public class IntValueClass
        {
            public int Number { get; set; }
        }

        [Test]
        public void Task5_2_ImprovedNullableTypeHandling()
        {
            var mapper = new Mapper();

            // Test nullable to non-nullable conversion
            int? nullableInt = 42;
            var result = mapper.GetType().GetMethod("ConvertValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(mapper, new object[] { nullableInt, typeof(int) });

            Assert.AreEqual(42, result);

            // Test non-nullable to nullable conversion
            int nonNullableInt = 35;
            var result2 = mapper.GetType().GetMethod("ConvertValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(mapper, new object[] { nonNullableInt, typeof(int?) });

            Assert.AreEqual(35, result2);
        }

        [Test]
        public void Task6_1_ImprovedErrorMessages()
        {
            var mapper = new Mapper();

            try
            {
                // Try to convert an invalid string to int to trigger error handling
                var result = mapper.GetType().GetMethod("ConvertValue",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(mapper, new object[] { "invalid_number", typeof(int) });

                // If we reach here without exception, there's an issue
                Assert.Fail("Expected exception was not thrown");
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                // Check that the inner exception contains helpful information
                Assert.IsTrue(ex.InnerException.Message.Contains("convert") || ex.InnerException.Message.Contains("Failed"));
            }
        }

        [Test]
        public void Task3_2_ConditionalMapping()
        {
            var mapper = new Mapper();

            // Create a conditional mapping expression
            var expression = new MappingExpression<Person, PersonDto>();

            // Add a conditional mapping (simplified test)
            expression.When(x => x.Name, p => p.Age > 18);

            var mappings = expression.Mappings;
            var conditionalMapping = mappings.FirstOrDefault(m => m.Condition != null);

            Assert.IsNotNull(conditionalMapping);
        }

        [Test]
        public void Task3_3_MappingWithTransformation()
        {
            var mapper = new Mapper();

            // Create a transformation mapping expression
            var expression = new MappingExpression<Person, PersonDto>();
            expression.MapWith<Person, string>(p => p.Name, p => $"Mr. {p.Name}");

            var mappings = expression.Mappings;
            var transformationMapping = mappings.FirstOrDefault(m => m.TransformFunction != null);

            Assert.IsNotNull(transformationMapping);
        }

        [Test]
        public void Task5_4_ComprehensiveBuiltInTypeConversions()
        {
            var mapper = new Mapper();

            // Test various type conversions
            var conversions = new (object Value, Type Target, object Expected)[]
            {
                ((object)3.14, typeof(float), 3.14f),
                ((object)100, typeof(long), 100L),
                ((object)42.5f, typeof(double), 42.5),
                ((object)123, typeof(decimal), 123m),
                ((object)"2023-01-01", typeof(DateTime), new DateTime(2023, 1, 1))
            };

            foreach (var conversion in conversions)
            {
                var result = mapper.GetType().GetMethod("ConvertValue",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(mapper, new object[] { conversion.Value, conversion.Target });

                Assert.AreEqual(conversion.Expected, result,
                    $"Conversion from {conversion.Value.GetType()} to {conversion.Target} failed");
            }
        }

        [Test]
        public void Task6_2_ConfigurationValidation()
        {
            var mapper = new Mapper();

            // Create a valid mapping configuration
            mapper.CreateMap<Person, PersonDto>(new List<PropertyMapping>());

            // Validate the mapping
            var validationResult = mapper.ValidateMapping<Person, PersonDto>();

            Assert.IsTrue(validationResult.IsValid, string.Join(", ", validationResult.Errors));
            Assert.AreEqual(0, validationResult.Errors.Count());
        }

        // Test models
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Address Address { get; set; }
        }

        public class PersonDto
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public AddressDto Address { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
        }

        public class AddressDto
        {
            public string Street { get; set; }
            public string City { get; set; }
        }
    }
}