using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;

namespace HotelReservation.Api.Controllers
{
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        // GET /api/room-types/{id}/availability?from=2026-09-01&to=2026-09-10
        [HttpGet("api/room-types/{id}/availability")]
        public async Task<IActionResult> GetAvailability(int id, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from.Date > to.Date)
                return BadRequest(new { message = "'from' date must be before or equal to 'to' date." });

            var availability = await _availabilityService.GetAvailabilityAsync(id, from, to);
            return Ok(availability);
        }

        // PUT /api/admin/room-inventory
        [HttpPut("api/admin/room-inventory")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInventory(RoomInventoryUpdateDto dto)
        {
            var updated = await _availabilityService.UpdateInventoryAsync(dto);
            if (!updated)
                return NotFound(new { message = "Room type not found." });

            return NoContent();
        }
    }
}
