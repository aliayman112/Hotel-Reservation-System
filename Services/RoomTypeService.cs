using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Data;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Services
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly AppDbContext _context;

        public RoomTypeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoomTypeResponseDto?> CreateAsync(int hotelId, RoomTypeCreateDto dto)
        {
            var hotelExists = await _context.Hotels.AnyAsync(h => h.Id == hotelId);
            if (!hotelExists)
                return null;

            var roomType = new RoomType
            {
                HotelId = hotelId,
                Name = dto.Name,
                Capacity = dto.Capacity,
                BedType = dto.BedType,
                BasePrice = dto.BasePrice,
                Description = dto.Description
            };

            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return ToDto(roomType);
        }

        public async Task<bool> UpdateAsync(int roomTypeId, RoomTypeUpdateDto dto)
        {
            var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
            if (roomType == null)
                return false;

            roomType.Name = dto.Name;
            roomType.Capacity = dto.Capacity;
            roomType.BedType = dto.BedType;
            roomType.BasePrice = dto.BasePrice;
            roomType.Description = dto.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int roomTypeId)
        {
            var roomType = await _context.RoomTypes.FindAsync(roomTypeId);
            if (roomType == null)
                return false;

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoomTypeResponseDto>> GetByHotelAsync(int hotelId)
        {
            // NOTE: We fetch the entities first with ToListAsync(), THEN map to DTOs
            // with a normal in-memory .Select(). EF Core can't translate a call to
            // our own ToDto() helper into SQL, so calling it inside the database
            // query would throw at runtime.
            var roomTypes = await _context.RoomTypes
                .Where(rt => rt.HotelId == hotelId)
                .ToListAsync();

            return roomTypes.Select(ToDto).ToList();
        }

        private static RoomTypeResponseDto ToDto(RoomType rt)
        {
            return new RoomTypeResponseDto
            {
                Id = rt.Id,
                HotelId = rt.HotelId,
                Name = rt.Name,
                Capacity = rt.Capacity,
                BedType = rt.BedType,
                BasePrice = rt.BasePrice,
                Description = rt.Description
            };
        }
    }
}
