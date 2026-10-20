namespace TaxiBooking.Domain.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = "";
        public string Make { get; set; } = "";        // Märke, t.ex. Volvo
        public string Model { get; set; } = "";       // Modell, t.ex. V90
        public int Year { get; set; }
        public int Seats { get; set; } = 4;
        public bool IsActive { get; set; } = true;

        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }
    }
}