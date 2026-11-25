using BarberWeb.Application.Common;
using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;

namespace BarberWeb.Application.Services.Interfaces;

public interface IProfessionalService
{
    Task<Result<ProfessionalResponse>> GetByIdAsync(int id);
    Task<Result<ProfessionalResponse>> CreateAsync(ProfessionalRequest professionalRequest);
    Task<Result<List<ProfessionalResponse>>> GetAllAsync();
    /// <summary>
    /// Desativa o profissional (exclusão lógica, permanente). Se houver agendamentos futuros, a operação
    /// só prossegue com <paramref name="confirmCancellation"/> = true, e esses agendamentos são cancelados.
    /// </summary>
    Task<Result> DeleteAsync(int id, bool confirmCancellation = false);
}
