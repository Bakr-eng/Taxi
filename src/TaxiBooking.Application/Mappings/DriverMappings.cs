using TaxiBooking.Application.DTOs;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Mappings;

public static class DriverMappings
{
    public static DriverDto ToDto(this Driver d) => new(
        d.Id, d.Name, d.PhoneNumber, d.LicenseNumber,
        d.IsAvailable, d.UserId, d.User?.Email);

    public static Driver ToEntity(this CreateDriverDto d) => new()
    {
        Name = d.Name,
        PhoneNumber = d.PhoneNumber,
        LicenseNumber = d.LicenseNumber,
        UserId = d.UserId,
        IsAvailable = true
    };
}