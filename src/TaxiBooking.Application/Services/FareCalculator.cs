using TaxiBooking.Application.Interfaces;

namespace TaxiBooking.Application.Services;

public class FareCalculator : IFareCalculator
{
    // Samma värden finns i JavaScript i User/Bookings/Index.cshtml. Ändra på båda ställena.
    private const decimal BaseFare = 50m;
    private const decimal PricePerKm = 15m;
    private const decimal PricePerMinute = 5m;

    public decimal Estimate(string pickup, string dropoff, double? distanceKm, double? durationMinutes)
    {
        if (distanceKm.HasValue && distanceKm.Value > 0.1)
        {
            var minutes = durationMinutes.HasValue && durationMinutes.Value > 0 ? durationMinutes.Value : 0;
            return BaseFare + PricePerKm * (decimal)distanceKm.Value + PricePerMinute * (decimal)minutes;
        }

        // Reservlösning om kartan inte kunde slå upp adresserna
        var combined = pickup + dropoff;
        uint hash = 2166136261;
        foreach (char c in combined)
        {
            hash ^= c;
            hash *= 16777619;
        }
        var pseudoKm = 3 + (hash % 18);
        return BaseFare + PricePerKm * pseudoKm;
    }
}