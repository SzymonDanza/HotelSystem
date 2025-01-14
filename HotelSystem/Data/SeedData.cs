using HotelSystem.Data;
using HotelSystem.Models;

public class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider, ApplicationDbContext context)
    {
        // Sprawdzanie, czy w tabeli Rooms są już dane
        if (!context.Rooms.Any())
        {
            var rooms = new List<Room>
            {
                new Room { Name = "Pokój 1", Capacity = 2, PricePerNight = 100.00m },
                new Room { Name = "Pokój 2", Capacity = 2, PricePerNight = 120.00m },
                new Room { Name = "Pokój 3", Capacity = 3, PricePerNight = 150.00m },
                new Room { Name = "Pokój 4", Capacity = 1, PricePerNight = 80.00m }
            };

            context.Rooms.AddRange(rooms);
            context.SaveChanges();
        }
    }
}
