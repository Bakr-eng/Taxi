using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface ICarRepository
    {
        Task<List<Car>> GetAllAsync();
        Task<Car?> GetByIdAsync(int id);
        Task AddAsync(Car car);
        Task UpdateAsync(Car car);
        Task DeleteAsync(int id);
    }
}