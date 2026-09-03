using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Data;
using HotelReservation.Api.DTOs;
using HotelReservation.Api.Interfaces;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Services
{
    public class HotelService : IHotelService
    {
        private readonly AppDbContext _context;

        public HotelService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HotelResponseDto>> SearchAsync(string? city)
        {
            var query = _context.Hotels.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(h => h.City.ToLower() == city.ToLower());

            return await query
                .Select(h => new HotelResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    City = h.City,
                    Address = h.Address,
                    Description = h.Description,
                    Stars = h.Stars,
                    ThumbnailUrl = h.ThumbnailUrl,
                    AverageRating = h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : null
                })
                .ToListAsync();
        }

        public async Task<HotelResponseDto?> GetByIdAsync(int id)
        {
            var hotel = await _context.Hotels
                .Where(h => h.Id == id)
                .Select(h => new HotelResponseDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    City = h.City,
                    Address = h.Address,
                    Description = h.Description,
                    Stars = h.Stars,
                    ThumbnailUrl = h.ThumbnailUrl,
                    AverageRating = h.Reviews.Any() ? h.Reviews.Average(r => r.Rating) : null
                })
                .FirstOrDefaultAsync();

            return hotel;
        }

        public async Task<HotelResponseDto> CreateAsync(HotelCreateDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                City = dto.City,
                Address = dto.Address,
                Description = dto.Description,
                Stars = dto.Stars,
                ThumbnailUrl = dto.ThumbnailUrl
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return new HotelResponseDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                City = hotel.City,
                Address = hotel.Address,
                Description = hotel.Description,
                Stars = hotel.Stars,
                ThumbnailUrl = hotel.ThumbnailUrl,
                AverageRating = null
            };
        }

        public async Task<bool> UpdateAsync(int id, HotelUpdateDto dto)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return false;

            hotel.Name = dto.Name;
            hotel.City = dto.City;
            hotel.Address = dto.Address;
            hotel.Description = dto.Description;
            hotel.Stars = dto.Stars;
            hotel.ThumbnailUrl = dto.ThumbnailUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
                return false;

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
