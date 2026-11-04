using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class SchedulingHoursService : ISchedulingHoursService
{
    private readonly ISchedulingHoursRepository _repository;
    private readonly IOpeningHoursService _openingHoursService;

    public SchedulingHoursService(ISchedulingHoursRepository repository, IOpeningHoursService openingHoursService)
    {
        _repository = repository;
        _openingHoursService = openingHoursService;
    }
    
    public async Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset utcStart, DateTimeOffset utcEnd)
    {
        var schedulingHours = await _repository.GetSchedulingHoursOfTheProfessionalByDay(professionalId, utcStart, utcEnd);
        return schedulingHours;
    }
}