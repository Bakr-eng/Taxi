using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.DTOs;

public record BookingDto(
    int Id, string UserId, string? UserName,
    string PickupAddress, string DropoffAddress,
    string ContactName, string ContactPhone, string ContactEmail,
    DateTime PickupTime, bool HasAllergy, bool HasIllness,
    string? SpecialNeedsNote, string? Message,
    DateTime RequestedAt, BookingStatus Status, decimal EstimatedFare,
    int? DriverId, string? DriverName, string? DriverPhone,
    PaymentMethod? PaymentMethod, bool IsPaid, DateTime? PaidAt);

public record CreateBookingDto(
    string UserId, string ContactName, string ContactPhone, string ContactEmail,
    string PickupAddress, string DropoffAddress, DateTime PickupTime,
    bool HasAllergy, bool HasIllness, string? SpecialNeedsNote, string? Message,
    double? DistanceKm, double? DurationMinutes);