using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces;

public interface IServiceRepository
{
    /// <summary>
    /// Busca o serviço com rastreamento, para que alterações feitas na entidade
    /// possam ser persistidas por <see cref="UpdateAsync"/>.
    /// </summary>
    Task<Service?> GetByIdAsync(int id);

    Task<List<Service>> GetAllAsync();

    /// <exception cref="Exception.UniqueConstraintViolationException">Já existe um serviço com o mesmo nome.</exception>
    Task<Service> CreateAsync(Service service);

    /// <summary>Persiste as alterações de um serviço carregado por <see cref="GetByIdAsync"/>.</summary>
    /// <exception cref="Exception.UniqueConstraintViolationException">Já existe outro serviço com o mesmo nome.</exception>
    Task UpdateAsync(Service service);
}
