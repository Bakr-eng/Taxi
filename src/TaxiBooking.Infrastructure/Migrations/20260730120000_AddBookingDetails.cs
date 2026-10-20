using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Bookings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Bookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Bookings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupTime",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()");

            migrationBuilder.AddColumn<bool>(
                name: "HasAllergy",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasIllness",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SpecialNeedsNote",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasAllergy",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasIllness",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SpecialNeedsNote",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Bookings");
        }
    }
}
