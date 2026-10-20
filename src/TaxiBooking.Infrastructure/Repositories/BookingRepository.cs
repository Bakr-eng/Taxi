using Microsoft.EntityFrameworkCore;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;
using TaxiBooking.Infrastructure.Data;

namespace TaxiBooking.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly TaxiBookingDbContext _db;

        public BookingRepository(TaxiBookingDbContext db)
        {
            _db = db;
        }

        public async Task<List<Booking>> GetAllAsync()
            => await _db.Bookings.Include(b => b.User).Include(b => b.Driver).ToListAsync();

        public async Task<List<Booking>> GetByUserIdAsync(string userId)
            => await _db.Bookings.Include(b => b.Driver)
                .Where(b => b.UserId == userId).ToListAsync();

        public async Task<List<Booking>> GetByDriverIdAsync(int driverId)
            => await _db.Bookings.Include(b => b.User)
                .Where(b => b.DriverId == driverId).ToListAsync();

        public async Task<Booking?> GetByIdAsync(int id)
            => await _db.Bookings.Include(b => b.User).Include(b => b.Driver)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task AddAsync(Booking booking)
        {
            await _db.Bookings.AddAsync(booking);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking != null)
            {
                _db.Bookings.Remove(booking);
                await _db.SaveChangesAsync();
            }
        }
    }
}
