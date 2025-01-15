using HotelSystem.Data;
using HotelSystem.Models;

namespace HotelSystem.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            // Dodaj użytkowników, jeśli nie istnieją
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        Password = "admin123",
                        IsAdmin = true // Admin
                    },
                    new User
                    {
                        Username = "user",
                        Password = "user123",
                        IsAdmin = false // Zwykły użytkownik
                    }
                );
                context.SaveChanges();
            }


            // Dodaj pokoje, jeśli nie istnieją
            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(
                    new Room { Name = "Pokój 1", Capacity = 2, PricePerNight = 100.00m },
                    new Room { Name = "Pokój 2", Capacity = 2, PricePerNight = 120.00m },
                    new Room { Name = "Pokój 3", Capacity = 3, PricePerNight = 150.00m },
                    new Room { Name = "Pokój 4", Capacity = 1, PricePerNight = 80.00m }
                );
                context.SaveChanges();
            }

            // Dodaj dostępność pokoi, jeśli nie istnieje
            if (!context.RoomAvailabilities.Any())
            {
                var today = DateTime.Now.Date;

                // Pobierz listę pokoi z bazy danych
                var rooms = context.Rooms.ToList();

                // Generuj dostępność w pamięci
                var roomAvailabilities = rooms.SelectMany(room =>
                    Enumerable.Range(0, 30).Select(offset => new RoomAvailability
                    {
                        RoomId = room.Id,
                        Date = today.AddDays(offset),
                        Availability = true
                    })
                ).ToList();

                // Zapisz dostępności w bazie danych
                context.RoomAvailabilities.AddRange(roomAvailabilities);
                context.SaveChanges();

            }
        }
    }
}