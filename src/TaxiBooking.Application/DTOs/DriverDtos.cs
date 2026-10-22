namespace TaxiBooking.Application.DTOs;

public record DriverDto(int Id, string Name, string PhoneNumber, string LicenseNumber,
                        bool IsAvailable, string? UserId, string? Email);

public record CreateDriverDto(string Name, string PhoneNumber, string LicenseNumber, string? UserId);