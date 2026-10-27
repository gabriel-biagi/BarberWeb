
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
    
    public async Task<ProfessionalResponse> GetByIdAsync(int id)
    {
        TriggerExceptionIfIdIsInvalid(id);

        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
        {
            throw new NotFoundException("Professional not found");
        }
        return ToResponse(professional);
    }

    public async Task<List<ProfessionalResponse>> GetAllAsync()
    {
        var professionals = await _repository.GetAllAsync();

        return  ToResponseList(professionals);
    }
    
    public async Task<ProfessionalResponse> CreateAsync(ProfessionalRequest professionalRequest)
    {
        var professional = ToEntity(professionalRequest);

        try
        {
            var created = await _repository.CreateAsync(professional);
            return ToResponse(created);
        }
        catch (UniqueConstraintViolationException)
        {
            throw new BusinessException("Email or phone already registered.");
        }
    }
    
    public async Task DeleteAsync(int id)
    {
        TriggerExceptionIfIdIsInvalid(id);
        
        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
            throw new NotFoundException("Professional not found.");
        
        await _repository.DeleteAsync(professional);
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

    private static void TriggerExceptionIfIdIsInvalid(int id)
    {
        if (id <= 0)
        {
            throw new InvalidInputException("Professional code must be greater than 0");
        }
    }
}