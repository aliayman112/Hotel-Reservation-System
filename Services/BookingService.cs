using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Data;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // DESIGN NOTE (worth understanding):
        // We reserve the rooms (increment sold_rooms) the MOMENT a booking
        // is created as Pending, not when the Admin confirms it. This is
        // what actually prevents double booking - two users can't both grab
        // the last room while the first request is still waiting for Admin.
        // If the Admin later Rejects it (or the user/Admin Cancels it),
        // we release the rooms again by decrementing sold_rooms.
        // Confirming a Pending booking does NOT touch inventory, because
        // the rooms were already reserved when the booking was created.
        // ---------------------------------------------------------------

        public async Task<(bool Success, string? Error, BookingResponseDto? Booking)> CreateAsync(int userId, BookingCreateDto dto)
        {
            if (dto.CheckIn.Date >= dto.CheckOut.Date)
                return (false, "Check-out date must be after check-in date.", null);

            var roomType = await _context.RoomTypes
                .Include(rt => rt.Hotel)
                .FirstOrDefaultAsync(rt => rt.Id == dto.RoomTypeId && rt.HotelId == dto.HotelId);

            if (roomType == null)
                return (false, "Room type not found for this hotel.", null);

            var dates = GetDateRange(dto.CheckIn.Date, dto.CheckOut.Date); // check-in inclusive, check-out exclusive

            var inventoryRows = await _context.RoomInventories
                .Where(ri => ri.RoomTypeId == dto.RoomTypeId && dates.Contains(ri.Date))
                .ToListAsync();

            // Every night of the stay must have an inventory row with room(s) left.
            foreach (var date in dates)
            {
                var row = inventoryRows.FirstOrDefault(r => r.Date == date);
                if (row == null || row.SoldRooms >= row.TotalRooms)
                    return (false, $"No rooms available on {date:yyyy-MM-dd}.", null);
            }

            // Reserve the rooms for every night of the stay.
            foreach (var date in dates)
            {
                var row = inventoryRows.First(r => r.Date == date);
                row.SoldRooms += 1;
            }

            var nights = dates.Count;
            var booking = new Booking
            {
                UserId = userId,
                HotelId = dto.HotelId,
                RoomTypeId = dto.RoomTypeId,
                CheckIn = dto.CheckIn.Date,
                CheckOut = dto.CheckOut.Date,
                Nights = nights,
                TotalPrice = nights * roomType.BasePrice,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return (true, null, await ToDtoAsync(booking.Id));
        }

        public async Task<List<BookingResponseDto>> GetForUserAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.RoomType)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToDto).ToList();
        }

        public async Task<(bool Success, string? Error)> CancelAsync(int userId, int bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != BookingStatus.Confirmed)
                return (false, "Only confirmed bookings can be cancelled.");

            await ReleaseInventoryAsync(booking);
            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<List<BookingResponseDto>> GetAllAsync(string? status)
        {
            var query = _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.RoomType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
                query = query.Where(b => b.Status == parsedStatus);

            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return bookings.Select(MapToDto).ToList();
        }

        public async Task<(bool Success, string? Error)> ConfirmAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                return (false, "Only pending bookings can be confirmed.");

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> RejectAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != BookingStatus.Pending)
                return (false, "Only pending bookings can be rejected.");

            await ReleaseInventoryAsync(booking);
            booking.Status = BookingStatus.Rejected;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> AdminCancelAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                return (false, "Only pending or confirmed bookings can be cancelled.");

            await ReleaseInventoryAsync(booking);
            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CompleteAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != BookingStatus.Confirmed)
                return (false, "Only confirmed bookings can be marked completed.");

            booking.Status = BookingStatus.Completed;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ---- helpers ----

        private async Task ReleaseInventoryAsync(Booking booking)
        {
            var dates = GetDateRange(booking.CheckIn.Date, booking.CheckOut.Date);
            var rows = await _context.RoomInventories
                .Where(ri => ri.RoomTypeId == booking.RoomTypeId && dates.Contains(ri.Date))
                .ToListAsync();

            foreach (var row in rows)
            {
                if (row.SoldRooms > 0)
                    row.SoldRooms -= 1;
            }
        }

        private static List<DateTime> GetDateRange(DateTime checkIn, DateTime checkOut)
        {
            var dates = new List<DateTime>();
            for (var d = checkIn; d < checkOut; d = d.AddDays(1))
                dates.Add(d);
            return dates;
        }

        private async Task<BookingResponseDto> ToDtoAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Hotel)
                .Include(b => b.RoomType)
                .FirstAsync(b => b.Id == bookingId);

            return MapToDto(booking);
        }

        private static BookingResponseDto MapToDto(Booking b)
        {
            return new BookingResponseDto
            {
                Id = b.Id,
                UserId = b.UserId,
                HotelId = b.HotelId,
                HotelName = b.Hotel.Name,
                RoomTypeId = b.RoomTypeId,
                RoomTypeName = b.RoomType.Name,
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                Nights = b.Nights,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt
            };
        }
    }
}
