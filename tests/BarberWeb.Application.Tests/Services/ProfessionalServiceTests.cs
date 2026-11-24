using BarberWeb.Application.Common;
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

        [Fact]
        public async Task GetByIdAsync_WhenProfessionalDoesNotExist_ShouldReturnNotFoundFailure()
        {
            // Arrange
            int professionalId = 1;
            _repositoryMock.Setup(r => r.GetByIdAsync(professionalId))
                .ReturnsAsync((Professional)null!);

            // Act
            var result = await _service.GetByIdAsync(professionalId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("Professional not found.", result.Error.Message);
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
            Assert.True(result.IsSuccess);
            Assert.Equal(professionalId, result.Value.Id);
            Assert.Equal("Carlos Silva", result.Value.Name);
            Assert.Equal("carlos@email.com", result.Value.Email);
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
            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value.Id);
            Assert.Equal("Ana Souza", result.Value.Name);
            Assert.Equal("ana@email.com", result.Value.Email);

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
        public async Task CreateAsync_WhenUniqueConstraintViolationOccurs_ShouldReturnConflictFailure()
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

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.Error.Type);
            Assert.Equal("Professional already exists.", result.Error.Message);
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