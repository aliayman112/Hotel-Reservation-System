using HotelReservation.Api.DTOs;

namespace HotelReservation.Api.Interfaces
{
    public interface IBookingService
    {
        Task<(bool Success, string? Error, BookingResponseDto? Booking)> CreateAsync(int userId, BookingCreateDto dto);
        Task<List<BookingResponseDto>> GetForUserAsync(int userId);
        Task<(bool Success, string? Error)> CancelAsync(int userId, int bookingId);

        // Admin / Receptionist operations
        Task<List<BookingResponseDto>> GetAllAsync(string? status);
        Task<(bool Success, string? Error)> ConfirmAsync(int bookingId);
        Task<(bool Success, string? Error)> RejectAsync(int bookingId);
        Task<(bool Success, string? Error)> AdminCancelAsync(int bookingId);
        Task<(bool Success, string? Error)> CompleteAsync(int bookingId);
    }
}
