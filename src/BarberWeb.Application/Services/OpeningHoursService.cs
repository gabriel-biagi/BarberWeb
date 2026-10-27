using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class OpeningHoursService(IOpeningHoursRepository repository) : IOpeningHoursService
{
    public async Task<IEnumerable<OpeningHours>> GetOpeningHoursOfTheProfessionalByDay(int professionalId, DayOfWeek dayOfWeek)
    {
        var openingHours = await repository.GetOpeningHoursOfTheProfessionalByDay(professionalId, dayOfWeek);
        return openingHours;
    }
}