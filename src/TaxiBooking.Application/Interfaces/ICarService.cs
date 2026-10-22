using TaxiBooking.Application.DTOs;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface ICarService
    {
        Task<List<CarDto>> GetAllAsync();
        Task CreateAsync(CreateCarDto dto);
        Task AssignDriverAsync(int id, int? driverId);
        Task ToggleActiveAsync(int id);
        Task DeleteAsync(int id);
    }
}