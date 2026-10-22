using TaxiBooking.Application.DTOs;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Application.Mappings;

public static class CarMappings
{
    public static CarDto ToDto(this Car c) => new(
        c.Id, c.RegistrationNumber, c.Make, c.Model,
        c.Year, c.Seats, c.IsActive, c.DriverId, c.Driver?.Name);

    public static Car ToEntity(this CreateCarDto d) => new()
    {
        RegistrationNumber = d.RegistrationNumber,
        Make = d.Make,
        Model = d.Model,
        Year = d.Year,
        Seats = d.Seats,
        DriverId = d.DriverId,
        IsActive = true
    };
}