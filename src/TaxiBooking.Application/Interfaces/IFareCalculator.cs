namespace TaxiBooking.Application.Interfaces;

public interface IFareCalculator
{
    decimal Estimate(string pickup, string dropoff, double? distanceKm, double? durationMinutes);
}