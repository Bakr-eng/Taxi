using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.DTOs;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages.Admin.Drivers
{
    [Authorize(Policy = "ShouldBeAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IDriverService _driverService;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(IDriverService driverService, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _driverService = driverService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<DriverDto> Drivers { get; set; } = new();

        [BindProperty]
        public NewDriverInput Input { get; set; } = new();

        public class NewDriverInput
        {
            [Required(ErrorMessage = "Ange namn")]
            [Display(Name = "Namn")]
            public string Name { get; set; } = "";

            [Required(ErrorMessage = "Ange e-post")]
            [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
            [Display(Name = "E-post (används som inloggning)")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Ange lösenord")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenordet måste vara minst {2} tecken.")]
            [DataType(DataType.Password)]
            [Display(Name = "Tillfälligt lösenord")]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Ange telefonnummer")]
            [Display(Name = "Telefonnummer")]
            public string PhoneNumber { get; set; } = "";

            [Required(ErrorMessage = "Ange körkortsnummer")]
            [Display(Name = "Körkortsnummer")]
            public string LicenseNumber { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            Drivers = await _driverService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var existing = await _userManager.FindByEmailAsync(Input.Email);
            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "Det finns redan ett konto med den e-postadressen.");
                await OnGetAsync();
                return Page();
            }

            var user = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                Name = Input.Name
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await OnGetAsync();
                return Page();
            }

            if (!await _roleManager.RoleExistsAsync("Driver"))
                await _roleManager.CreateAsync(new IdentityRole("Driver"));

            await _userManager.AddToRoleAsync(user, "Driver");
            await _driverService.CreateAsync(new CreateDriverDto(
                Input.Name, Input.PhoneNumber, Input.LicenseNumber, user.Id));

            TempData["StatusMessage"] = $"Föraren {Input.Name} har lagts till och kan logga in med {Input.Email}.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            await _driverService.ToggleAvailabilityAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var driver = await _driverService.GetByIdAsync(id);
            await _driverService.DeleteAsync(id);

            // Ta även bort inloggningskontot om föraren hade ett
            if (driver?.UserId != null)
            {
                var user = await _userManager.FindByIdAsync(driver.UserId);
                if (user != null)
                    await _userManager.DeleteAsync(user);
            }

            TempData["StatusMessage"] = "Föraren har tagits bort.";
            return RedirectToPage();
        }
    }
}