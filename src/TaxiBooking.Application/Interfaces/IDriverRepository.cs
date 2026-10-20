using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IDriverRepository
    {
        Task<List<Driver>> GetAllAsync();
        Task<Driver?> GetByIdAsync(int id);
        Task<Driver?> GetByUserIdAsync(string userId);
        Task AddAsync(Driver driver);
        Task UpdateAsync(Driver driver);
        Task DeleteAsync(int id);
    }
}
