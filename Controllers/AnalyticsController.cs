using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Data;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Controllers
{
    // Optional extension: "Simple analytics for Admin (revenue, occupancy)".
    // Kept intentionally simple - a couple of aggregate numbers, not a
    // full reporting engine.
    [ApiController]
    [Route("api/admin/analytics")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/admin/analytics/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            // Revenue = sum of total_price for bookings that were actually honoured.
            var revenue = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

            var totalBookings = await _context.Bookings.CountAsync();
            var pendingCount = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            var confirmedCount = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);
            var cancelledCount = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);
            var completedCount = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Completed);

            // Occupancy = sold_rooms / total_rooms across all inventory rows we've configured.
            var totalRoomNights = await _context.RoomInventories.SumAsync(ri => (int?)ri.TotalRooms) ?? 0;
            var soldRoomNights = await _context.RoomInventories.SumAsync(ri => (int?)ri.SoldRooms) ?? 0;
            var occupancyRate = totalRoomNights == 0 ? 0 : Math.Round((double)soldRoomNights / totalRoomNights * 100, 1);

            return Ok(new
            {
                totalRevenue = revenue,
                totalBookings,
                pendingCount,
                confirmedCount,
                cancelledCount,
                completedCount,
                occupancyRatePercent = occupancyRate
            });
        }
    }
}
