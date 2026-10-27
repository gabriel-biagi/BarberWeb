
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
        if (id <= 0)
        {
            throw new InvalidInputException("Professional code must be greater than 0");
        }

        var professional = await _repository.GetByIdAsync(id);
        if (professional == null)
        {
            throw new NotFoundException("Professional not found");
        }
        return ToResponse(professional);
    }

    public async Task<ProfessionalResponse> CreateAsync(ProfessionalRequest professionalRequest)
    {
        var professional = new Professional(
            professionalRequest.Name,
            professionalRequest.Email,
            professionalRequest.PhoneNumber);

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
    
    private static ProfessionalResponse ToResponse(Professional professional)
    {
        return new ProfessionalResponse
        {
            Name = professional.Name,
            Email = professional.Email
        };
    }
}