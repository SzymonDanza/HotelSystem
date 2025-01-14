using Microsoft.AspNetCore.Mvc;
using HotelSystem.Models;
using System.Linq;
using HotelSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext DbContext;

        public ReservationController(ApplicationDbContext context)
        {
            DbContext = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Now;
            int year = today.Year;

           
            var roomsAvailability = DbContext.RoomAvailabilities
                                             .Where(r => r.Date.Year == year)
                                             .Include(r => r.Room) 
                                             .ToList();

            
            var calendarViewModel = new CalendarViewModel
            {
                Year = year,
                CalendarDays = GenerateCalendarData(year, roomsAvailability)
            };

            return View(calendarViewModel);
        }

        private List<dynamic> GenerateCalendarData(int year, List<RoomAvailability> roomsAvailability)
        {
            var calendarDays = new List<dynamic>();

            for (int month = 0; month < 12; month++)
            {
                var monthDays = new List<dynamic>();
                var daysInMonth = DateTime.DaysInMonth(year, month + 1);
                var firstDay = new DateTime(year, month + 1, 1).DayOfWeek;

                
                for (int i = 0; i < (int)firstDay; i++)
                {
                    monthDays.Add(null);
                }

                
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month + 1, day);

                    
                    var roomAvailability = roomsAvailability
                        .Where(r => r.Date == date)
                        .FirstOrDefault();

                    
                    var available = roomAvailability != null && roomAvailability.Availability;

                    monthDays.Add(new { Day = day, IsAvailable = available });
                }

                calendarDays.Add(monthDays);
            }

            return calendarDays;
        }
    }
}
