using AutoMapper;
using BarberWeb.Application.DTOs.Response;
using BarberWeb.Domain.Entities;

namespace BarberWeb.Application.DTOs.Mappings;

public class ProfessionalResponseMappingProfile : Profile
{
    public ProfessionalResponseMappingProfile()
    {
        CreateMap<Professional, ProfessionalResponse>();
    }
}