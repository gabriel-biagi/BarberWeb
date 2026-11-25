using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces;

public interface IProfessionalRepository
{
    Task<Professional?> GetByIdAsync(int id);
    Task<Professional> CreateAsync(Professional professional);
    Task<List<Professional>> GetAllAsync();

    /// <summary>
    /// Persiste, em uma única transação, a desativação do profissional (já aplicada na entidade)
    /// e o cancelamento dos agendamentos informados (já aplicado nas entidades).
    /// Os agendamentos devem ter sido carregados com rastreamento pelo mesmo contexto de persistência,
    /// por exemplo, via <see cref="ISchedulingHoursRepository.GetFutureActiveByProfessionalAsync"/>.
    /// </summary>
    Task DeactivateAsync(Professional professional, IReadOnlyCollection<SchedulingHours> canceledAppointments);
}
