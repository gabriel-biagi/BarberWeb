using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace BarberWeb.Infrastructure.Repositories;

public class EfProfessionalRepository(AppDbContext context) : IProfessionalRepository
{
    public async Task<Professional?> GetByIdAsync(int id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await context.Professionals.Where(professional => professional.Id == id).AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<Professional> CreateAsync(Professional professional)
    {
        ArgumentNullException.ThrowIfNull(professional);
        try
        {
            context.Professionals.Add(professional);
            await context.SaveChangesAsync();
            return professional;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            context.Entry(professional).State = EntityState.Detached;
            throw new UniqueConstraintViolationException(
                "A concurrent insert caused a unique constraint violation.", ex);
        }
        
    }

    public async Task<List<Professional>> GetAllAsync()
    {
        return await context.Professionals.AsNoTracking().ToListAsync();
    }

    public async Task DeleteAsync(Professional professional)
    {
        ArgumentNullException.ThrowIfNull(professional);
        context.Professionals.Remove(professional);
        await context.SaveChangesAsync();
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is MySqlException mySqlEx
           && mySqlEx.Number == 1062;
}