using Microsoft.AspNetCore.Mvc;
using HotelSystem.Models;
using HotelSystem.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class RoomController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Akcja wyświetlająca dostępne pokoje w formie kalendarza
    public IActionResult Calendar()
    {
        var rooms = _context.Rooms.Include(r => r.Reservations).ToList();

        // Załóżmy, że chcesz wyświetlić dostępność na cały miesiąc
        var startDate = DateTime.Now;
        var endDate = startDate.AddMonths(1);

        var availableRooms = new List<RoomAvailabilityModel>();

        foreach (var room in rooms)
        {
            var availability = new RoomAvailabilityModel
            {
                RoomId = room.Id,
                RoomName = room.Name,
                Availability = new Dictionary<DateTime, bool>()
            };

            // Generowanie dostępności dla każdego dnia w okresie
            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var isReserved = room.Reservations.Any(r => r.StartDate <= day && r.EndDate >= day);
                availability.Availability[day] = !isReserved; // True jeśli dostępny, False jeśli zarezerwowany
            }

            availableRooms.Add(availability);
        }

        return View(availableRooms);
    }
}