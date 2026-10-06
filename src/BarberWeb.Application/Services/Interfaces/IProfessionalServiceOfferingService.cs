namespace BarberWeb.Application.Services.Interfaces;

public interface IProfessionalServiceOfferingService
{
    Task<TimeSpan?> GetTimeSpanByServiceIdAndProfessionalId(int serviceId, int professionalId);
}