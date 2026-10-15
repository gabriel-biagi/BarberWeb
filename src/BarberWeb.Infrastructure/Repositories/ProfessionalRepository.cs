using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

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

    public async Task<Professional> CreateAsync(Professional professional)
    {
        try
        {
            _context.Professionals.Add(professional);
            await _context.SaveChangesAsync();
            return professional;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _context.Entry(professional).State = EntityState.Detached;
            throw new ConcurrencyDuplicateException(
                "A concurrent insert caused a unique constraint violation.", ex);
        }
        
    }
    
    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is MySqlException mySqlEx
           && mySqlEx.ErrorCode == MySqlErrorCode.DuplicateKeyEntry;
}