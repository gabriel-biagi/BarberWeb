using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Domain.Interfaces;

namespace BarberWeb.Application.Services;

public class ProfessionalServiceOfferingService : IProfessionalServiceOfferingService
{
    private readonly IProfessionalServiceOfferingRepository _repository;

    public ProfessionalServiceOfferingService(IProfessionalServiceOfferingRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<TimeSpan?> GetTimeSpanByServiceIdAndProfessionalId(int serviceId, int professionalId)
    {
        TimeSpan? timeSpan = await _repository.GetTimeSpanByServiceIdAndProfessionalId(professionalId, serviceId);
        return timeSpan;
    }
}