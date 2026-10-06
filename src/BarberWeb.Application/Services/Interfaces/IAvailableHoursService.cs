using BarberWeb.Application.DTOs;

namespace BarberWeb.Application.Services.Interfaces;

public interface IAvailableHoursService
{
        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsByProfessionalAndDateAsync(int professionalId, DateTimeOffset date, int serviceId);
}