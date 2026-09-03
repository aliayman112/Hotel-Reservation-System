<<<<<<< HEAD
# Hotel-Reservation-System
el reagala el tmm
=======
# Hotel Reservation API

Same layered style as StudentManagement.Api: Models → DTOs → Interfaces → Services → Controllers, with EF Core + SQL Server.

## Project layout

```
Models/          Entity classes (User, Hotel, RoomType, RoomInventory, Booking, Review, Enums)
DTOs/            Request/response shapes used by the controllers
Data/            AppDbContext (EF Core)
Interfaces/      Service contracts (I*.cs)
Services/        Business logic implementing the interfaces
Controllers/     API endpoints
```

## 1. Restore & configure

1. Update the connection string in `appsettings.json` to point at your SQL Server instance.
2. Change the `Jwt:Key` value in `appsettings.json` to your own secret before you go beyond local testing.
3. Restore packages:
   ```
   dotnet restore
   ```

## 2. Create the database

```
dotnet tool install --global dotnet-ef   # only needed once, if you don't have it
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 3. Run it

```
dotnet run
```

Swagger UI opens automatically at `/swagger` in Development mode — use it to try every endpoint.

## 4. Getting an admin/receptionist account

Public `/api/auth/register` always creates a normal `User`. To test Admin-only or Receptionist endpoints,
either:
- Register normally, then manually change that row's `Role` column in the DB to `Admin` or `Receptionist`, or
- Insert a row directly with a `BCrypt`-hashed password.

## 5. Typical test flow

1. `POST /api/auth/register` → get a token (User role).
2. Promote that user to Admin in the DB (see step 4).
3. Log in again (`POST /api/auth/login`) to get a fresh token with the `Admin` role claim.
4. In Swagger, click **Authorize** and paste the token (no `Bearer` prefix needed).
5. `POST /api/admin/hotels` → create a hotel.
6. `POST /api/admin/hotels/{id}/room-types` → add a room type.
7. `PUT /api/admin/room-inventory` → set how many rooms exist for specific dates.
8. Switch to (or register) a normal user account.
9. `POST /api/bookings` → create a booking for those dates (status starts `Pending`).
10. Back on the Admin/Receptionist account: `PATCH /api/admin/bookings/{id}/confirm`.
11. On the user account: `GET /api/me/bookings` → see it as `Confirmed`.
12. `PATCH /api/admin/bookings/{id}/complete` (after check-out) → status becomes `Completed`.
13. `POST /api/reviews` → now allowed, since there's a Completed stay.

## Design notes worth knowing

- **Inventory is reserved at booking creation, not at confirmation.** This is what actually prevents
  double-booking — two users can't both claim the last room while a booking is still Pending.
  If a booking is Rejected or Cancelled, the reserved rooms are released back into inventory.
- **Enums are stored as strings** (`Pending`, `Admin`, etc.) in the database instead of numbers, so the
  data is human-readable if you look directly at the SQL tables.
- **DTOs never expose entities directly** — controllers only ever see/return DTOs, keeping the API
  contract stable even if the internal models change.
>>>>>>> master
