using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.DTOs
{
    public class RoomTypeCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(1, 20)]
        public int Capacity { get; set; }

        [Required]
        public string BedType { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        public string? Description { get; set; }
    }

    public class RoomTypeUpdateDto : RoomTypeCreateDto { }

    public class RoomTypeResponseDto
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string BedType { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? Description { get; set; }
    }
}
