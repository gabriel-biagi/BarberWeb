using BarberWeb.Domain.Entities;

namespace BarberWeb.Domain.Interfaces;

public interface IProfessionalRepository
{
    Task<Professional?> GetByIdAsync(int id);
}