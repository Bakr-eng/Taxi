using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IDriverService
    {
        Task<List<Driver>> GetAllAsync();
        Task<Driver?> GetByIdAsync(int id);
        Task<Driver?> GetByUserIdAsync(string userId);
        Task CreateAsync(string name, string phone, string license, string? userId = null);
        Task UpdateAsync(Driver driver);
        Task DeleteAsync(int id);
    }
}
