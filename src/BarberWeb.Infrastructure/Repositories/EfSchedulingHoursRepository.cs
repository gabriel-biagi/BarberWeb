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
                .Where(s => s.StartDate >= utcStart)
                .Where(s => s.StartDate < utcEnd)
                .ToListAsync();

            return schedulingHours;
        }
    }
}