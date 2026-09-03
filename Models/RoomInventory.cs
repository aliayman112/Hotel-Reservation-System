namespace HotelReservation.Api.Models
{
    // One row = "on this date, this room type has this many total rooms,
    // and this many are already sold/reserved".
    public class RoomInventory
    {
        public int Id { get; set; }
        public int RoomTypeId { get; set; }
        public DateTime Date { get; set; }
        public int TotalRooms { get; set; }
        public int SoldRooms { get; set; }

        // Navigation
        public RoomType RoomType { get; set; } = null!;
    }
}
