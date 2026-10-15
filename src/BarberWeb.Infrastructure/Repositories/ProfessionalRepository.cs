using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Interfaces;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BarberWeb.Infrastructure.Repositories;

public class ProfessionalRepository : IProfessionalRepository
{
    private readonly AppDbContext _context;

    public ProfessionalRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Professional?> GetByIdAsync(int id)
    {
        return await _context.Professionals.Where(professional => professional.Id == id).AsNoTracking().FirstOrDefaultAsync();
    }
}