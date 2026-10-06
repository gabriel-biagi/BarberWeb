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
    
    public async Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset date)
    {
        var schedulingHours = await _repository.GetSchedulingHoursOfTheProfessionalByDay(professionalId, date);
        return schedulingHours;
    }
}