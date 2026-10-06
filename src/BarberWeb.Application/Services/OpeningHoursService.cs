using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class OpeningHoursService : IOpeningHoursService
{
    private readonly IOpeningHoursRepository _repository;

    public OpeningHoursService(IOpeningHoursRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<OpeningHours>> GetOpeningHoursOfTheProfessionalByDay(int professionalId, DayOfWeek dayOfWeek)
    {
        var openingHours = await _repository.GetOpeningHoursOfTheProfessionalByDay(professionalId, dayOfWeek);
        return openingHours;
    }
}