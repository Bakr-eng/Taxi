using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;
using TaxiBooking.Infrastructure.Data;

namespace TaxiBooking.Infrastructure.Repositories
{
    public class ContactMessageRepository : IContactMessageRepository
    {
        private readonly TaxiBookingDbContext _db;

        public ContactMessageRepository(TaxiBookingDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(ContactMessage message)
        {
            await _db.ContactMessages.AddAsync(message);
            await _db.SaveChangesAsync();
        }
    }
}
