using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;

namespace HotelReservation.Api.Controllers
{
    [ApiController]
    [Authorize] // every action here requires a logged-in user
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST /api/bookings
        [HttpPost("api/bookings")]
        public async Task<IActionResult> Create(BookingCreateDto dto)
        {
            var userId = this.GetUserId();
            var (success, error, booking) = await _bookingService.CreateAsync(userId, dto);

            if (!success)
                return BadRequest(new { message = error });

            return CreatedAtAction(nameof(GetMyBookings), null, booking);
        }

        // GET /api/me/bookings
        [HttpGet("api/me/bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = this.GetUserId();
            var bookings = await _bookingService.GetForUserAsync(userId);
            return Ok(bookings);
        }

        // PATCH /api/bookings/{id}/cancel
        [HttpPatch("api/bookings/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = this.GetUserId();
            var (success, error) = await _bookingService.CancelAsync(userId, id);

            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }
    }
}
