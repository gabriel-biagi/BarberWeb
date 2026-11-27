using BarberWeb.Application.Common;
using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;

namespace BarberWeb.Application.Services.Interfaces;

/// <summary>
/// Casos de uso do catálogo de serviços (entidade <c>Service</c>).
/// Não há exclusão por enquanto: a política de exclusão de Service ainda está pendente.
/// </summary>
public interface ICatalogServiceService
{
    Task<Result<CatalogServiceResponse>> GetByIdAsync(int id);
    Task<Result<List<CatalogServiceResponse>>> GetAllAsync();
    Task<Result<CatalogServiceResponse>> CreateAsync(CatalogServiceRequest request);

    /// <summary>Substitui nome e descrição do serviço.</summary>
    Task<Result<CatalogServiceResponse>> UpdateAsync(int id, CatalogServiceRequest request);
}
