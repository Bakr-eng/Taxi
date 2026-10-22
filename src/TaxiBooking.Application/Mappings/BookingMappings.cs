using TaxiBooking.Application.DTOs;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Mappings;

public static class BookingMappings
{
    public static BookingDto ToDto(this Booking b) => new(
        b.Id, b.UserId, b.User?.Name,
        b.PickupAddress, b.DropoffAddress,
        b.ContactName, b.ContactPhone, b.ContactEmail,
        b.PickupTime, b.HasAllergy, b.HasIllness,
        b.SpecialNeedsNote, b.Message,
        b.RequestedAt, b.Status, b.EstimatedFare,
        b.DriverId, b.Driver?.Name, b.Driver?.PhoneNumber,
        b.PaymentMethod, b.IsPaid, b.PaidAt);

    public static Booking ToEntity(this CreateBookingDto d, decimal fare) => new()
    {
        UserId = d.UserId,
        ContactName = d.ContactName,
        ContactPhone = d.ContactPhone,
        ContactEmail = d.ContactEmail,
        PickupAddress = d.PickupAddress,
        DropoffAddress = d.DropoffAddress,
        PickupTime = d.PickupTime,
        HasAllergy = d.HasAllergy,
        HasIllness = d.HasIllness,
        SpecialNeedsNote = d.SpecialNeedsNote,
        Message = d.Message,
        EstimatedFare = fare,
        RequestedAt = DateTime.UtcNow,
        Status = BookingStatus.Pending
    };
}