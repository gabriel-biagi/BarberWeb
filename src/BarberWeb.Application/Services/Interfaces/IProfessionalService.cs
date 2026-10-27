using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;

namespace BarberWeb.Application.Services.Interfaces;

public interface IProfessionalService
{
    Task<ProfessionalResponse> GetByIdAsync(int id);
    Task<ProfessionalResponse> CreateAsync(ProfessionalRequest professionalRequest);
    Task<List<ProfessionalResponse>> GetAllAsync();
    Task DeleteAsync(int id);
}