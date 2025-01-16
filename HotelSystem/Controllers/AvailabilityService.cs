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
        //  zakres dat na cały rok
        var startDate = new DateTime(year, 1, 1); 
        var endDate = new DateTime(year, 12, 31); 

        var rooms = DbContext.Rooms.ToList();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var room in rooms)
            {
                
                var existingAvailability = DbContext.RoomAvailabilities
                                                    .FirstOrDefault(r => r.RoomId == room.Id && r.Date == date);

                if (existingAvailability == null)
                {
                    
                    var newAvailability = new RoomAvailability
                    {
                        RoomId = room.Id,
                        Date = date,
                        Availability = true 
                    };

                    DbContext.RoomAvailabilities.Add(newAvailability);
                }
            }
        }

        
        DbContext.SaveChanges();
    }
}
