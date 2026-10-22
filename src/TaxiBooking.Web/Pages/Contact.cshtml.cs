using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TaxiBooking.Application.DTOs;
using TaxiBooking.Application.Interfaces;

namespace TaxiBooking.Web.Pages
{
    public class ContactModel : PageModel
    {
        private readonly IContactService _contactService;

        public ContactModel(IContactService contactService)
        {
            _contactService = contactService;
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

            await _contactService.SendAsync(new CreateContactMessageDto(Input.Name, Input.Email, Input.Message));

            Sent = true;
            Input = new ContactInput();
            ModelState.Clear();
            return Page();
        }
    }
}