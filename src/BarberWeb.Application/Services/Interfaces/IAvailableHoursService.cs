using BarberWeb.Application.Common;
using BarberWeb.Application.DTOs;

namespace BarberWeb.Application.Services.Interfaces;

public interface IAvailableHoursService
{
    Task<Result<IEnumerable<AvailableSlotDto>>> GetAvailableSlotsByProfessionalAndDateAsync(int professionalId, DateOnly localDate, int serviceId);
}