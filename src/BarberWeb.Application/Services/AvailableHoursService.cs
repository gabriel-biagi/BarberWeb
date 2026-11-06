
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

    public async Task<Result<IEnumerable<AvailableSlotDto>>> GetAvailableSlotsByProfessionalAndDateAsync(
        int professionalId,
        DateOnly localDate,
        int serviceId)
    {
        DayOfWeek dayOfWeek = localDate.DayOfWeek;

        // 1. Obtém a faixa UTC exata para o dia local.
        var (utcStart, utcEnd) = _dateTimeProvider.GetUtcRangeForLocalDay(localDate);

        var openingHours = await _openingHoursService
            .GetOpeningHoursOfTheProfessionalByDay(professionalId, dayOfWeek);

        // 2. Busca os agendamentos que ocupam o dia consultado.
        var schedulingHours = await _schedulingHoursService
            .GetSchedulingHoursOfTheProfessionalByDay(professionalId, utcStart, utcEnd);

        TimeSpan? professionalServiceOffering =
            await _professionalServiceOfferingService
                .GetTimeSpanByServiceIdAndProfessionalId(serviceId, professionalId);

        if (professionalServiceOffering is null)
        {
            return Result<IEnumerable<AvailableSlotDto>>.Failure(
                ErrorType.BusinessRule,
                "The professional does not have a defined duration for the specific Service");
        }

        List<AvailableSlotDto> availableSlotsDto = new();

        TimeSpan duration = professionalServiceOffering.Value;

        foreach (var bloco in openingHours)
        {
            TimeOnly currentStart = bloco.StartTime;
            string professionalName = bloco.Professional.Name;

            while (currentStart.Add(duration) <= bloco.EndTime)
            {
                TimeOnly currentEnd = currentStart.Add(duration);

                // 3. Converte o intervalo candidato para UTC,
                // preservando a data e o fuso da barbearia.
                DateTimeOffset candidateStartUtc =
                    _dateTimeProvider.ConvertLocalToUtc(localDate, currentStart);

                DateTimeOffset candidateEndUtc =
                    _dateTimeProvider.ConvertLocalToUtc(localDate, currentEnd);

                // 4. Compara os intervalos completos em UTC.
                bool isOccupied = schedulingHours.Any(s =>
                    s.StartDate < candidateEndUtc &&
                    s.EndDate > candidateStartUtc);

                if (!isOccupied)
                {
                    availableSlotsDto.Add(
                        new AvailableSlotDto(
                            currentStart,
                            currentEnd,
                            professionalName));
                }

                currentStart = currentEnd;
            }
        }

        return Result<IEnumerable<AvailableSlotDto>>.Success(availableSlotsDto);
    }
}
