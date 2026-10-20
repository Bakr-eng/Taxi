# TaxiBooking

ASP.NET Core Razor Pages taxi booking application (.NET 9), using EF Core + SQL Server and ASP.NET Identity for auth.

## First-time setup

1. Restore and build:
   ```
   dotnet restore
   dotnet build
   ```

2. Set the seed admin account password (no longer hardcoded in source). From `src/TaxiBooking.Web`:
   ```
   dotnet user-secrets set "SeedData:AdminPassword" "<a-strong-password>"
   dotnet user-secrets set "SeedData:AdminEmail" "admin@taxibooking.se"
   ```
   In production, set these as environment variables (`SeedData__AdminPassword`, `SeedData__AdminEmail`) or via your secret manager instead of user-secrets.

3. Update the connection string in `appsettings.json` (or override via user-secrets / environment variable) to point at your SQL Server instance.

4. Apply migrations:
   ```
   dotnet ef database update --project src/TaxiBooking.Infrastructure --startup-project src/TaxiBooking.Web
   ```

5. Run the app:
   ```
   dotnet run --project src/TaxiBooking.Web
   ```

See `UPGRADE_REPORT.md` for a full list of changes made in the latest upgrade pass.
