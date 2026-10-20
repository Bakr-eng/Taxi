using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages.DriverPortal
{
    [Authorize(Policy = "ShouldBeDriver")]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IDriverService _driverService;
        private readonly UserManager<AppUser> _userManager;

        public IndexModel(IBookingService bookingService, IDriverService driverService, UserManager<AppUser> userManager)
        {
            _bookingService = bookingService;
            _driverService = driverService;
            _userManager = userManager;
        }

        public Driver? MyDriverProfile { get; set; }
        public List<Booking> MyBookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            MyDriverProfile = await _driverService.GetByUserIdAsync(userId);

            if (MyDriverProfile != null)
            {
                MyBookings = (await _bookingService.GetByDriverIdAsync(MyDriverProfile.Id))
                    .Where(b => b.Status is BookingStatus.Confirmed or BookingStatus.Completed)
                    .OrderBy(b => b.PickupTime)
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            MyDriverProfile = await _driverService.GetByUserIdAsync(userId);

            if (MyDriverProfile == null)
                return RedirectToPage();

            var booking = await _bookingService.GetByIdAsync(id);
            if (booking != null && booking.DriverId == MyDriverProfile.Id && booking.Status == BookingStatus.Confirmed)
            {
                await _bookingService.UpdateStatusAsync(id, BookingStatus.Completed);
                TempData["StatusMessage"] = "Körningen har markerats som slutförd.";
            }

            return RedirectToPage();
        }
    }
}
