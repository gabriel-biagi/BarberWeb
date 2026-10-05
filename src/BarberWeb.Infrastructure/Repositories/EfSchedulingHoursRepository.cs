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

        public async Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset date)
        {
            DateTimeOffset StartOfTheDay = date.Date;
            DateTimeOffset StartOfTheNextDay = StartOfTheDay.AddDays(1);

                 var SchedulingHours = await _context.SchedulingHours.Include(s => s.Professional)
                .Where(s => s.ProfessionalId == professionalId)
                .Where(s => s.StartDate >= StartOfTheDay)
                .Where(s => s.StartDate < StartOfTheNextDay)
                .ToListAsync();

            return SchedulingHours;
        }
    }
}
