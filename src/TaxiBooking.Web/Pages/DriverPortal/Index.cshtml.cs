using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.DTOs;
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

        public DriverDto? MyDriverProfile { get; set; }
        public List<BookingDto> MyBookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            MyDriverProfile = await _driverService.GetByUserIdAsync(userId);

            if (MyDriverProfile != null)
            {
                MyBookings = await _bookingService.GetForDriverAsync(MyDriverProfile.Id);
            }
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var driver = await _driverService.GetByUserIdAsync(userId);

            if (driver != null && await _bookingService.CompleteAsync(id, driver.Id))
            {
                TempData["StatusMessage"] = "Körningen har markerats som slutförd.";
            }

            return RedirectToPage();
        }
    }
}