using BarberWeb.Application.DTOs.Response;

namespace BarberWeb.Application.Services.Interfaces;

public interface IProfessionalService
{
    Task<ProfessionalResponse> GetByIdAsync(int id);
}