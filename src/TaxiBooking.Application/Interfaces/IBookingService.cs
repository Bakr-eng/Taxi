using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllAsync();
        Task<List<Booking>> GetByUserIdAsync(string userId);
        Task<List<Booking>> GetByDriverIdAsync(int driverId);
        Task<Booking?> GetByIdAsync(int id);

        Task<int> CreateAsync(
            string userId,
            string contactName,
            string contactPhone,
            string contactEmail,
            string pickup,
            string dropoff,
            DateTime pickupTime,
            bool hasAllergy,
            bool hasIllness,
            string? specialNeedsNote,
            string? message,
            decimal fare);

        Task<bool> UpdateStatusAsync(int id, BookingStatus status);
        Task<bool> SetPaymentAsync(int id, PaymentMethod method, bool isPaid, string? paymentReference = null);
        Task<bool> AssignDriverAsync(int bookingId, int driverId);
        Task DeleteAsync(int id);
    }
}
