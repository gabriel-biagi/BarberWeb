using BarberWeb.Domain.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BarberWeb.Infrastructure.Repositories
{
    public class EfSchedulingHoursRepository : ISchedulingHoursRepository
    {
        private readonly AppDbContext _context;

        public EfSchedulingHoursRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset utcStart, DateTimeOffset utcEnd)
        {
            var schedulingHours = await _context.Appointments.Include(s => s.ProfessionalServiceOffering)
                .Where(s => s.ProfessionalId == professionalId)
                .Where(s => s.CanceledAt == null)
                .Where(s => s.StartDate >= utcStart)
                .Where(s => s.StartDate < utcEnd)
                .ToListAsync();

            return schedulingHours;
        }

        public async Task<IReadOnlyList<SchedulingHours>> GetFutureActiveByProfessionalAsync(int professionalId, DateTimeOffset now)
        {
            // Sem AsNoTracking: as entidades precisam ser rastreadas para que Cancel() seja persistido.
            return await _context.Appointments
                .Where(s => s.ProfessionalId == professionalId)
                .Where(s => s.CanceledAt == null)
                .Where(s => s.StartDate > now)
                .ToListAsync();
        }
    }
}