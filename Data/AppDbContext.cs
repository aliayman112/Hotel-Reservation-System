using Microsoft.EntityFrameworkCore;
using HotelReservation.Api.Models;

namespace HotelReservation.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<RoomInventory> RoomInventories => Set<RoomInventory>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store enums as readable strings in the DB instead of numbers.
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<string>();

            // A user's email must be unique (used for login).
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Decimal precision for money fields (avoids EF Core warnings).
            modelBuilder.Entity<RoomType>()
                .Property(r => r.BasePrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(10,2)");

            // Hotel -> RoomTypes (one hotel has many room types)
            modelBuilder.Entity<RoomType>()
                .HasOne(rt => rt.Hotel)
                .WithMany(h => h.RoomTypes)
                .HasForeignKey(rt => rt.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomType -> RoomInventory (one room type has many daily inventory rows)
            modelBuilder.Entity<RoomInventory>()
                .HasOne(ri => ri.RoomType)
                .WithMany(rt => rt.Inventory)
                .HasForeignKey(ri => ri.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // One room type can only have ONE inventory row per date.
            modelBuilder.Entity<RoomInventory>()
                .HasIndex(ri => new { ri.RoomTypeId, ri.Date })
                .IsUnique();

            // Booking relationships - restrict deletes so we don't accidentally
            // wipe out booking history when a hotel/room type/user is removed.
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.RoomType)
                .WithMany(rt => rt.Bookings)
                .HasForeignKey(b => b.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
