using Microsoft.EntityFrameworkCore;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;
using TaxiBooking.Infrastructure.Data;

namespace TaxiBooking.Infrastructure.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly TaxiBookingDbContext _db;

        public DriverRepository(TaxiBookingDbContext db)
        {
            _db = db;
        }

        public async Task<List<Driver>> GetAllAsync()
            => await _db.Drivers.Include(d => d.User).ToListAsync();

        public async Task<Driver?> GetByIdAsync(int id)
            => await _db.Drivers.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<Driver?> GetByUserIdAsync(string userId)
            => await _db.Drivers.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

        public async Task AddAsync(Driver driver)
        {
            await _db.Drivers.AddAsync(driver);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Driver driver)
        {
            _db.Drivers.Update(driver);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var driver = await _db.Drivers.FindAsync(id);
            if (driver != null)
            {
                _db.Drivers.Remove(driver);
                await _db.SaveChangesAsync();
            }
        }
    }
}
