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
    public class PaymentModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<AppUser> _userManager;

        public PaymentModel(IBookingService bookingService, UserManager<AppUser> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }

        public BookingDto? Booking { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var booking = await _bookingService.GetByIdForUserAsync(id, userId);

            if (booking == null)
            {
                return RedirectToPage("./Index");
            }

            if (booking.IsPaid || booking.PaymentMethod.HasValue)
            {
                // Redan hanterad - inget mer att göra här.
                return RedirectToPage("./Index");
            }

            Booking = booking;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, string method)
        {
            var userId = _userManager.GetUserId(User)!;
            var booking = await _bookingService.GetByIdForUserAsync(id, userId);

            if (booking == null)
            {
                return RedirectToPage("./Index");
            }

            if (!Enum.TryParse<PaymentMethod>(method, out var paymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Ogiltigt betalsätt.");
                Booking = booking;
                return Page();
            }

            // Swish och kort betalas direkt (simulerat här - koppla till en riktig betaltjänst i produktion).
            // "Betala senare" bokar upphämtningen men betalningen sker vid resan.
            var isPaid = paymentMethod is PaymentMethod.Swish or PaymentMethod.Kort;

            await _bookingService.SetPaymentAsync(id, paymentMethod, isPaid);

            TempData["StatusMessage"] = isPaid
                ? "Betalningen lyckades! Din bokning är bekräftad."
                : "Bokningen är bekräftad. Betala vid resan.";

            return RedirectToPage("./Index");
        }
    }
}