using Microsoft.Extensions.Logging;
using TaxiBooking.Application.DTOs;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Application.Mappings;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;
        private readonly IFareCalculator _fareCalculator;
        private readonly ILogger<BookingService> _logger;

        public BookingService(IBookingRepository repo, IFareCalculator fareCalculator, ILogger<BookingService> logger)
        {
            _repo = repo;
            _fareCalculator = fareCalculator;
            _logger = logger;
        }

        public async Task<List<BookingDto>> GetAllAsync()
            => (await _repo.GetAllAsync())
                .OrderByDescending(b => b.RequestedAt)
                .Select(b => b.ToDto())
                .ToList();

        public async Task<List<BookingDto>> GetByUserIdAsync(string userId)
            => (await _repo.GetByUserIdAsync(userId))
                .OrderByDescending(b => b.RequestedAt)
                .Select(b => b.ToDto())
                .ToList();

        public async Task<List<BookingDto>> GetForDriverAsync(int driverId)
            => (await _repo.GetByDriverIdAsync(driverId))
                .Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Completed)
                .OrderBy(b => b.PickupTime)
                .Select(b => b.ToDto())
                .ToList();

        public async Task<BookingDto?> GetByIdForUserAsync(int id, string userId)
        {
            var booking = await _repo.GetByIdAsync(id);
            return booking?.UserId == userId ? booking.ToDto() : null;
        }

        public async Task<int> CreateAsync(CreateBookingDto dto)
        {
            var fare = _fareCalculator.Estimate(dto.PickupAddress, dto.DropoffAddress, dto.DistanceKm, dto.DurationMinutes);
            var booking = dto.ToEntity(fare);
            await _repo.AddAsync(booking);
            _logger.LogInformation("Booking created for user {UserId}, pickup {Pickup}", dto.UserId, dto.PickupAddress);
            return booking.Id;
        }

        public async Task<bool> CancelAsync(int id, string userId)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null || booking.UserId != userId || booking.Status != BookingStatus.Pending)
                return false;

            booking.Status = BookingStatus.Cancelled;
            await _repo.UpdateAsync(booking);
            _logger.LogInformation("Booking {BookingId} cancelled by user", id);
            return true;
        }

        public async Task<bool> CompleteAsync(int id, int driverId)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null || booking.DriverId != driverId || booking.Status != BookingStatus.Confirmed)
                return false;

            booking.Status = BookingStatus.Completed;
            await _repo.UpdateAsync(booking);
            _logger.LogInformation("Booking {BookingId} completed by driver {DriverId}", id, driverId);
            return true;
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