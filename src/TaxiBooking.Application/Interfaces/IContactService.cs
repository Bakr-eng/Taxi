using TaxiBooking.Application.DTOs;

namespace TaxiBooking.Application.Interfaces;

public interface IContactService 
{
    Task SendAsync(CreateContactMessageDto dto);
}