using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaxiBooking.Application.Interfaces;
using TaxiBooking.Application.Services;
using TaxiBooking.Domain.Entities;
using TaxiBooking.Infrastructure.Data;
using TaxiBooking.Infrastructure.Repositories;
using TaxiBooking.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<TaxiBookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<TaxiBookingDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ShouldBeAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("ShouldBeUser", p => p.RequireRole("User", "Admin"));
    options.AddPolicy("ShouldBeDriver", p => p.RequireRole("Driver", "Admin"));
});

// Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IContactMessageRepository, ContactMessageRepository>();

// Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<ICarService, CarService>();

// Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Seed
using (var scope = app.Services.CreateScope())
{
    await SeedData.Initialize(scope.ServiceProvider);
}

app.Run();
