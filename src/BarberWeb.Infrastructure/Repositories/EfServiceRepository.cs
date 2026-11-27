using BarberWeb.Domain.Entities;
using BarberWeb.Domain.Exception;
using BarberWeb.Domain.Interfaces;
using BarberWeb.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace BarberWeb.Infrastructure.Repositories;

public class EfServiceRepository(AppDbContext context) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(int id)
    {
        // Com rastreamento: UpdateAsync depende disso para detectar as alterações.
        return await context.Services.FirstOrDefaultAsync(service => service.Id == id);
    }

    public async Task<List<Service>> GetAllAsync()
    {
        return await context.Services.AsNoTracking().OrderBy(service => service.Name).ToListAsync();
    }

    public async Task<Service> CreateAsync(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);
        try
        {
            context.Services.Add(service);
            await context.SaveChangesAsync();
            return service;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            context.Entry(service).State = EntityState.Detached;
            throw new UniqueConstraintViolationException("A service with the same name already exists.", ex);
        }
    }

    public async Task UpdateAsync(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Descarta a alteração rejeitada para não contaminar outro SaveChanges do mesmo contexto.
            await context.Entry(service).ReloadAsync();
            throw new UniqueConstraintViolationException("A service with the same name already exists.", ex);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is MySqlException mySqlEx
           && mySqlEx.Number == 1062;
}
