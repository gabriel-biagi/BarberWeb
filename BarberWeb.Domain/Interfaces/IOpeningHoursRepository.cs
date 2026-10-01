using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces
{
    public interface IOpeningHoursRepository
    {
        Task<IEnumerable<OpeningHours>> GetOpeningHoursOfTheProfessionalByDay(int professionalId, DayOfWeek dayOfWeek);
    }
}
