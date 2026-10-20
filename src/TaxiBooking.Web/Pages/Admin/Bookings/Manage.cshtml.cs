using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages.Admin.Bookings
{
    [Authorize(Policy = "ShouldBeAdmin")]
    public class ManageModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IDriverService _driverService;

        public ManageModel(IBookingService bookingService, IDriverService driverService)
        {
            _bookingService = bookingService;
            _driverService = driverService;
        }

        public List<Booking> AllBookings { get; set; } = new();
        public List<Driver> AvailableDrivers { get; set; } = new();

        public async Task OnGetAsync()
        {
            AllBookings = (await _bookingService.GetAllAsync())
                .OrderByDescending(b => b.RequestedAt)
                .ToList();
            AvailableDrivers = (await _driverService.GetAllAsync())
                .Where(d => d.IsAvailable)
                .ToList();
        }

        public async Task<IActionResult> OnPostAssignAsync(int bookingId, int driverId)
        {
            await _bookingService.AssignDriverAsync(bookingId, driverId);
            TempData["StatusMessage"] = "Förare tilldelad bokningen.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSetStatusAsync(int id, BookingStatus status)
        {
            await _bookingService.UpdateStatusAsync(id, status);
            TempData["StatusMessage"] = $"Bokning #{id} uppdaterad till \"{status}\".";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _bookingService.DeleteAsync(id);
            TempData["StatusMessage"] = "Bokningen raderades.";
            return RedirectToPage();
        }
    }
}
