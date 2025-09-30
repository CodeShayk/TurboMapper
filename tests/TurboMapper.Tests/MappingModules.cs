using System;

namespace TurboMapper.Tests
{
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
                // Age property will be mapped by default naming convention
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
                // Other properties will be mapped by default naming convention
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
                // LastName and Email will be mapped by default naming convention
            };
        }
    }

    // Example models for mapping module
    public class UserSource
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; } // This will be mapped by default
    }

    public class UserTarget
    {
        public string Name { get; set; }
        public int Years { get; set; }
        public string Email { get; set; } // Mapped by default
    }

    public class UserWithAddressSource
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Phone { get; set; } // This will be mapped by default
    }

    public class UserWithAddressTarget
    {
        public string Name { get; set; }
        public AddressWithConfig Address { get; set; }
        public string Phone { get; set; } // Mapped by default
    }

    public class AddressWithConfig
    {
        public string Street { get; set; }
        public string Location { get; set; }
        public string ZipCode { get; set; } // Mapped by default
    }
}