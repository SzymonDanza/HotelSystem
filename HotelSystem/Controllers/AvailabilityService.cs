using System;
using System.Linq;
using HotelSystem.Data;
using HotelSystem.Models;

public class AvailabilityService
{
    private readonly ApplicationDbContext DbContext;

    public AvailabilityService(ApplicationDbContext context)
    {
        DbContext = context;
    }

    public void UpdateRoomAvailability(int year)
    {
        // Ustal zakres dat na cały rok
        var startDate = new DateTime(year, 1, 1); // 1 stycznia
        var endDate = new DateTime(year, 12, 31); // 31 grudnia

        // Pobierz wszystkie pokoje z bazy danych
        var rooms = DbContext.Rooms.ToList();

        // Iteruj po każdej dacie w podanym zakresie
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var room in rooms)
            {
                // Sprawdź, czy istnieje już wpis dla danego pokoju i daty
                var existingAvailability = DbContext.RoomAvailabilities
                                                    .FirstOrDefault(r => r.RoomId == room.Id && r.Date == date);

                if (existingAvailability == null)
                {
                    // Twórz nowy wpis dla tego pokoju i daty
                    var newAvailability = new RoomAvailability
                    {
                        RoomId = room.Id,
                        Date = date,
                        Availability = true // Domyślnie zakładamy, że pokój jest dostępny
                    };

                    DbContext.RoomAvailabilities.Add(newAvailability);
                }
            }
        }

        // Zapisz zmiany w bazie danych
        DbContext.SaveChanges();
    }
}
