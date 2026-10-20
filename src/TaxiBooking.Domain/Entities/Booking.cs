namespace TaxiBooking.Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public AppUser? User { get; set; }

        public string PickupAddress { get; set; } = "";
        public string DropoffAddress { get; set; } = "";

        public string ContactName { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        public string ContactEmail { get; set; } = "";

        public DateTime PickupTime { get; set; }

        public bool HasAllergy { get; set; }
        public bool HasIllness { get; set; }
        public string? SpecialNeedsNote { get; set; }

        public string? Message { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public decimal EstimatedFare { get; set; }
        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? PaymentReference { get; set; }
    }
}
