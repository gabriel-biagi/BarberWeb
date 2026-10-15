using AutoMapper;
using BarberWeb.Application.DTOs.Response;
using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class ProfessionalService : IProfessionalService
{
    private readonly IProfessionalRepository _repository;
    private readonly IMapper _mapper;

    public ProfessionalService(IProfessionalRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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
        var professionalResponse = _mapper.Map<Professional, ProfessionalResponse>(professional);
        return professionalResponse;
    }
}