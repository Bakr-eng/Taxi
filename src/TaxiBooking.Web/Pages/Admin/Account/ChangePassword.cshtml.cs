using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages.Admin.Account
{
    [Authorize(Policy = "ShouldBeAdmin")]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Ange ditt nuvarande lösenord")]
            [DataType(DataType.Password)]
            [Display(Name = "Nuvarande lösenord")]
            public string OldPassword { get; set; } = "";

            [Required(ErrorMessage = "Ange ett nytt lösenord")]
            [StringLength(100, ErrorMessage = "{0} måste vara minst {2} och högst {1} tecken långt.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Nytt lösenord")]
            public string NewPassword { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Bekräfta nytt lösenord")]
            [Compare("NewPassword", ErrorMessage = "Lösenorden matchar inte.")]
            public string ConfirmPassword { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kunde inte hitta användare med id '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kunde inte hitta användare med id '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                // Uppdatera inloggningscookien så användaren inte loggas ut
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Admin-användaren {UserId} bytte sitt lösenord.", user.Id);

                StatusMessage = "Ditt lösenord har ändrats.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ett oväntat fel inträffade när admin-användaren {UserId} försökte byta lösenord.", user.Id);
                ModelState.AddModelError(string.Empty, "Ett oväntat fel inträffade. Försök igen om en stund.");
                return Page();
            }
        }
    }
}
