using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;

namespace HotelReservation.Api.Controllers
{
    [ApiController]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;

        public RoomTypesController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        // GET /api/hotels/{hotelId}/room-types
        [HttpGet("api/hotels/{hotelId}/room-types")]
        public async Task<IActionResult> GetByHotel(int hotelId)
        {
            var roomTypes = await _roomTypeService.GetByHotelAsync(hotelId);
            return Ok(roomTypes);
        }

        // POST /api/admin/hotels/{id}/room-types
        [HttpPost("api/admin/hotels/{id}/room-types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int id, RoomTypeCreateDto dto)
        {
            var roomType = await _roomTypeService.CreateAsync(id, dto);
            if (roomType == null)
                return NotFound(new { message = "Hotel not found." });

            return CreatedAtAction(nameof(GetByHotel), new { hotelId = id }, roomType);
        }

        // PUT /api/admin/room-types/{id}
        [HttpPut("api/admin/room-types/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, RoomTypeUpdateDto dto)
        {
            var updated = await _roomTypeService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = "Room type not found." });

            return NoContent();
        }

        // DELETE /api/admin/room-types/{id}
        [HttpDelete("api/admin/room-types/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _roomTypeService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Room type not found." });

            return NoContent();
        }
    }
}
