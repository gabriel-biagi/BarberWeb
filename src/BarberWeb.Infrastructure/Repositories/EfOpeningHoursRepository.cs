using BarberWeb.Domain.Interfaces;
using BarberWeb.Domain.Entities;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BarberWeb.Infrastructure.Repositories
{
    public class EfOpeningHoursRepository : IOpeningHoursRepository
    {
        private readonly AppDbContext _context;

        public EfOpeningHoursRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OpeningHours>> GetOpeningHoursOfTheProfessionalByDay(int professionalId, DayOfWeek dayOfWeek)
        {
            var openingHours = await _context.OpeningHours.Include(o => o.Professional)
                .Where(o => o.ProfessionalId == professionalId)
                .Where(o => o.DayOfWeek == dayOfWeek)
                .ToListAsync();

            return openingHours;
        }
    }
}
