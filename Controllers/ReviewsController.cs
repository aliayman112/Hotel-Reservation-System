using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;

namespace HotelReservation.Api.Controllers
{
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST /api/reviews  (logged-in users only, and only after a Completed stay)
        [HttpPost("api/reviews")]
        [Authorize]
        public async Task<IActionResult> Create(ReviewCreateDto dto)
        {
            var userId = this.GetUserId();
            var (success, error, review) = await _reviewService.CreateAsync(userId, dto);

            if (!success)
                return BadRequest(new { message = error });

            return CreatedAtAction(nameof(GetForHotel), new { hotelId = dto.HotelId }, review);
        }

        // GET /api/hotels/{hotelId}/reviews  (public - anyone can read reviews)
        [HttpGet("api/hotels/{hotelId}/reviews")]
        public async Task<IActionResult> GetForHotel(int hotelId)
        {
            var reviews = await _reviewService.GetForHotelAsync(hotelId);
            return Ok(reviews);
        }
    }
}
