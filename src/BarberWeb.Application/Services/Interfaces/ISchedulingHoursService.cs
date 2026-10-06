using BarberWeb.Domain.Entities;

namespace BarberWeb.Application.Services.Interfaces;

public interface ISchedulingHoursService
{
    Task<IEnumerable<SchedulingHours>> GetSchedulingHoursOfTheProfessionalByDay(int professionalId, DateTimeOffset date);
}