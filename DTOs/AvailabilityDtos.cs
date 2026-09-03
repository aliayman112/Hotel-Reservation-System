using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.DTOs
{
    public class AvailabilityResponseDto
    {
        public DateTime Date { get; set; }
        public int TotalRooms { get; set; }
        public int SoldRooms { get; set; }
        public int AvailableRooms => TotalRooms - SoldRooms;
    }

    // Admin sets total room count for a specific room type + date.
    public class RoomInventoryUpdateDto
    {
        [Required]
        public int RoomTypeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 1000)]
        public int TotalRooms { get; set; }
    }
}
