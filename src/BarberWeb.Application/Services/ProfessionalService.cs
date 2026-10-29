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
        if (id <= 0)
            return Result<ProfessionalResponse>.Failure(ErrorTypes.InvalidEntry,"Invalid professional id.");

        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
            return Result<ProfessionalResponse>.Failure(ErrorTypes.NotFound,"Professional not found.");
        
        var response = ToResponse(professional);
        return Result<ProfessionalResponse>.Success(response);
    }

    public async Task<List<ProfessionalResponse>> GetAllAsync()
    {
        var professionals = await _repository.GetAllAsync();

        return  ToResponseList(professionals);
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
            return Result<ProfessionalResponse>.Failure(ErrorTypes.Conflict,"Professional already exists.");
        }
    }
    
    public async Task<Result<ProfessionalResponse>> DeleteAsync(int id)
    {
        if (id <= 0)
            return Result<ProfessionalResponse>.Failure(ErrorTypes.InvalidEntry,"Invalid professional id.");

        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
            return Result<ProfessionalResponse>.Failure(ErrorTypes.NotFound,"Professional not found.");
        
        await _repository.DeleteAsync(professional);
        return Result<ProfessionalResponse>.Success(null);
    }

    //Métodos Auxiliares
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