using BarberWeb.Application.Common;
using BarberWeb.Application.Common.Interfaces;
using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;
using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class ProfessionalService : IProfessionalService
{
    private readonly IProfessionalRepository _repository;
    private readonly ISchedulingHoursRepository _schedulingHoursRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ProfessionalService(
        IProfessionalRepository repository,
        ISchedulingHoursRepository schedulingHoursRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _schedulingHoursRepository = schedulingHoursRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ProfessionalResponse>> GetByIdAsync(int id)
    {
        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
            return Result<ProfessionalResponse>.Failure(ErrorType.NotFound, "Professional not found.");

        return Result<ProfessionalResponse>.Success(ToResponse(professional));
    }

    public async Task<Result<List<ProfessionalResponse>>> GetAllAsync()
    {
        var professionals = await _repository.GetAllAsync();

        return Result<List<ProfessionalResponse>>.Success(ToResponseList(professionals));
    }

    public async Task<Result<ProfessionalResponse>> CreateAsync(ProfessionalRequest professionalRequest)
    {
        var professional = ToEntity(professionalRequest);

        try
        {
            var created = await _repository.CreateAsync(professional);
            return Result<ProfessionalResponse>.Success(ToResponse(created));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<ProfessionalResponse>.Failure(ErrorType.Conflict, "Professional already exists.");
        }
    }

    public async Task<Result> DeleteAsync(int id, bool confirmCancellation = false)
    {
        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
            return Result.Failure(ErrorType.NotFound, "Professional not found.");

        var now = _dateTimeProvider.GetCurrentInstant();
        var futureAppointments = await _schedulingHoursRepository.GetFutureActiveByProfessionalAsync(id, now);

        if (futureAppointments.Count > 0 && !confirmCancellation)
        {
            return Result.Failure(
                ErrorType.BusinessRule,
                $"Professional has {futureAppointments.Count} future appointment(s) that will be canceled " +
                "if the professional is deactivated. Repeat the request with confirm=true to proceed.");
        }

        foreach (var appointment in futureAppointments)
            appointment.Cancel(now, CancellationReason.ProfessionalDeactivated);

        professional.Deactivate(now);

        await _repository.DeactivateAsync(professional, futureAppointments);
        return Result.Success();
    }

    // Métodos Auxiliares
    private static ProfessionalResponse ToResponse(Professional professional)
    {
        ArgumentNullException.ThrowIfNull(professional);
        return new ProfessionalResponse
        {
            Id = professional.Id,
            Name = professional.Name,
            Email = professional.Email
        };
    }

    private static List<ProfessionalResponse> ToResponseList(List<Professional> professionals)
    {
        ArgumentNullException.ThrowIfNull(professionals);
        return professionals.Select(ToResponse).ToList();
    }

    private static Professional ToEntity(ProfessionalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new Professional(
            request.Name,
            request.Email,
            request.PhoneNumber
        );
    }
}
