namespace HotelReservation.Api.Models
{
    // Optional extension: users can review a hotel after a Completed booking.
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Hotel Hotel { get; set; } = null!;
    }
}
