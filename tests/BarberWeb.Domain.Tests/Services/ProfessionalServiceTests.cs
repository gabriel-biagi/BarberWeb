using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.Services;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;
using Moq;
using Xunit;

namespace BarberWeb.Application.Tests.Services
{
    public class ProfessionalServiceTests
    {
        private readonly Mock<IProfessionalRepository> _repositoryMock;
        private readonly ProfessionalService _service;

        public ProfessionalServiceTests()
        {
            _repositoryMock = new Mock<IProfessionalRepository>();
            _service = new ProfessionalService(_repositoryMock.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowInvalidInputException(int invalidId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() => 
                _service.GetByIdAsync(invalidId));

            Assert.Equal("Professional code must be greater than 0", exception.Message);
            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenProfessionalDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            int professionalId = 1;
            _repositoryMock.Setup(r => r.GetByIdAsync(professionalId))
                .ReturnsAsync((Professional)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetByIdAsync(professionalId));

            Assert.Equal("Professional not found", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WhenProfessionalExists_ShouldReturnResponseDto()
        {
            // Arrange
            int professionalId = 1;
            var professional = new Professional("Carlos Silva", "carlos@email.com", "11977776666");
            typeof(Professional).GetProperty(nameof(Professional.Id))?.SetValue(professional, professionalId);

            _repositoryMock.Setup(r => r.GetByIdAsync(professionalId))
                .ReturnsAsync(professional);

            // Act
            var result = await _service.GetByIdAsync(professionalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(professionalId, result.Id);
            Assert.Equal("Carlos Silva", result.Name);
            Assert.Equal("carlos@email.com", result.Email);
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldCreateAndReturnResponseDto()
        {
            // Arrange
            var request = new ProfessionalRequest
            {
                Name = "Ana Souza",
                Email = "ana@email.com",
                PhoneNumber = "11955554444"
            };

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Professional>()))
                .Callback<Professional>(p => typeof(Professional).GetProperty(nameof(Professional.Id))?.SetValue(p, 10))
                .ReturnsAsync((Professional p) => p);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Ana Souza", result.Name);
            Assert.Equal("ana@email.com", result.Email);
            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Professional>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenUniqueConstraintViolationOccurs_ShouldThrowBusinessException()
        {
            // Arrange
            var request = new ProfessionalRequest
            {
                Name = "Ana Souza",
                Email = "ana@email.com",
                PhoneNumber = "11955554444"
            };

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Professional>()))
                .ThrowsAsync(new UniqueConstraintViolationException("Duplicate key", null!)); // Corrigido para passar os dois argumentos exigidos

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() => 
                _service.CreateAsync(request));

            Assert.Equal("Email or phone already registered.", exception.Message);
        }
    }
}