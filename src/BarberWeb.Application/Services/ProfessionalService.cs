using BarberWeb.Application.Common;
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

    public ProfessionalService(IProfessionalRepository repository)
    {
        _repository = repository;
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

    public async Task<Result> DeleteAsync(int id)
    {
        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
            return Result.Failure(ErrorType.NotFound, "Professional not found.");

        await _repository.DeleteAsync(professional);
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
