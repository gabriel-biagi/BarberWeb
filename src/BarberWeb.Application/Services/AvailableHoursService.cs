using BarberWeb.Application.DTOs;
using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Exception;

namespace BarberWeb.Application.Services;

public class AvailableHoursService : IAvailableHoursService
{
    private readonly IOpeningHoursService _openingHoursService;
    private readonly ISchedulingHoursService _schedulingHoursService;
    private readonly IProfessionalServiceOfferingService  _professionalServiceOfferingService;

    public AvailableHoursService(IOpeningHoursService openingHoursService,
        ISchedulingHoursService schedulingHoursService, IProfessionalServiceOfferingService professionalServiceOfferingService)
    {
        _openingHoursService = openingHoursService;
        _schedulingHoursService = schedulingHoursService;
        _professionalServiceOfferingService = professionalServiceOfferingService;
    }
    
    public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsByProfessionalAndDateAsync(int professionalId, DateTimeOffset date, int serviceId)
    {
        DayOfWeek dayOfWeek = date.DayOfWeek;
        var openingHours = await _openingHoursService.GetOpeningHoursOfTheProfessionalByDay(professionalId, dayOfWeek);
        var schedulingHours = await _schedulingHoursService.GetSchedulingHoursOfTheProfessionalByDay(professionalId, date);
        TimeSpan? professionalServiceOffering = await _professionalServiceOfferingService.GetTimeSpanByServiceIdAndProfessionalId(serviceId, professionalId);
        if (professionalServiceOffering is null)
        {
            throw new BusinessException("The professional does not have a defined duration for the specific Service");
        }

        List<AvailableSlotDto> availableSlotsDto = new List<AvailableSlotDto>();
        
        foreach (var bloco in openingHours)
        {
            TimeOnly start = bloco.StartTime;
            TimeOnly end = bloco.EndTime;
            
            while (start.Add(professionalServiceOffering.Value) <= bloco.EndTime)
            {
                availableSlotsDto.Add(start);
                end = end.Add(professionalServiceOffering.Value);
                start = start.Add(professionalServiceOffering.Value);
            }
        }
        
        
    }
}