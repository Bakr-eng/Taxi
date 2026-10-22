namespace TaxiBooking.Application.DTOs;

public record CarDto(int Id, string RegistrationNumber, string Make, string Model,
                     int Year, int Seats, bool IsActive, int? DriverId, string? DriverName);

public record CreateCarDto(string RegistrationNumber, string Make, string Model,
                           int Year, int Seats, int? DriverId);