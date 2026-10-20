using Microsoft.Extensions.Logging;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IBookingRepository repo, ILogger<BookingService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<Booking>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<List<Booking>> GetByUserIdAsync(string userId)
            => await _repo.GetByUserIdAsync(userId);

        public async Task<List<Booking>> GetByDriverIdAsync(int driverId)
            => await _repo.GetByDriverIdAsync(driverId);

        public async Task<Booking?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<int> CreateAsync(
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
            decimal fare)
        {
            var booking = new Booking
            {
                UserId = userId,
                ContactName = contactName,
                ContactPhone = contactPhone,
                ContactEmail = contactEmail,
                PickupAddress = pickup,
                DropoffAddress = dropoff,
                PickupTime = pickupTime,
                HasAllergy = hasAllergy,
                HasIllness = hasIllness,
                SpecialNeedsNote = specialNeedsNote,
                Message = message,
                EstimatedFare = fare,
                RequestedAt = DateTime.UtcNow,
                Status = BookingStatus.Pending
            };
            await _repo.AddAsync(booking);
            _logger.LogInformation("Booking created for user {UserId}, pickup {Pickup}", userId, pickup);
            return booking.Id;
        }

        public async Task<bool> SetPaymentAsync(int id, PaymentMethod method, bool isPaid, string? paymentReference = null)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null)
            {
                _logger.LogWarning("Attempted to set payment on non-existent booking {BookingId}", id);
                return false;
            }
            booking.PaymentMethod = method;
            booking.IsPaid = isPaid;
            booking.PaidAt = isPaid ? DateTime.UtcNow : null;
            booking.PaymentReference = paymentReference;
            await _repo.UpdateAsync(booking);
            _logger.LogInformation("Booking {BookingId} payment set to {Method} (paid: {IsPaid})", id, method, isPaid);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, BookingStatus status)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null)
            {
                _logger.LogWarning("Attempted to update status on non-existent booking {BookingId}", id);
                return false;
            }
            booking.Status = status;
            await _repo.UpdateAsync(booking);
            _logger.LogInformation("Booking {BookingId} status changed to {Status}", id, status);
            return true;
        }

        public async Task<bool> AssignDriverAsync(int bookingId, int driverId)
        {
            var booking = await _repo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Attempted to assign driver to non-existent booking {BookingId}", bookingId);
                return false;
            }
            booking.DriverId = driverId;
            booking.Status = BookingStatus.Confirmed;
            await _repo.UpdateAsync(booking);
            _logger.LogInformation("Driver {DriverId} assigned to booking {BookingId}", driverId, bookingId);
            return true;
        }

        public async Task DeleteAsync(int id)
            => await _repo.DeleteAsync(id);
    }
}
