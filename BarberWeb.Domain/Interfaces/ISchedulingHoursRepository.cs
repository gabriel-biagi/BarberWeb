using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces
{
    public interface ISchedulingHoursRepository
    {
        Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset date);
    }
}
