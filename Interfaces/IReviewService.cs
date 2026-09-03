using HotelReservation.Api.DTOs;

namespace HotelReservation.Api.Interfaces
{
    public interface IReviewService
    {
        Task<(bool Success, string? Error, ReviewResponseDto? Review)> CreateAsync(int userId, ReviewCreateDto dto);
        Task<List<ReviewResponseDto>> GetForHotelAsync(int hotelId);
    }
}
