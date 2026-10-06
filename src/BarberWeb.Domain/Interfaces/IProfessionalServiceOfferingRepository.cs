using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces;

public interface IProfessionalServiceOfferingRepository
{
    Task<TimeSpan?> GetTimeSpanByServiceIdAndProfessionalId(int serviceId, int professionalId);
}