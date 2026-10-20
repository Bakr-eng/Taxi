namespace TaxiBooking.Domain.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string LicenseNumber { get; set; } = "";
        public bool IsAvailable { get; set; } = true;
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
