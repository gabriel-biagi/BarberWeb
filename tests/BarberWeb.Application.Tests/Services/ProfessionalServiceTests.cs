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
            professional.Id = professionalId; // Atribuição direta utilizando o setter público

            _repositoryMock.Setup(r => r.GetByIdAsync(professionalId))
                .ReturnsAsync(professional);

            // Act
            var result = await _service.GetByIdAsync(professionalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Carlos Silva", result.Name);
            Assert.Equal("carlos@email.com", result.Email);
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldMapAndPersistEntitySuccessfully()
        {
            // Arrange
            var request = new ProfessionalRequest
            {
                Name = "Ana Souza",
                Email = "ana@email.com",
                PhoneNumber = "(11) 95555-4444" // Enviando com máscara para testar a normalização
            };

            Professional? capturedProfessional = null;

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Professional>()))
                .Callback<Professional>(p => 
                {
                    capturedProfessional = p;
                    p.Id = 10;
                })
                .ReturnsAsync((Professional p) => p);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ana Souza", result.Name);
            Assert.Equal("ana@email.com", result.Email);

            // Conferindo se a entidade montada possui os dados corretos do request e normalizados
            Assert.NotNull(capturedProfessional);
            Assert.Equal(request.Name, capturedProfessional.Name);
            Assert.Equal(request.Email, capturedProfessional.Email);
            Assert.Equal("11955554444", capturedProfessional.PhoneNumber); // Normalizado pela entidade

            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Professional>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidRequestData_ShouldThrowInvalidInputExceptionAndNotCallRepository()
        {
            // Arrange (Nome inválido com menos de 3 caracteres)
            var request = new ProfessionalRequest
            {
                Name = "Ab",
                Email = "ana@email.com",
                PhoneNumber = "11955554444"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidInputException>(() => 
                _service.CreateAsync(request));

            Assert.Equal("Name must be between 3 and 60 characters.", exception.Message);
            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Professional>()), Times.Never);
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
                .ThrowsAsync(new UniqueConstraintViolationException("Duplicate key", null!));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() => 
                _service.CreateAsync(request));

            Assert.Equal("Email or phone already registered.", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WhenOtherExceptionOccurs_ShouldPassThroughWithoutTranslation()
        {
            // Arrange
            var request = new ProfessionalRequest
            {
                Name = "Ana Souza",
                Email = "ana@email.com",
                PhoneNumber = "11955554444"
            };

            var unexpectedException = new InvalidOperationException("Database connection failure");

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Professional>()))
                .ThrowsAsync(unexpectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateAsync(request));

            Assert.Same(unexpectedException, exception);
        }
    }
}