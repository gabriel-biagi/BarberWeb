using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using Xunit;

namespace BarberWeb.Domain.Tests.Entities
{
    public class ServiceTests
    {
        private const string ValidName = "Corte";
        private const string ValidDescription = "Corte masculino";

        public static TheoryData<string?> InvalidNames => new()
        {
            null,
            "",
            "   ",
            "ab",                    // 2 caracteres
            new string('a', 61)
        };

        public static TheoryData<string?> InvalidDescriptions => new()
        {
            null,
            "",
            "    ",
            "abcd",                  // 4 caracteres
            new string('a', 101)
        };

        [Theory]
        [InlineData(3, 5)]
        [InlineData(60, 100)]
        public void Constructor_WithLengthsAtTheLimits_ShouldCreate(int nameLength, int descriptionLength)
        {
            var name = new string('n', nameLength);
            var description = new string('d', descriptionLength);

            var service = new Service(name, description);

            Assert.Equal(name, service.Name);
            Assert.Equal(description, service.Description);
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void Constructor_WithInvalidName_ShouldThrow(string? name)
        {
            var exception = Assert.Throws<InvalidInputException>(() => new Service(name!, ValidDescription));

            Assert.Equal("Name must be between 3 and 60 characters.", exception.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidDescriptions))]
        public void Constructor_WithInvalidDescription_ShouldThrow(string? description)
        {
            var exception = Assert.Throws<InvalidInputException>(() => new Service(ValidName, description!));

            Assert.Equal("Description must be between 5 and 100 characters.", exception.Message);
        }

        [Fact]
        public void Update_WithValidValues_ShouldReplaceNameAndDescription()
        {
            var service = new Service(ValidName, ValidDescription) { Id = 3 };

            service.Update("Barba", "Barba com toalha quente");

            Assert.Equal("Barba", service.Name);
            Assert.Equal("Barba com toalha quente", service.Description);
            Assert.Equal(3, service.Id);
        }

        [Theory]
        [MemberData(nameof(InvalidNames))]
        public void Update_WithInvalidName_ShouldThrowAndKeepOriginalValues(string? name)
        {
            var service = new Service(ValidName, ValidDescription);

            Assert.Throws<InvalidInputException>(() => service.Update(name!, "Nova descrição"));

            Assert.Equal(ValidName, service.Name);
            Assert.Equal(ValidDescription, service.Description);
        }

        [Theory]
        [MemberData(nameof(InvalidDescriptions))]
        public void Update_WithInvalidDescription_ShouldThrowAndKeepOriginalValues(string? description)
        {
            var service = new Service(ValidName, ValidDescription);

            Assert.Throws<InvalidInputException>(() => service.Update("Novo nome", description!));

            Assert.Equal(ValidName, service.Name);
            Assert.Equal(ValidDescription, service.Description);
        }
    }
}
