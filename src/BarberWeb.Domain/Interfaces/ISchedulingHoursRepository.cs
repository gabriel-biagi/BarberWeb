using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces
{
    public interface ISchedulingHoursRepository
    {
        /// <summary>Agendamentos não cancelados do profissional que começam dentro do intervalo UTC informado.</summary>
        Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset utcStart, DateTimeOffset utcEnd);

        /// <summary>
        /// Agendamentos não cancelados do profissional com <c>StartDate</c> posterior a <paramref name="now"/>.
        /// As entidades retornadas são rastreadas pelo contexto, para que o cancelamento possa ser persistido.
        /// </summary>
        Task<IReadOnlyList<SchedulingHours>> GetFutureActiveByProfessionalAsync(int professionalId, DateTimeOffset now);
    }
}
