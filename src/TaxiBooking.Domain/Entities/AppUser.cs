using Microsoft.AspNetCore.Identity;

namespace TaxiBooking.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = "";
    }
}
