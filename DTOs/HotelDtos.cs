using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Api.DTOs
{
    public class HotelCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, 5)]
        public int Stars { get; set; }

        public string? ThumbnailUrl { get; set; }
    }

    public class HotelUpdateDto : HotelCreateDto { }

    public class HotelResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Stars { get; set; }
        public string? ThumbnailUrl { get; set; }
        public double? AverageRating { get; set; }
    }
}
