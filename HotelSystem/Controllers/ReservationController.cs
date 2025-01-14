using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelSystem.Data;
using HotelSystem.Models;
using System;
using System.Linq;

namespace HotelSystem.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<dynamic> CalendarDays { get; private set; }

        public IActionResult Calendar(int roomId, int year, int month)
        {
            // Pobierz dostępne pokoje
            var room = _context.Rooms.Include(r => r.Reservations)
                                      .FirstOrDefault(r => r.Id == roomId);
            if (room == null)
            {
                return NotFound();
            }

           
            var reservations = _context.Reservations
                .Where(r => r.RoomId == roomId && r.StartDate.Year == year && r.StartDate.Month == month)
                .ToList();

            
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var calendarDays = Enumerable.Range(1, daysInMonth)
                                         .Select(day => new
                                         {
                                             Day = day,
                                             IsReserved = reservations.Any(r => r.StartDate.Day <= day && r.EndDate.Day >= day)
                                         })
                                         .ToList();

            
            var model = new CalendarViewModel
            {
                RoomId = roomId,
                Year = year,
                Month = month,
                CalendarDays = CalendarDays
            };

            return View(model);
        }
    }
}
