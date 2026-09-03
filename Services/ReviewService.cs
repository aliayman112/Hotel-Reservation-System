using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Data;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? Error, ReviewResponseDto? Review)> CreateAsync(int userId, ReviewCreateDto dto)
        {
            // Business rule from the docs: users can only review a hotel
            // AFTER a Completed booking there.
            var hasCompletedStay = await _context.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.HotelId == dto.HotelId &&
                b.Status == BookingStatus.Completed);

            if (!hasCompletedStay)
                return (false, "You can only review a hotel after a completed stay.", null);

            var review = new Review
            {
                UserId = userId,
                HotelId = dto.HotelId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return (true, null, new ReviewResponseDto
            {
                Id = review.Id,
                UserId = userId,
                UserName = user?.Name ?? string.Empty,
                HotelId = review.HotelId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            });
        }

        public async Task<List<ReviewResponseDto>> GetForHotelAsync(int hotelId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User.Name,
                HotelId = r.HotelId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
