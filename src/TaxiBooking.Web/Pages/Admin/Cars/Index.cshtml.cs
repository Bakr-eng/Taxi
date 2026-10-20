using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages.Admin.Cars
{
    [Authorize(Policy = "ShouldBeAdmin")]
    public class IndexModel : PageModel
    {
        private readonly ICarService _carService;
        private readonly IDriverService _driverService;

        public IndexModel(ICarService carService, IDriverService driverService)
        {
            _carService = carService;
            _driverService = driverService;
        }

        public List<Car> Cars { get; set; } = new();
        public List<Driver> Drivers { get; set; } = new();

        [BindProperty]
        public NewCarInput Input { get; set; } = new();

        public class NewCarInput
        {
            [Required(ErrorMessage = "Ange registreringsnummer")]
            [Display(Name = "Reg.nr")]
            public string RegistrationNumber { get; set; } = "";

            [Required(ErrorMessage = "Ange märke")]
            [Display(Name = "Märke")]
            public string Make { get; set; } = "";

            [Required(ErrorMessage = "Ange modell")]
            [Display(Name = "Modell")]
            public string Model { get; set; } = "";

            [Range(1980, 2100, ErrorMessage = "Ange ett giltigt år")]
            [Display(Name = "År")]
            public int Year { get; set; } = DateTime.Now.Year;

            [Range(1, 20, ErrorMessage = "Ange antal platser")]
            [Display(Name = "Platser")]
            public int Seats { get; set; } = 4;

            [Display(Name = "Förare")]
            public int? DriverId { get; set; }
        }

        public async Task OnGetAsync()
        {
            Cars = (await _carService.GetAllAsync())
                .OrderBy(c => c.RegistrationNumber)
                .ToList();
            Drivers = (await _driverService.GetAllAsync())
                .OrderBy(d => d.Name)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            await _carService.CreateAsync(
                Input.RegistrationNumber,
                Input.Make,
                Input.Model,
                Input.Year,
                Input.Seats,
                Input.DriverId);

            TempData["StatusMessage"] = $"Bilen {Input.RegistrationNumber} har lagts till.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignDriverAsync(int id, int? driverId)
        {
            var car = await _carService.GetByIdAsync(id);
            if (car != null)
            {
                car.DriverId = driverId;
                await _carService.UpdateAsync(car);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var car = await _carService.GetByIdAsync(id);
            if (car != null)
            {
                car.IsActive = !car.IsActive;
                await _carService.UpdateAsync(car);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _carService.DeleteAsync(id);
            TempData["StatusMessage"] = "Bilen har tagits bort.";
            return RedirectToPage();
        }
    }
}
