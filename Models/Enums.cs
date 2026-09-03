namespace HotelReservation.Api.Models
{
    // The three roles in the system.
    // Receptionist is the "optional extension" role from the docs:
    // it can confirm/reject bookings like Admin, but cannot manage hotels/rooms.
    public enum UserRole
    {
        User,
        Admin,
        Receptionist
    }

    // Every booking moves through these statuses over its lifetime.
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled,
        Completed
    }
}
