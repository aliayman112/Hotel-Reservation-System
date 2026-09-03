using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Api.Interfaces;

namespace HotelReservation.Api.Controllers
{
    [ApiController]
    [Route("api/admin/bookings")]
    // Both Admin and Receptionist can confirm/reject bookings (per the optional
    // "Receptionist" extension in the docs). Only Admin can cancel outright,
    // since cancelling touches inventory more permanently - see the [Authorize]
    // on the Cancel action below, which narrows this further.
    [Authorize(Roles = "Admin,Receptionist")]
    public class AdminBookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminBookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET /api/admin/bookings?status=PENDING
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var bookings = await _bookingService.GetAllAsync(status);
            return Ok(bookings);
        }

        // PATCH /api/admin/bookings/{id}/confirm
        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var (success, error) = await _bookingService.ConfirmAsync(id);
            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }

        // PATCH /api/admin/bookings/{id}/reject
        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var (success, error) = await _bookingService.RejectAsync(id);
            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }

        // PATCH /api/admin/bookings/{id}/cancel
        // Admin-only: cancelling a confirmed booking on the guest's behalf.
        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var (success, error) = await _bookingService.AdminCancelAsync(id);
            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }

        // PATCH /api/admin/bookings/{id}/complete
        // Marks a booking Completed, typically run after the check-out date passes.
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var (success, error) = await _bookingService.CompleteAsync(id);
            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }
    }
}
