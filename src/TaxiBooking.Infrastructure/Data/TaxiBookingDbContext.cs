using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaxiBooking.Domain.Entities;

namespace TaxiBooking.Infrastructure.Data
{
    public class TaxiBookingDbContext : IdentityDbContext<AppUser>
    {
        public TaxiBookingDbContext(DbContextOptions<TaxiBookingDbContext> options)
            : base(options) { }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .Property(b => b.ContactName)
                .HasMaxLength(200);

            builder.Entity<Booking>()
                .Property(b => b.ContactPhone)
                .HasMaxLength(50);

            builder.Entity<Booking>()
                .Property(b => b.ContactEmail)
                .HasMaxLength(256);

            builder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Entity<Booking>()
                .Property(b => b.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Entity<Booking>()
                .Property(b => b.PaymentReference)
                .HasMaxLength(100);

            builder.Entity<Booking>()
                .HasOne(b => b.Driver)
                .WithMany(d => d.Bookings)
                .HasForeignKey(b => b.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Driver>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Car>()
                .HasOne(c => c.Driver)
                .WithMany(d => d.Cars)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Car>()
                .Property(c => c.RegistrationNumber)
                .HasMaxLength(20);

            builder.Entity<ContactMessage>()
                .Property(m => m.Name)
                .HasMaxLength(200);

            builder.Entity<ContactMessage>()
                .Property(m => m.Email)
                .HasMaxLength(256);
            
            builder.Entity<ContactMessage>()
                .Property(m => m.Message)
                .HasMaxLength(2000);
        }
    }
}
