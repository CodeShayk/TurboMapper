using Microsoft.Extensions.DependencyInjection;

namespace TurboMapper.Tests
{
    [TestFixture]
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddTurboMapper_RegistersIMapper()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mapper = serviceProvider.GetService<IMapper>();
            Assert.IsNotNull(mapper);
        }

        [Test]
        public void AddTurboMapper_RegistersAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var mapper1 = serviceProvider.GetService<IMapper>();
            var mapper2 = serviceProvider.GetService<IMapper>();
            Assert.AreSame(mapper1, mapper2);
        }

        [Test, Ignore("DI with test")]
        public void AddTurboMapper_DiscoversMappingModules()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            // Verify mapping modules are registered by attempting a mapping
            var source = new UserSource
            {
                FirstName = "John",
                Age = 30,
                Email = "john@example.com"
            };

            var result = mapper.Map<UserSource, UserTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual(30, result.Years);
            Assert.AreEqual("john@example.com", result.Email);
        }

        [Test, Ignore("DI with test")]
        public void AddTurboMapper_HandlesMultipleMappingModules()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            // Test first mapping module
            var source1 = new UserSource { FirstName = "John", Age = 30, Email = "john@example.com" };
            var result1 = mapper.Map<UserSource, UserTarget>(source1);

            // Test second mapping module
            var source2 = new UserWithAddressSource
            {
                Name = "Jane",
                Address = new Address { Street = "123 Main St", City = "Boston", ZipCode = "02101" },
                Phone = "555-1234"
            };
            var result2 = mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source2);

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual("John", result1.Name);
            Assert.AreEqual(30, result1.Years);

            Assert.IsNotNull(result2);
            Assert.AreEqual("Jane", result2.Name);
            Assert.AreEqual("123 Main St", result2.Address.Street);
            Assert.AreEqual("Boston", result2.Address.Location);
        }

        [Test]
        public void AddTurboMapper_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddTurboMapper();

            // Assert
            Assert.AreSame(services, result);
        }

        [Test]
        public void AddTurboMapper_MultipleCallsUseSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            services.AddTurboMapper(); // Second call
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // The last registration should win, but both should resolve to same instance
            var mapper1 = serviceProvider.GetService<IMapper>();
            var mapper2 = serviceProvider.GetService<IMapper>();
            Assert.AreSame(mapper1, mapper2);
        }

        [Test]
        public void AddTurboMapper_IgnoresAssemblyLoadFailures()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act - Should not throw even if some assemblies can't be loaded
            Assert.DoesNotThrow(() => services.AddTurboMapper());

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();
            Assert.IsNotNull(mapper);
        }

        [Test]
        public void AddTurboMapper_SkipsDynamicAssemblies()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            // Assert - Mapper should work despite dynamic assemblies being skipped
            Assert.IsNotNull(mapper);
            var source = new SourceModel { Name = "Test", Age = 25 };
            var result = mapper.Map<SourceModel, TargetModel>(source);
            Assert.IsNotNull(result);
        }

        [Test]
        public void AddTurboMapper_WithNullServices_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddTurboMapper());
        }

        [Test]
        public void AddTurboMapper_RegisteredInServiceDescriptor()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTurboMapper();

            // Assert
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));
            Assert.IsNotNull(descriptor);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Test]
        public void AddTurboMapper_MapperIsUsableImmediately()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act
            var result = mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test", result.Name);
            Assert.AreEqual(25, result.Age);
        }

        [Test]
        public void AddTurboMapper_WorksWithScopedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();

            // Act
            var serviceProvider = services.BuildServiceProvider();

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
    }
}