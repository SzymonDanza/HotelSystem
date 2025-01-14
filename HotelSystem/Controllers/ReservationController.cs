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

            // Pobranie dostępności pokoi na dany rok
            var roomsAvailability = DbContext.RoomAvailabilities
                                             .Where(r => r.Date.Year == year)
                                             .Include(r => r.Room)
                                             .ToList();

            // Przygotowanie modelu widoku
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

                // Dodaj puste dni przed pierwszym dniem miesiąca
                for (int i = 0; i < (int)firstDay; i++)
                {
                    monthDays.Add(null);
                }

                // Dodaj dni miesiąca z informacją o dostępności
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month + 1, day);
                    var roomAvailability = roomsAvailability.FirstOrDefault(r => r.Date == date);
                    var isAvailable = roomAvailability != null && roomAvailability.Availability;

                    monthDays.Add(new
                    {
                        Day = day,
                        CssClass = isAvailable ? "btn-danger" : "btn-success" // Klasa CSS dla dostępności
                    });
                }

                calendarDays.Add(monthDays);
            }

            return calendarDays;
        }

        public IActionResult Reserve(int year, int month, int day)
        {
            // Pobranie listy pokoi z bazy danych
            ViewBag.Rooms = DbContext.Rooms.ToList();

            // Sprawdzamy dostępność pokoi dla wybranego dnia
            var selectedDate = new DateTime(year, month, day);
            var roomAvailability = DbContext.RoomAvailabilities
                                            .FirstOrDefault(r => r.Date == selectedDate);

            // Jeśli dostępność pokoi na ten dzień jest null, ustawiamy dostępność na false
            if (roomAvailability == null)
            {
                roomAvailability = new RoomAvailability
                {
                    Date = selectedDate,
                    Availability = false // Zakładając, że pokój jest niedostępny, jeżeli brak rekordu
                };
            }

            // Zwracamy widok z modelem zawierającym dane o dostępności
            return View(new ReservationViewModel
            {
                SelectedDate = selectedDate,
                RoomAvailability = roomAvailability
            });
        }

        // Dodatkowe akcje rezerwacji, jeśli są wymagane
    }
}
