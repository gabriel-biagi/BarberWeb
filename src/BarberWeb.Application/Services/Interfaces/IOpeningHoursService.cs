using BarberWeb.Domain.Entities;

namespace BarberWeb.Application.Services.Interfaces;

public interface IOpeningHoursService
{
    Task<IEnumerable<OpeningHours>> GetOpeningHoursOfTheProfessionalByDay(int professionalId, DayOfWeek dayOfWeek);
}