using BarberWeb.Application.Common;
using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;

namespace BarberWeb.Application.Services.Interfaces;

public interface IProfessionalService
{
    Task<Result<ProfessionalResponse>> GetByIdAsync(int id);
    Task<Result<ProfessionalResponse>> CreateAsync(ProfessionalRequest professionalRequest);
    Task<List<ProfessionalResponse>> GetAllAsync();
    Task<Result<ProfessionalResponse>> DeleteAsync(int id);
}