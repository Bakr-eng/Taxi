using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IContactMessageRepository _contactRepo;

        public ContactModel(IContactMessageRepository contactRepo)
        {
            _contactRepo = contactRepo;
        }

        [BindProperty]
        public ContactInput Input { get; set; } = new();

        public bool Sent { get; set; }

        public class ContactInput
        {
            [Required(ErrorMessage = "Ange namn")]
            [Display(Name = "Namn")]
            public string Name { get; set; } = "";

            [Required(ErrorMessage = "Ange e-post")]
            [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
            [Display(Name = "E-post")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Skriv ett meddelande")]
            [Display(Name = "Meddelande")]
            public string Message { get; set; } = "";
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var message = new ContactMessage
            {
                Name = Input.Name,
                Email = Input.Email,
                Message = Input.Message,
                SentAt = DateTime.UtcNow
            };
            await _contactRepo.AddAsync(message);

            Sent = true;
            Input = new ContactInput();
            ModelState.Clear();
            return Page();
        }
    }
}
