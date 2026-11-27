using BarberWeb.Application.Common;
using BarberWeb.Application.DTOs.Request;
using BarberWeb.Application.DTOs.Response;
using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class CatalogServiceService : ICatalogServiceService
{
    private const string NotFoundMessage = "Service not found.";
    private const string ConflictMessage = "Service already exists.";

    private readonly IServiceRepository _repository;

    public CatalogServiceService(IServiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CatalogServiceResponse>> GetByIdAsync(int id)
    {
        var service = await _repository.GetByIdAsync(id);
        if (service == null)
            return Result<CatalogServiceResponse>.Failure(ErrorType.NotFound, NotFoundMessage);

        return Result<CatalogServiceResponse>.Success(ToResponse(service));
    }

    public async Task<Result<List<CatalogServiceResponse>>> GetAllAsync()
    {
        var services = await _repository.GetAllAsync();

        return Result<List<CatalogServiceResponse>>.Success(services.Select(ToResponse).ToList());
    }

    public async Task<Result<CatalogServiceResponse>> CreateAsync(CatalogServiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var service = new Service(request.Name, request.Description);

        try
        {
            var created = await _repository.CreateAsync(service);
            return Result<CatalogServiceResponse>.Success(ToResponse(created));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<CatalogServiceResponse>.Failure(ErrorType.Conflict, ConflictMessage);
        }
    }

    public async Task<Result<CatalogServiceResponse>> UpdateAsync(int id, CatalogServiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var service = await _repository.GetByIdAsync(id);
        if (service == null)
            return Result<CatalogServiceResponse>.Failure(ErrorType.NotFound, NotFoundMessage);

        service.Update(request.Name, request.Description);

        try
        {
            await _repository.UpdateAsync(service);
            return Result<CatalogServiceResponse>.Success(ToResponse(service));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<CatalogServiceResponse>.Failure(ErrorType.Conflict, ConflictMessage);
        }
    }

    private static CatalogServiceResponse ToResponse(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return new CatalogServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description
        };
    }
}
