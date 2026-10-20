using Microsoft.AspNetCore.Identity;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Web.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            if (!await roleManager.RoleExistsAsync("Driver"))
                await roleManager.CreateAsync(new IdentityRole("Driver"));

            string adminEmail = config["SeedData:AdminEmail"] ?? "admin@taxibooking.se";
            string adminPassword = config["SeedData:AdminPassword"]
                ?? throw new InvalidOperationException(
                    "Admin password must be configured via SeedData:AdminPassword in user secrets or environment variables.");

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Name = "Admin"
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
