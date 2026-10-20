using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Interfaces
{
    public interface IContactMessageRepository
    {
        Task AddAsync(ContactMessage message);
    }
}
