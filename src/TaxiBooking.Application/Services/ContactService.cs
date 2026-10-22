using Microsoft.Extensions.Logging;
using TaxiBooking.Application.DTOs;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Services;

public class ContactService : IContactService
{
    private readonly IContactMessageRepository _repo;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IContactMessageRepository repo, ILogger<ContactService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task SendAsync(CreateContactMessageDto dto)
    {
        var message = new ContactMessage
        {
            Name = dto.Name,
            Email = dto.Email,
            Message = dto.Message,
            SentAt = DateTime.UtcNow
        };
        await _repo.AddAsync(message);
        _logger.LogInformation("Contact message received from {Email}", dto.Email);
    }
}