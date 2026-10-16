using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using Xunit;

namespace BarberWeb.Domain.Tests.Entities
{
    public class ProfessionalTests
    {
        [Fact]
        public void Constructor_WithValidArguments_ShouldCreateProfessionalSuccessfully()
        {
            // Arrange & Act
            var professional = new Professional("João Silva", "joao@email.com", "11988887777");

            // Assert
            Assert.Equal("João Silva", professional.Name);
            Assert.Equal("joao@email.com", professional.Email);
            Assert.Equal("11988887777", professional.PhoneNumber);
        }

        [Fact]
        public void Constructor_WithFormattedPhoneNumber_ShouldNormalizeCorrectly()
        {
            // Arrange & Act
            var professional = new Professional("João Silva", "joao@email.com", "(11) 98888-7777");

            // Assert
            Assert.Equal("11988887777", professional.PhoneNumber);
        }

        [Fact]
        public void Constructor_WithCountryCodePhoneNumber_ShouldAcceptSuccessfully()
        {
            // Arrange & Act (13 dígitos após normalização)
            var professional = new Professional("João Silva", "joao@email.com", "+55 (67) 99999-9999");

            // Assert
            Assert.Equal("5567999999999", professional.PhoneNumber);
        }

        [Theory]
        [InlineData("Ana")] // Limite mínimo exato (3 caracteres)
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxsdyz012345")] // Limite máximo exato (60 caracteres)
        public void Constructor_WithBoundaryValidName_ShouldCreateSuccessfully(string validName)
        {
            // Act & Assert
            var professional = new Professional(validName, "joao@email.com", "11988887777");
            Assert.Equal(validName, professional.Name);
        }

        [Theory]
        [InlineData("a@b.c")] // Limite mínimo exato (5 caracteres)
        [InlineData("abcdefghijklmnopqrstuvwxyz012345623789abcdefghijkl@email.com")] // Limite máximo exato (60 caracteres)
        public void Constructor_WithBoundaryValidEmail_ShouldCreateSuccessfully(string validEmail)
        {
            // Act & Assert
            var professional = new Professional("João Silva", validEmail, "11988887777");
            Assert.Equal(validEmail, professional.Email);
        }

        [Theory]
        [InlineData("1234567890")] // Limite mínimo exato (10 dígitos)
        [InlineData("123456789012345")] // Limite máximo exato (15 dígitos)
        public void Constructor_WithBoundaryValidPhoneNumber_ShouldCreateSuccessfully(string validPhone)
        {
            // Act & Assert
            var professional = new Professional("João Silva", "joao@email.com", validPhone);
            Assert.NotNull(professional);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("Ab")] // Menor que 3 caracteres
        [InlineData("   ")] // Apenas espaços em branco
        [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")] // 61 caracteres (maior que 60)
        public void Constructor_WithInvalidName_ShouldThrowInvalidInputException(string? invalidName)
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidInputException>(() => 
                new Professional(invalidName!, "joao@email.com", "11988887777"));

            Assert.Equal("Name must be between 3 and 60 characters.", exception.Message);
        }

        [Fact]
        public void Constructor_WithNameToLong_ShouldThrowInvalidInputException()
        {
            // Arrange
            var invalidName = new string('A', 61); // 61 caracteres (maior que 60)

            // Act & Assert
            var exception = Assert.Throws<InvalidInputException>(() => 
                new Professional(invalidName, "joao@email.com", "11988887777"));

            Assert.Equal("Name must be between 3 and 60 characters.", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("a@b")] // Menor que 5 caracteres
        [InlineData("     ")] // Apenas espaços em branco
        public void Constructor_WithInvalidEmail_ShouldThrowInvalidInputException(string? invalidEmail)
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidInputException>(() => 
                new Professional("João Silva", invalidEmail!, "11988887777"));

            Assert.Equal("Email must be between 5 and 60 characters.", exception.Message);
        }

        [Fact]
        public void Constructor_WithEmailTooLong_ShouldThrowInvalidInputException()
        {
            // Arrange
            var invalidEmail = new string('a', 51) + "@email.com"; // 61 caracteres no total (maior que 60)

            // Act & Assert
            var exception = Assert.Throws<InvalidInputException>(() => 
                new Professional("João Silva", invalidEmail, "11988887777"));

            Assert.Equal("Email must be between 5 and 60 characters.", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("123456789")] // Menor que 10 dígitos
        [InlineData("1234567890123456")] // Maior que 15 dígitos
        [InlineData("(abc) -")] // Sem nenhum dígito (normaliza para vazio)
        [InlineData("(11) 9888-777")] // 13 caracteres visuais, mas apenas 9 dígitos após normalização
        public void Constructor_WithInvalidPhoneNumber_ShouldThrowInvalidInputException(string? invalidPhone)
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidInputException>(() => 
                new Professional("João Silva", "joao@email.com", invalidPhone!));

            Assert.Equal("Phone Number must be between 10 and 15 digits.", exception.Message);
        }
    }
}