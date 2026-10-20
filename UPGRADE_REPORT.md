# TaxiBooking — Upgrade Report

Date: 2026-08-10

This report covers every change made in this upgrade pass, grouped by category. File paths are relative to the repo root.

> **Important caveat:** No .NET SDK was available in the environment used to make these changes, so the code could not be compiled, migrated, or run to verify it. Every change was made by careful manual inspection and follows the existing project's patterns exactly (including hand-writing the new EF Core migration to match the format of the existing ones). **Before deploying, run `dotnet build` and test locally** — see `README.md` for setup steps. If anything doesn't compile, it's most likely a small typo, not a structural issue.

---

## 1. Bug fixes

### 1.1 Broken role assignment on registration
**File:** `src/TaxiBooking.Web/Areas/Identity/Pages/Account/Register.cshtml.cs`

The old code called `AddToRoleAsync(user, "User")` *before* checking whether the "User" role existed, and then called it a second time redundantly:
```csharp
await _userManager.AddToRoleAsync(user, "User");       // could fail — role may not exist yet
if (!await _roleManager.RoleExistsAsync("User"))
    await _roleManager.CreateAsync(new IdentityRole("User"));
await _userManager.AddToRoleAsync(user, "User");        // duplicate call
```
Fixed to check/create the role first, then assign it once. If seed data hadn't run yet (fresh DB, different environment), the old code would throw on the very first registration.

### 1.2 Non-deterministic fare estimation
**File:** `src/TaxiBooking.Web/Pages/User/Bookings/Index.cshtml.cs`

`EstimateFare` used `string.GetHashCode()`, which .NET randomizes per-process since .NET 6 (for security, hash-flooding protection). This meant the *same* pickup/dropoff pair could produce a different quoted price after every app restart. Replaced with a manual FNV-1a hash implementation, which is deterministic across runs and platforms.

### 1.3 Contact form silently discarded messages
**Files:** `src/TaxiBooking.Web/Pages/Contact.cshtml.cs`, new `ContactMessage` entity/repository

The contact page set `Sent = true` and threw the submitted message away (there was a `// TODO: send email or save to DB` comment). It now persists every submission to a new `ContactMessages` table via `IContactMessageRepository`, so messages are no longer lost. (Note: there's still no admin page to *view* these — see "Suggested follow-ups" below.)

### 1.4 Garbled Swedish characters
**File:** `src/TaxiBooking.Web/Pages/Admin/Cars/Index.cshtml.cs`

Labels like `M�rke`, `Modell`, `�r`, `F�rare` (mis-encoded `Märke`, `År`, `Förare`) have been corrected to proper UTF-8 Swedish characters.

### 1.5 Silent failures on not-found records
**Files:** `src/TaxiBooking.Application/Services/BookingService.cs`, `IBookingService.cs`

`UpdateStatusAsync` and `AssignDriverAsync` used to do `if (booking == null) return;` — the caller had no way to know whether the operation actually happened. Both now return `bool` (`true` = succeeded, `false` = booking not found), and log a warning on the not-found path.

---

## 2. Type safety — Booking status enum

**Files:** new `src/TaxiBooking.Domain/Entities/BookingStatus.cs`, `Booking.cs`, `BookingService.cs`, `IBookingService.cs`, `TaxiBookingDbContext.cs`, all Admin/User/DriverPortal booking pages (`.cs` and `.cshtml`), new migration.

`Booking.Status` was a raw `string` compared against magic literals (`"Pending"`, `"Confirmed"`, `"Completed"`, `"Cancelled"`) in 8+ places across services and Razor views. A typo anywhere would silently break status logic with no compiler warning.

Replaced with a `BookingStatus` enum (`Pending`, `Confirmed`, `Completed`, `Cancelled`), stored in the database as a string (`HasConversion<string>()`, `nvarchar(20)`) so existing data in the `Status` column (which was already text like `"Pending"`) remains readable without a data migration — only a column-size change was needed.

A new EF Core migration (`20260810120000_AddContactMessagesAndBookingStatusEnum`) narrows the `Status` column from `nvarchar(max)` to `nvarchar(20)` and creates the new `ContactMessages` table. The model snapshot was updated to match.

---

## 3. Security

### 3.1 Hardcoded admin password removed
**File:** `src/TaxiBooking.Web/Data/SeedData.cs`

The seed admin account password (`"Admin123!"`) was committed directly in source. It's now read from configuration (`SeedData:AdminPassword`), intended to be supplied via `dotnet user-secrets` locally or environment variables (`SeedData__AdminPassword`) in production. **Startup now throws a clear error if it isn't set**, rather than silently using a weak default — see `README.md` for the exact command to run.

### 3.2 Stronger password policy
**File:** `src/TaxiBooking.Web/Program.cs`

Identity's password requirements were previously very weak (6 characters, no uppercase, no special character required):
```csharp
options.Password.RequiredLength = 6;
options.Password.RequireUppercase = false;
options.Password.RequireNonAlphanumeric = false;
```
Now requires 8+ characters, at least one uppercase letter, and at least one non-alphanumeric character. The Register and driver-creation pages' `[StringLength(..., MinimumLength = ...)]` attributes were updated to match (6 → 8).

### 3.3 Removed dead `MapControllers()` call
**File:** `src/TaxiBooking.Web/Program.cs`

The app has no MVC controllers (Razor Pages only), so `app.MapControllers()` was dead code that only added confusion about the app's actual routing surface. Removed.

### 3.4 `.gitignore` added
**File:** new `.gitignore`

The repo had no `.gitignore` — `bin/`, `obj/`, and `.vs/` (including cached build state and IDE indexes) were all trackable. Also excludes `secrets.json` and `*.env` so local secrets can never be accidentally committed.

---

## 4. Architecture & code quality

### 4.1 Logging added to the service layer
**Files:** `BookingService.cs`, `DriverService.cs`, `CarService.cs`

None of the services had any logging. `ILogger<T>` is now injected into all three, with `LogInformation` on create/status-change/delete operations and `LogWarning` when an operation targets a booking that doesn't exist (see 1.5). This required adding the `Microsoft.Extensions.Logging.Abstractions` package reference to `TaxiBooking.Application.csproj`.

### 4.2 Removed unnecessary eager-loading
**File:** `src/TaxiBooking.Infrastructure/Repositories/DriverRepository.cs`

`GetAllAsync`, `GetByIdAsync`, and `GetByUserIdAsync` always did `.Include(d => d.Bookings)`, meaning every driver-list page load pulled in each driver's *entire booking history* even though the Admin/Drivers page only ever displays name/phone/license/availability. Removed — bookings are still fetched separately (and correctly) wherever they're actually needed via `IBookingService.GetByDriverIdAsync`.

### 4.3 Drivers can now complete their own bookings
**Files:** `src/TaxiBooking.Web/Pages/DriverPortal/Index.cshtml.cs`, `Index.cshtml`

Previously only an Admin could mark a `Confirmed` booking as `Completed` — a driver had no way to close out a finished trip from their own portal. Added `OnPostCompleteAsync`, scoped so a driver can only complete bookings assigned to *them*, plus a "Slutför körning" button on their booking cards.

---

## 5. Repo hygiene

- **`.gitignore`** (new) — see 3.4.
- **`README.md`** (new) — setup steps: restore/build, setting the seed admin password via user secrets, running migrations, running the app.
- **This report** (`UPGRADE_REPORT.md`).

---

## What was intentionally left alone

- **NuGet package versions** — all Microsoft packages are pinned at `9.0.0`. This environment had no internet/NuGet cache access to safely verify which newer 9.0.x patch actually exists, and guessing a version number risks a broken restore. **Recommended next step:** run `dotnet list package --outdated` and bump to the latest 9.0.x patch yourself.
- **No test project** — still a real gap. The Application layer (services) is now more testable than before (logging via constructor injection, `bool`-returning methods instead of silent no-ops), but no xUnit project was added. Recommended follow-up.
- **No DTOs/ViewModels** — domain entities are still passed directly to Razor views. Left as-is to keep this pass focused; worth doing as a separate refactor since it touches nearly every page.
- **No generic repository / Unit of Work** — the three repositories remain near-identical copy-paste code with independent `SaveChangesAsync()` calls. Not changed, since introducing a Unit of Work is a bigger structural change best done deliberately with tests in place first.
- **No pagination** — `GetAllAsync()` on all three repositories still loads everything into memory. Fine at current scale; flag if the dataset grows.

## Suggested follow-ups (not done, out of scope for this pass)

- Add an Admin page to view submitted `ContactMessage` records (the data is now saved, but nothing displays it yet).
- Add password reset / email confirmation flow for regular user registration.
- Add a booking-edit capability so users don't have to cancel and rebook for small changes.
- Add rate limiting on booking creation and the contact form.
