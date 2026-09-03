using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;

namespace HotelReservation.Api.Controllers
{
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        // GET /api/hotels?city=Cairo
        [HttpGet("api/hotels")]
        public async Task<IActionResult> Search([FromQuery] string? city)
        {
            var hotels = await _hotelService.SearchAsync(city);
            return Ok(hotels);
        }

        // GET /api/hotels/{id}
        [HttpGet("api/hotels/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
                return NotFound(new { message = "Hotel not found." });

            return Ok(hotel);
        }

        // POST /api/admin/hotels
        [HttpPost("api/admin/hotels")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(HotelCreateDto dto)
        {
            var hotel = await _hotelService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, hotel);
        }

        // PUT /api/admin/hotels/{id}
        [HttpPut("api/admin/hotels/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, HotelUpdateDto dto)
        {
            var updated = await _hotelService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = "Hotel not found." });

            return NoContent();
        }

        // DELETE /api/admin/hotels/{id}
        [HttpDelete("api/admin/hotels/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _hotelService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Hotel not found." });

            return NoContent();
        }
    }
}
