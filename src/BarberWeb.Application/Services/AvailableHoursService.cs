using BarberWeb.Application.Common;
using BarberWeb.Application.Common.Interfaces;
using BarberWeb.Application.DTOs;
using BarberWeb.Application.Services.Interfaces;

namespace BarberWeb.Application.Services;

public class AvailableHoursService : IAvailableHoursService
{
    private readonly IOpeningHoursService _openingHoursService;
    private readonly ISchedulingHoursService _schedulingHoursService;
    private readonly IProfessionalServiceOfferingService _professionalServiceOfferingService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AvailableHoursService(
        IOpeningHoursService openingHoursService,
        ISchedulingHoursService schedulingHoursService,
        IProfessionalServiceOfferingService professionalServiceOfferingService,
        IDateTimeProvider dateTimeProvider)
    {
        _openingHoursService = openingHoursService;
        _schedulingHoursService = schedulingHoursService;
        _professionalServiceOfferingService = professionalServiceOfferingService;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public async Task<Result<IEnumerable<AvailableSlotDto>>> GetAvailableSlotsByProfessionalAndDateAsync(int professionalId, DateOnly localDate, int serviceId)
    {
        DayOfWeek dayOfWeek = localDate.DayOfWeek;
        
        // 1. Obtém a faixa UTC exata para o dia local usando o Provider (respeitando fuso e horário de verão)
        var (utcStart, utcEnd) = _dateTimeProvider.GetUtcRangeForLocalDay(localDate);

        var openingHours = await _openingHoursService.GetOpeningHoursOfTheProfessionalByDay(professionalId, dayOfWeek);
        
        // 2. Passa a faixa UTC para o repositório buscar os agendamentos corretos
        var schedulingHours = await _schedulingHoursService.GetSchedulingHoursOfTheProfessionalByDay(professionalId, utcStart, utcEnd);
        
        TimeSpan? professionalServiceOffering = await _professionalServiceOfferingService.GetTimeSpanByServiceIdAndProfessionalId(serviceId, professionalId);
        if (professionalServiceOffering is null)
        {
            return Result<IEnumerable<AvailableSlotDto>>.Failure(ErrorType.BusinessRule,
                "The professional does not have a defined duration for the specific Service");
        }

        List<AvailableSlotDto> availableSlotsDto = new List<AvailableSlotDto>();
        TimeSpan duration = professionalServiceOffering.Value;
        
        foreach (var bloco in openingHours)
        {
            TimeOnly currentStart = bloco.StartTime;
            string professionalName = bloco.Professional.Name;
            
            while (currentStart.Add(duration) <= bloco.EndTime)
            {
                TimeOnly currentEnd = currentStart.Add(duration);

                // 3. Converte os horários UTC salvos no banco de volta para o horário local da barbearia para comparar
                bool isOccupied = schedulingHours.Any(s => 
                {
                    var appointmentStartLocal = _dateTimeProvider.ConvertUtcToLocalTime(s.StartDate);
                    var appointmentEndLocal = _dateTimeProvider.ConvertUtcToLocalTime(s.EndDate);

                    return currentStart < appointmentEndLocal && currentEnd > appointmentStartLocal;
                });

                if (!isOccupied)
                {
                    availableSlotsDto.Add(new AvailableSlotDto(currentStart, currentEnd, professionalName));
                }

                currentStart = currentEnd;
            }
        } 
        
        return Result<IEnumerable<AvailableSlotDto>>.Success(availableSlotsDto);
    }
}