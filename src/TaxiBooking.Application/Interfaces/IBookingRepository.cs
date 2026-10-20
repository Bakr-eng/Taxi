using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetByUserIdAsync(string userId);
        Task<List<Booking>> GetByDriverIdAsync(int driverId);
        Task<Booking?> GetByIdAsync(int id);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(int id);
    }
}
