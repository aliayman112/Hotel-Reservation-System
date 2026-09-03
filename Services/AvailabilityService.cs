using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Data;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _context;

        public AvailabilityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AvailabilityResponseDto>> GetAvailabilityAsync(int roomTypeId, DateTime from, DateTime to)
        {
            var rows = await _context.RoomInventories
                .Where(ri => ri.RoomTypeId == roomTypeId && ri.Date >= from.Date && ri.Date <= to.Date)
                .OrderBy(ri => ri.Date)
                .ToListAsync();

            return rows.Select(r => new AvailabilityResponseDto
            {
                Date = r.Date,
                TotalRooms = r.TotalRooms,
                SoldRooms = r.SoldRooms
            }).ToList();
        }

        // Upsert: if an inventory row already exists for this room type + date,
        // update its total_rooms. Otherwise create a new row with sold_rooms = 0.
        public async Task<bool> UpdateInventoryAsync(RoomInventoryUpdateDto dto)
        {
            var roomTypeExists = await _context.RoomTypes.AnyAsync(rt => rt.Id == dto.RoomTypeId);
            if (!roomTypeExists)
                return false;

            var existing = await _context.RoomInventories.FirstOrDefaultAsync(
                ri => ri.RoomTypeId == dto.RoomTypeId && ri.Date.Date == dto.Date.Date);

            if (existing != null)
            {
                existing.TotalRooms = dto.TotalRooms;
            }
            else
            {
                _context.RoomInventories.Add(new RoomInventory
                {
                    RoomTypeId = dto.RoomTypeId,
                    Date = dto.Date.Date,
                    TotalRooms = dto.TotalRooms,
                    SoldRooms = 0
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
