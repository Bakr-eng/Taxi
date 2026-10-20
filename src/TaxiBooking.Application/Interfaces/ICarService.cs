using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface ICarService
    {
        Task<List<Car>> GetAllAsync();
        Task<Car?> GetByIdAsync(int id);
        Task CreateAsync(string registrationNumber, string make, string model, int year, int seats, int? driverId = null);
        Task UpdateAsync(Car car);
        Task DeleteAsync(int id);
    }
}