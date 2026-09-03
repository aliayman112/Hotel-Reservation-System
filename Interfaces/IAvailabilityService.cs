using HotelReservation.Api.DTOs;

namespace HotelReservation.Api.Interfaces
{
    public interface IAvailabilityService
    {
        Task<List<AvailabilityResponseDto>> GetAvailabilityAsync(int roomTypeId, DateTime from, DateTime to);
        Task<bool> UpdateInventoryAsync(RoomInventoryUpdateDto dto);
    }
}
