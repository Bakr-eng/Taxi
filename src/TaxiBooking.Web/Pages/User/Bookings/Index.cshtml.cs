using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.DTOs;
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

        public List<BookingDto> MyBookings { get; set; } = new();

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
            MyBookings = await _bookingService.GetByUserIdAsync(userId);
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

            var bookingId = await _bookingService.CreateAsync(new CreateBookingDto(
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
                Input.DistanceKm,
                Input.DurationMinutes));

            return RedirectToPage("./Payment", new { id = bookingId });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;

            if (await _bookingService.CancelAsync(id, userId))
            {
                TempData["StatusMessage"] = "Bokningen har avbokats.";
            }

            return RedirectToPage();
        }
    }
}