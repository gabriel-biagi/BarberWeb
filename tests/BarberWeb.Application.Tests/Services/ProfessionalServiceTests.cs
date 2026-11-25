using BarberWeb.Application.Common;
using BarberWeb.Application.Common.Interfaces;
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
        private static readonly DateTimeOffset Now = new(2026, 11, 30, 12, 0, 0, TimeSpan.Zero);

        private readonly Mock<IProfessionalRepository> _repositoryMock;
        private readonly Mock<ISchedulingHoursRepository> _schedulingRepositoryMock;
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
        private readonly ProfessionalService _service;

        public ProfessionalServiceTests()
        {
            _repositoryMock = new Mock<IProfessionalRepository>();
            _schedulingRepositoryMock = new Mock<ISchedulingHoursRepository>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _dateTimeProviderMock.Setup(p => p.GetCurrentInstant()).Returns(Now);

            _service = new ProfessionalService(
                _repositoryMock.Object,
                _schedulingRepositoryMock.Object,
                _dateTimeProviderMock.Object);
        }

        private Professional GivenExistingProfessional(int id = 7)
        {
            var professional = new Professional("Carlos Silva", "carlos@email.com", "11977776666") { Id = id };
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(professional);
            return professional;
        }

        private List<SchedulingHours> GivenFutureAppointments(Professional professional, int count)
        {
            var service = new Service("Corte", "Corte masculino") { Id = 1 };
            var offering = new ProfessionalServiceOffering(professional, service, 50m, TimeSpan.FromMinutes(30));
            var customer = new Customer("Maria Souza", "maria@email.com", "11966665555") { Id = 1 };

            var appointments = Enumerable.Range(1, count)
                .Select(i => new SchedulingHours(customer, offering, Now.AddDays(i), Now.AddDays(i).AddMinutes(30)))
                .ToList();

            _schedulingRepositoryMock
                .Setup(r => r.GetFutureActiveByProfessionalAsync(professional.Id, Now))
                .ReturnsAsync(appointments);

            return appointments;
        }

        [Fact]
        public async Task DeleteAsync_WhenProfessionalDoesNotExist_ShouldReturnNotFoundFailure()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((Professional?)null);

            var result = await _service.DeleteAsync(7);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.Error.Type);
            _repositoryMock.Verify(r => r.DeactivateAsync(It.IsAny<Professional>(), It.IsAny<IReadOnlyCollection<SchedulingHours>>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithoutFutureAppointments_ShouldDeactivateAndAnonymizeWithoutConfirmation()
        {
            var professional = GivenExistingProfessional();
            _schedulingRepositoryMock
                .Setup(r => r.GetFutureActiveByProfessionalAsync(7, Now))
                .ReturnsAsync(new List<SchedulingHours>());

            var result = await _service.DeleteAsync(7);

            Assert.True(result.IsSuccess);
            Assert.Equal(Now, professional.DeletedAt);
            Assert.Equal("deleted-7@removed.invalid", professional.Email);
            Assert.Equal("Carlos Silva", professional.Name);
            _repositoryMock.Verify(r => r.DeactivateAsync(
                professional,
                It.Is<IReadOnlyCollection<SchedulingHours>>(a => a.Count == 0)), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithFutureAppointmentsAndNoConfirmation_ShouldReturnBusinessRuleFailureAndChangeNothing()
        {
            var professional = GivenExistingProfessional();
            var appointments = GivenFutureAppointments(professional, 2);

            var result = await _service.DeleteAsync(7, confirmCancellation: false);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
            Assert.Contains("2 future appointment(s)", result.Error.Message);
            Assert.Contains("confirm=true", result.Error.Message);
            Assert.Null(professional.DeletedAt);
            Assert.All(appointments, a => Assert.False(a.IsCanceled));
            _repositoryMock.Verify(r => r.DeactivateAsync(It.IsAny<Professional>(), It.IsAny<IReadOnlyCollection<SchedulingHours>>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithFutureAppointmentsAndConfirmation_ShouldCancelAllAndDeactivate()
        {
            var professional = GivenExistingProfessional();
            var appointments = GivenFutureAppointments(professional, 2);

            var result = await _service.DeleteAsync(7, confirmCancellation: true);

            Assert.True(result.IsSuccess);
            Assert.Equal(Now, professional.DeletedAt);
            Assert.All(appointments, a =>
            {
                Assert.True(a.IsCanceled);
                Assert.Equal(Now, a.CanceledAt);
                Assert.Equal(CancellationReason.ProfessionalDeactivated, a.CancellationReason);
            });
            _repositoryMock.Verify(r => r.DeactivateAsync(
                professional,
                It.Is<IReadOnlyCollection<SchedulingHours>>(a => a.Count == 2)), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldQueryFutureAppointmentsUsingCurrentInstant()
        {
            GivenExistingProfessional();
            _schedulingRepositoryMock
                .Setup(r => r.GetFutureActiveByProfessionalAsync(7, Now))
                .ReturnsAsync(new List<SchedulingHours>());

            await _service.DeleteAsync(7);

            _schedulingRepositoryMock.Verify(r => r.GetFutureActiveByProfessionalAsync(7, Now), Times.Once);
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