using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using Xunit;

namespace BarberWeb.Domain.Tests.Entities
{
    public class SchedulingHoursCancellationTests
    {
        private static readonly DateTimeOffset Now = new(2026, 11, 30, 12, 0, 0, TimeSpan.Zero);

        private static SchedulingHours NewAppointment()
        {
            var professional = new Professional("Carlos Silva", "carlos@email.com", "11977776666") { Id = 1 };
            var service = new Service("Corte", "Corte masculino") { Id = 1 };
            var offering = new ProfessionalServiceOffering(professional, service, 50m, TimeSpan.FromMinutes(30));
            var customer = new Customer("Maria Souza", "maria@email.com", "11966665555") { Id = 1 };

            return new SchedulingHours(customer, offering, Now.AddDays(1), Now.AddDays(1).AddMinutes(30));
        }

        [Fact]
        public void NewAppointment_ShouldNotBeCanceled()
        {
            var appointment = NewAppointment();

            Assert.False(appointment.IsCanceled);
            Assert.Null(appointment.CanceledAt);
            Assert.Null(appointment.CancellationReason);
        }

        [Fact]
        public void Cancel_ShouldRegisterInstantAndReason()
        {
            var appointment = NewAppointment();

            appointment.Cancel(Now, CancellationReason.ProfessionalDeactivated);

            Assert.True(appointment.IsCanceled);
            Assert.Equal(Now, appointment.CanceledAt);
            Assert.Equal(CancellationReason.ProfessionalDeactivated, appointment.CancellationReason);
        }

        [Fact]
        public void Cancel_WhenAlreadyCanceled_ShouldThrowBusinessExceptionAndKeepOriginalData()
        {
            var appointment = NewAppointment();
            appointment.Cancel(Now, CancellationReason.ProfessionalDeactivated);

            var exception = Assert.Throws<BusinessException>(
                () => appointment.Cancel(Now.AddHours(1), CancellationReason.ProfessionalDeactivated));

            Assert.Equal("Appointment is already canceled.", exception.Message);
            Assert.Equal(Now, appointment.CanceledAt);
        }
    }
}
