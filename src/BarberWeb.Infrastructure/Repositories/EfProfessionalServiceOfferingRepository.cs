using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Interfaces;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BarberWeb.Infrastructure.Repositories;

public class EfProfessionalServiceOfferingRepository : IProfessionalServiceOfferingRepository
{
    private readonly AppDbContext _context;

    public EfProfessionalServiceOfferingRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<TimeSpan?> GetTimeSpanByServiceIdAndProfessionalId(int serviceId, int professionalId)
    {
        TimeSpan? timeSpan = await _context.ProfessionalOfferings
            .Where(p => p.ProfessionalId == professionalId && p.ServiceId == serviceId)
            .Select(p => (TimeSpan?)p.Time)
            .FirstOrDefaultAsync();

        return timeSpan;
    }
}