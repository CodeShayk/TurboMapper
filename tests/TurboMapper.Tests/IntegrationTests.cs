using Microsoft.Extensions.DependencyInjection;

namespace TurboMapper.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void EndToEnd_SimpleMappingWithDI()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new SourceModel { Name = "John Doe", Age = 30 };

            // Act
            var result = mapper.Map<SourceModel, TargetModel>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
            Assert.AreEqual(30, result.Age);
        }

        [Test]
        public void EndToEnd_ComplexMappingWithMappingModule()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new UserWithExtraSource
            {
                FirstName = "Jane",
                LastName = "Smith",
                Age = 28,
                Email = "jane.smith@example.com"
            };

            // Act
            var result = mapper.Map<UserWithExtraSource, UserWithExtraTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Jane", result.FullName);
            Assert.AreEqual("Smith", result.LastName);
            Assert.AreEqual(28, result.AgeInYears);
            Assert.AreEqual("jane.smith@example.com", result.Email);
        }

        [Test, Ignore("DI with test")]
        public void EndToEnd_NestedObjectMappingWithMixedConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new UserWithAddressSource
            {
                Name = "Bob Johnson",
                Address = new Address
                {
                    Street = "456 Oak Avenue",
                    City = "San Francisco",
                    ZipCode = "94102"
                },
                Phone = "555-9876"
            };

            // Act
            var result = mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Bob Johnson", result.Name);
            Assert.IsNotNull(result.Address);
            Assert.AreEqual("456 Oak Avenue", result.Address.Street);
            Assert.AreEqual("San Francisco", result.Address.Location);
            Assert.AreEqual("94102", result.Address.ZipCode);
            Assert.AreEqual("555-9876", result.Phone);
        }

        [Test, Ignore("DI with test")]
        public void EndToEnd_MultipleConsecutiveMappings()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source1 = new UserSource { FirstName = "Alice", Age = 25, Email = "alice@test.com" };
            var source2 = new UserSource { FirstName = "Bob", Age = 35, Email = "bob@test.com" };
            var source3 = new UserSource { FirstName = "Charlie", Age = 45, Email = "charlie@test.com" };

            // Act
            var result1 = mapper.Map<UserSource, UserTarget>(source1);
            var result2 = mapper.Map<UserSource, UserTarget>(source2);
            var result3 = mapper.Map<UserSource, UserTarget>(source3);

            // Assert
            Assert.AreEqual("Alice", result1.Name);
            Assert.AreEqual(25, result1.Years);
            Assert.AreEqual("alice@test.com", result1.Email);

            Assert.AreEqual("Bob", result2.Name);
            Assert.AreEqual(35, result2.Years);
            Assert.AreEqual("bob@test.com", result2.Email);

            Assert.AreEqual("Charlie", result3.Name);
            Assert.AreEqual(45, result3.Years);
            Assert.AreEqual("charlie@test.com", result3.Email);
        }

        [Test]
        public void EndToEnd_MappingWithNullHandling()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new UserWithAddressSource
            {
                Name = "Test User",
                Address = null,
                Phone = "555-0000"
            };

            // Act
            var result = mapper.Map<UserWithAddressSource, UserWithAddressTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test User", result.Name);
            Assert.IsNull(result.Address);
            Assert.AreEqual("555-0000", result.Phone);
        }

        [Test]
        public void EndToEnd_MappingSameSourceToDifferentTargets()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new SourceModel { Name = "John", Age = 30 };

            // Act
            var result1 = mapper.Map<SourceModel, TargetModel>(source);
            var result2 = mapper.Map<SourceModel, TargetModelWithStringAge>(source);

            // Assert
            Assert.AreEqual("John", result1.Name);
            Assert.AreEqual(30, result1.Age);

            Assert.AreEqual("John", result2.Name);
            Assert.AreEqual("30", result2.Age);
        }

        [Test]
        public void EndToEnd_RealWorldUserProfileMapping()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new UserProfileSource
            {
                UserId = 12345,
                FirstName = "Emma",
                LastName = "Watson",
                Email = "emma.watson@example.com",
                DateOfBirth = new DateTime(1990, 4, 15),
                Address = new Address
                {
                    Street = "123 Hollywood Blvd",
                    City = "Los Angeles",
                    ZipCode = "90028"
                },
                PhoneNumber = "555-1234",
                IsActive = true
            };

            // Act
            var result = mapper.Map<UserProfileSource, UserProfileTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12345, result.UserId);
            Assert.AreEqual("Emma", result.FirstName);
            Assert.AreEqual("Watson", result.LastName);
            Assert.AreEqual("emma.watson@example.com", result.Email);
            Assert.AreEqual(new DateTime(1990, 4, 15), result.DateOfBirth);
            Assert.IsNotNull(result.Address);
            Assert.AreEqual("123 Hollywood Blvd", result.Address.Street);
            Assert.AreEqual("Los Angeles", result.Address.City);
            Assert.AreEqual("90028", result.Address.ZipCode);
            Assert.AreEqual("555-1234", result.PhoneNumber);
            Assert.IsTrue(result.IsActive);
        }

        [Test]
        public void EndToEnd_EnumConversionInComplexObject()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new OrderSource
            {
                OrderId = "ORD-001",
                CustomerName = "John Smith",
                Status = "Active",
                TotalAmount = 199.99m
            };

            // Act
            var result = mapper.Map<OrderSource, OrderTarget>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ORD-001", result.OrderId);
            Assert.AreEqual("John Smith", result.CustomerName);
            Assert.AreEqual(StatusEnum.Active, result.Status);
            Assert.AreEqual(199.99m, result.TotalAmount);
        }

        [Test]
        public void EndToEnd_ListOfObjects()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var sources = new List<SourceModel>
            {
                new SourceModel { Name = "User1", Age = 20 },
                new SourceModel { Name = "User2", Age = 30 },
                new SourceModel { Name = "User3", Age = 40 }
            };

            // Act
            var results = new List<TargetModel>();
            foreach (var source in sources)
            {
                results.Add(mapper.Map<SourceModel, TargetModel>(source));
            }

            // Assert
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("User1", results[0].Name);
            Assert.AreEqual(20, results[0].Age);
            Assert.AreEqual("User2", results[1].Name);
            Assert.AreEqual(30, results[1].Age);
            Assert.AreEqual("User3", results[2].Name);
            Assert.AreEqual(40, results[2].Age);
        }

        [Test]
        public void EndToEnd_ThreadSafety_MultipleConcurrentMappings()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var sources = new List<SourceModel>
            {
                new SourceModel { Name = "User1", Age = 20 },
                new SourceModel { Name = "User2", Age = 30 },
                new SourceModel { Name = "User3", Age = 40 },
                new SourceModel { Name = "User4", Age = 50 }
            };

            // Act
            var results = new System.Collections.Concurrent.ConcurrentBag<TargetModel>();
            System.Threading.Tasks.Parallel.ForEach(sources, source =>
            {
                var result = mapper.Map<SourceModel, TargetModel>(source);
                results.Add(result);
            });

            // Assert
            Assert.AreEqual(4, results.Count);
            Assert.IsTrue(results.Any(r => r.Name == "User1" && r.Age == 20));
            Assert.IsTrue(results.Any(r => r.Name == "User2" && r.Age == 30));
            Assert.IsTrue(results.Any(r => r.Name == "User3" && r.Age == 40));
            Assert.IsTrue(results.Any(r => r.Name == "User4" && r.Age == 50));
        }

        [Test]
        public void EndToEnd_PerformanceTest_1000Mappings()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new SourceModel { Name = "Performance Test", Age = 25 };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                var result = mapper.Map<SourceModel, TargetModel>(source);
            }
            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000); // Should complete in less than 5 seconds
        }

        [Test]
        public void EndToEnd_ChainedMappings()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new SourceModel { Name = "Test", Age = 25 };

            // Act - Chain multiple mappings
            var intermediate = mapper.Map<SourceModel, TargetModel>(source);
            var final = mapper.Map<TargetModel, SourceModel>(intermediate);

            // Assert
            Assert.AreEqual("Test", final.Name);
            Assert.AreEqual(25, final.Age);
        }

        [Test]
        public void EndToEnd_MappingWithDefaultValues()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var source = new SourceWithDefaults();

            // Act
            var result = mapper.Map<SourceWithDefaults, TargetWithDefaults>(source);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Name); // Default null
            Assert.AreEqual(0, result.Age); // Default 0
        }

        [Test]
        public void EndToEnd_ComplexBusinessScenario_OrderProcessing()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTurboMapper();
            var serviceProvider = services.BuildServiceProvider();
            var mapper = serviceProvider.GetService<IMapper>();

            var orderDto = new OrderDTO
            {
                OrderNumber = "ORD-2024-001",
                CustomerInfo = new CustomerDTO
                {
                    Name = "Jane Doe",
                    Email = "jane.doe@example.com",
                    ShippingAddress = new Address
                    {
                        Street = "789 Main St",
                        City = "Seattle",
                        ZipCode = "98101"
                    }
                },
                OrderDate = DateTime.Now,
                Items = 5,
                TotalPrice = 499.99m
            };

            // Act
            var orderEntity = mapper.Map<OrderDTO, OrderEntity>(orderDto);

            // Assert
            Assert.IsNotNull(orderEntity);
            Assert.AreEqual("ORD-2024-001", orderEntity.OrderNumber);
            Assert.IsNotNull(orderEntity.CustomerInfo);
            Assert.AreEqual("Jane Doe", orderEntity.CustomerInfo.Name);
            Assert.AreEqual("jane.doe@example.com", orderEntity.CustomerInfo.Email);
            Assert.IsNotNull(orderEntity.CustomerInfo.ShippingAddress);
            Assert.AreEqual("789 Main St", orderEntity.CustomerInfo.ShippingAddress.Street);
            Assert.AreEqual("Seattle", orderEntity.CustomerInfo.ShippingAddress.City);
            Assert.AreEqual("98101", orderEntity.CustomerInfo.ShippingAddress.ZipCode);
            Assert.AreEqual(5, orderEntity.Items);
            Assert.AreEqual(499.99m, orderEntity.TotalPrice);
        }
    }

    #region Test Models for Integration Tests

    public class UserProfileSource
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserProfileTarget
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Address? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class OrderSource
    {
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderTarget
    {
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public StatusEnum Status { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SourceWithDefaults
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class TargetWithDefaults
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    public class OrderDTO
    {
        public string? OrderNumber { get; set; }
        public CustomerDTO? CustomerInfo { get; set; }
        public DateTime OrderDate { get; set; }
        public int Items { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderEntity
    {
        public string? OrderNumber { get; set; }
        public CustomerEntity? CustomerInfo { get; set; }
        public DateTime OrderDate { get; set; }
        public int Items { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CustomerDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public Address? ShippingAddress { get; set; }
    }

    public class CustomerEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public Address? ShippingAddress { get; set; }
    }

    #endregion Test Models for Integration Tests
}