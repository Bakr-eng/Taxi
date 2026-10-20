using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages.User.Bookings
{
    [Authorize(Policy = "ShouldBeUser")]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(IBookingService bookingService, UserManager<AppUser> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        public List<Booking> MyBookings { get; set; } = new();

        public DateTime MinPickupTime { get; set; }

        [BindProperty]
        public NewBookingInput Input { get; set; } = new();

        public class NewBookingInput
        {
            [Required(ErrorMessage = "Ange ditt namn")]
            [Display(Name = "Namn")]
            public string ContactName { get; set; } = "";

            [Required(ErrorMessage = "Ange mobiltelefon")]
            [Phone(ErrorMessage = "Ogiltigt telefonnummer")]
            [Display(Name = "Mobiltelefon")]
            public string ContactPhone { get; set; } = "";

            [Required(ErrorMessage = "Ange e-post")]
            [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
            [Display(Name = "E-post")]
            public string ContactEmail { get; set; } = "";

            [Required(ErrorMessage = "Ange upphämtningsadress")]
            [Display(Name = "Upphämtningsadress")]
            public string PickupAddress { get; set; } = "";

            [Required(ErrorMessage = "Ange avlämningsadress")]
            [Display(Name = "Avlämningsadress")]
            public string DropoffAddress { get; set; } = "";

            [Required(ErrorMessage = "Ange önskat datum")]
            [Display(Name = "Datum")]
            public DateTime? PickupDate { get; set; }

            [Required(ErrorMessage = "Välj timme")]
            [Range(0, 23, ErrorMessage = "Ogiltig timme")]
            [Display(Name = "Timme")]
            public int? PickupHour { get; set; }

            [Required(ErrorMessage = "Välj minut")]
            [Range(0, 59, ErrorMessage = "Ogiltig minut")]
            [Display(Name = "Minut")]
            public int? PickupMinute { get; set; }

            // Sätts ihop av PickupDate + PickupHour + PickupMinute i OnPostCreateAsync.
            [Display(Name = "Önskad upphämtningstid")]
            public DateTime? PickupTime { get; set; }

            [Display(Name = "Jag har allergi")]
            public bool HasAllergy { get; set; }

            [Display(Name = "Jag har sjukdom/behöver assistans")]
            public bool HasIllness { get; set; }

            [Display(Name = "Beskriv gärna allergin/sjukdomen (frivilligt)")]
            public string? SpecialNeedsNote { get; set; }

            [Display(Name = "Meddelande till föraren (frivilligt)")]
            public string? Message { get; set; }

            // Fylls i automatiskt av kartan (JavaScript) när adresserna slås upp.
            // Om de saknas (t.ex. JS avstängt) faller prisberäkningen tillbaka på en uppskattning.
            public double? PickupLat { get; set; }
            public double? PickupLng { get; set; }
            public double? DropoffLat { get; set; }
            public double? DropoffLng { get; set; }
            public double? DistanceKm { get; set; }
            public double? DurationMinutes { get; set; }
        }

        public async Task OnGetAsync()
        {
            MinPickupTime = DateTime.Now.AddMinutes(30);

            var user = await _userManager.GetUserAsync(User);
            if (user != null && string.IsNullOrWhiteSpace(Input.ContactName))
            {
                Input.ContactName = user.Name;
                Input.ContactPhone = user.PhoneNumber ?? "";
                Input.ContactEmail = user.Email ?? "";
            }

            var userId = _userManager.GetUserId(User)!;
            MyBookings = (await _bookingService.GetByUserIdAsync(userId))
                .OrderByDescending(b => b.RequestedAt)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            MinPickupTime = DateTime.Now.AddMinutes(30);

            if (Input.PickupDate.HasValue && Input.PickupHour.HasValue && Input.PickupMinute.HasValue)
            {
                Input.PickupTime = Input.PickupDate.Value.Date
                    .AddHours(Input.PickupHour.Value)
                    .AddMinutes(Input.PickupMinute.Value);
            }

            if (Input.PickupTime.HasValue && Input.PickupTime.Value <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(Input.PickupDate), "Upphämtningstiden måste ligga framåt i tiden, inte en tid som redan passerat.");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var userId = _userManager.GetUserId(User)!;
            var estimatedFare = EstimateFare(Input.PickupAddress, Input.DropoffAddress, Input.DistanceKm, Input.DurationMinutes);

            var bookingId = await _bookingService.CreateAsync(
                userId,
                Input.ContactName,
                Input.ContactPhone,
                Input.ContactEmail,
                Input.PickupAddress,
                Input.DropoffAddress,
                Input.PickupTime!.Value,
                Input.HasAllergy,
                Input.HasIllness,
                Input.SpecialNeedsNote,
                Input.Message,
                estimatedFare);

            return RedirectToPage("./Payment", new { id = bookingId });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var booking = await _bookingService.GetByIdAsync(id);

            if (booking != null && booking.UserId == userId && booking.Status == BookingStatus.Pending)
            {
                await _bookingService.UpdateStatusAsync(id, BookingStatus.Cancelled);
                TempData["StatusMessage"] = "Bokningen har avbokats.";
            }

            return RedirectToPage();
        }

        // PRISMODELL — ändra dessa tre värden till era riktiga priser.
        // VIKTIGT: samma tre värden finns även i JavaScript-koden i Index.cshtml (@section Scripts),
        // så att priset kunden ser i kartan innan bokning stämmer med det som sparas här. Ändra på båda ställena.
        private const decimal BaseFare = 50m;          // Fast grundavgift, kr, oavsett resans längd
        private const decimal PricePerKm = 15m;        // Kr per körd kilometer
        private const decimal PricePerMinute = 5m;     // Kr per minuts körtid (trafik/väntetid ingår i restiden från rutten)

        private static decimal EstimateFare(string pickup, string dropoff, double? distanceKm, double? durationMinutes)
        {
            // Om kartan har räknat ut ett riktigt avstånd + körtid (via geokodning + rutt) används det.
            if (distanceKm.HasValue && distanceKm.Value > 0.1)
            {
                var minutes = durationMinutes.HasValue && durationMinutes.Value > 0 ? durationMinutes.Value : 0;
                return BaseFare + PricePerKm * (decimal)distanceKm.Value + PricePerMinute * (decimal)minutes;
            }

            // Reservlösning om JavaScript/kartan inte kunde slå upp adresserna (t.ex. adress kartan
            // inte hittade, eller JS avstängt). Ger ett grovt uppskattat pris istället för att blockera bokningen.
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
}
