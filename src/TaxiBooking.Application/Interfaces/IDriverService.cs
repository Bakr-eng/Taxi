using TaxiBooking.Application.DTOs;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IDriverService
    {
        Task<List<DriverDto>> GetAllAsync();
        Task<List<DriverDto>> GetAvailableAsync();
        Task<DriverDto?> GetByIdAsync(int id);
        Task<DriverDto?> GetByUserIdAsync(string userId);
        Task CreateAsync(CreateDriverDto dto);
        Task ToggleAvailabilityAsync(int id);
        Task DeleteAsync(int id);
    }
}
