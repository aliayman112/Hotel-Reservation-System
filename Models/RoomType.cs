namespace HotelReservation.Api.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Deluxe", "Suite"
        public int Capacity { get; set; }
        public string BedType { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? Description { get; set; }

        // Navigation
        public Hotel Hotel { get; set; } = null!;
        public ICollection<RoomInventory> Inventory { get; set; } = new List<RoomInventory>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
