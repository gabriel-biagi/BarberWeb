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
    public class CatalogServiceServiceTests
    {
        private readonly Mock<IServiceRepository> _repositoryMock = new();
        private readonly CatalogServiceService _service;

        public CatalogServiceServiceTests()
        {
            _service = new CatalogServiceService(_repositoryMock.Object);
        }

        private static CatalogServiceRequest Request(string name = "Corte", string description = "Corte masculino")
            => new() { Name = name, Description = description };

        private Service GivenExistingService(int id = 5, string name = "Corte", string description = "Corte masculino")
        {
            var service = new Service(name, description) { Id = id };
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(service);
            return service;
        }

        private static UniqueConstraintViolationException UniqueViolation()
            => new("A service with the same name already exists.", new System.Exception("Duplicate entry"));

        // GetById

        [Fact]
        public async Task GetByIdAsync_WhenServiceExists_ShouldReturnIt()
        {
            GivenExistingService();

            var result = await _service.GetByIdAsync(5);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Value.Id);
            Assert.Equal("Corte", result.Value.Name);
            Assert.Equal("Corte masculino", result.Value.Description);
        }

        [Fact]
        public async Task GetByIdAsync_WhenServiceDoesNotExist_ShouldReturnNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Service?)null);

            var result = await _service.GetByIdAsync(5);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            Assert.Equal("Service not found.", result.Error.Message);
        }

        // GetAll

        [Fact]
        public async Task GetAllAsync_ShouldMapEveryService()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Service>
            {
                new("Barba", "Barba completa") { Id = 2 },
                new("Corte", "Corte masculino") { Id = 1 }
            });

            var result = await _service.GetAllAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { 2, 1 }, result.Value.Select(s => s.Id));
            Assert.Equal(new[] { "Barba", "Corte" }, result.Value.Select(s => s.Name));
        }

        [Fact]
        public async Task GetAllAsync_WhenThereAreNoServices_ShouldReturnEmptyList()
        {
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Service>());

            var result = await _service.GetAllAsync();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        // Create

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldPersistAndReturnCreatedService()
        {
            _repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Service>()))
                .ReturnsAsync((Service s) => { s.Id = 10; return s; });

            var result = await _service.CreateAsync(Request());

            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value.Id);
            Assert.Equal("Corte", result.Value.Name);
            _repositoryMock.Verify(r => r.CreateAsync(It.Is<Service>(s =>
                s.Name == "Corte" && s.Description == "Corte masculino")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenNameAlreadyExists_ShouldReturnConflict()
        {
            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Service>())).ThrowsAsync(UniqueViolation());

            var result = await _service.CreateAsync(Request());

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.Error.Type);
            Assert.Equal("Service already exists.", result.Error.Message);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidData_ShouldThrowBeforePersisting()
        {
            // Os DTOs já são validados pelo [ApiController]; o domínio é a segunda barreira,
            // e a exceção é convertida em 400 pelo middleware.
            await Assert.ThrowsAsync<InvalidInputException>(() => _service.CreateAsync(Request(name: "ab")));

            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Service>()), Times.Never);
        }

        // Update

        [Fact]
        public async Task UpdateAsync_WhenServiceExists_ShouldReplaceValuesAndPersist()
        {
            var service = GivenExistingService();

            var result = await _service.UpdateAsync(5, Request("Barba", "Barba completa"));

            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Value.Id);
            Assert.Equal("Barba", result.Value.Name);
            Assert.Equal("Barba completa", result.Value.Description);
            Assert.Equal("Barba", service.Name);
            _repositoryMock.Verify(r => r.UpdateAsync(service), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceDoesNotExist_ShouldReturnNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Service?)null);

            var result = await _service.UpdateAsync(5, Request());

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenNameBelongsToAnotherService_ShouldReturnConflict()
        {
            GivenExistingService();
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Service>())).ThrowsAsync(UniqueViolation());

            var result = await _service.UpdateAsync(5, Request("Barba", "Barba completa"));

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.Error.Type);
            Assert.Equal("Service already exists.", result.Error.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidData_ShouldThrowAndNotPersist()
        {
            var service = GivenExistingService();

            await Assert.ThrowsAsync<InvalidInputException>(
                () => _service.UpdateAsync(5, Request(description: new string('a', 101))));

            Assert.Equal("Corte masculino", service.Description);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Service>()), Times.Never);
        }
    }
}
