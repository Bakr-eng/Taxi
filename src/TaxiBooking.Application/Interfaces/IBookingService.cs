using TaxiBooking.Application.DTOs;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingDto>> GetAllAsync();
        Task<List<BookingDto>> GetByUserIdAsync(string userId);
        Task<List<BookingDto>> GetForDriverAsync(int driverId);
        Task<BookingDto?> GetByIdForUserAsync(int id, string userId);

        Task<int> CreateAsync(CreateBookingDto dto);
        Task<bool> CancelAsync(int id, string userId);
        Task<bool> CompleteAsync(int id, int driverId);
        Task<bool> UpdateStatusAsync(int id, BookingStatus status);
        Task<bool> SetPaymentAsync(int id, PaymentMethod method, bool isPaid, string? paymentReference = null);
        Task<bool> AssignDriverAsync(int bookingId, int driverId);
        Task DeleteAsync(int id);
    }
}