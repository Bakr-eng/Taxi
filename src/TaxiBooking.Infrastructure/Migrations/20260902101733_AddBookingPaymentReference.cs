using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPaymentReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Bookings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Bookings");
        }
    }
}
