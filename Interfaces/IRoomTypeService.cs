using HotelReservation.Api.DTOs;

namespace HotelReservation.Api.Interfaces
{
    public interface IRoomTypeService
    {
        Task<RoomTypeResponseDto?> CreateAsync(int hotelId, RoomTypeCreateDto dto);
        Task<bool> UpdateAsync(int roomTypeId, RoomTypeUpdateDto dto);
        Task<bool> DeleteAsync(int roomTypeId);
        Task<List<RoomTypeResponseDto>> GetByHotelAsync(int hotelId);
    }
}
