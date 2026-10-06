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
        TimeSpan duration = professionalServiceOffering.Value;

        foreach (var bloco in openingHours)
        {
            TimeOnly currentStart = bloco.StartTime;
            while (currentStart.Add(duration) <= bloco.EndTime)
            {
                TimeOnly currentEnd = currentStart.Add(duration);

                bool isOccupied = schedulingHours.Any(s => currentStart < TimeOnly.FromDateTime(s.EndDate.DateTime)
                                                           && currentEnd > TimeOnly.FromDateTime(s.StartDate.DateTime));

                if (!isOccupied)
                {
                    availableSlotsDto.Add(new AvailableSlotDto(currentStart, currentEnd));
                }

                currentStart = currentEnd;
            }
        }
        
        return availableSlotsDto;
    }
}