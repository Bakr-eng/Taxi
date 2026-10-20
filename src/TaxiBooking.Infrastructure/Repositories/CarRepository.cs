using Microsoft.EntityFrameworkCore;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;
using TaxiBooking.Infrastructure.Data;

namespace TaxiBooking.Infrastructure.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly TaxiBookingDbContext _db;

        public CarRepository(TaxiBookingDbContext db)
        {
            _db = db;
        }

        public async Task<List<Car>> GetAllAsync()
            => await _db.Cars.Include(c => c.Driver).ToListAsync();

        public async Task<Car?> GetByIdAsync(int id)
            => await _db.Cars.Include(c => c.Driver).FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Car car)
        {
            await _db.Cars.AddAsync(car);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Car car)
        {
            _db.Cars.Update(car);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var car = await _db.Cars.FindAsync(id);
            if (car != null)
            {
                _db.Cars.Remove(car);
                await _db.SaveChangesAsync();
            }
        }
    }
}