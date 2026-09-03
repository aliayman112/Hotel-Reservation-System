using HotelReservation.Api.DTOs;

namespace HotelReservation.Api.Interfaces
{
    public interface IHotelService
    {
        Task<List<HotelResponseDto>> SearchAsync(string? city);
        Task<HotelResponseDto?> GetByIdAsync(int id);
        Task<HotelResponseDto> CreateAsync(HotelCreateDto dto);
        Task<bool> UpdateAsync(int id, HotelUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
