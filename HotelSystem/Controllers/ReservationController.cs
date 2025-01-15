﻿using Microsoft.AspNetCore.Mvc;
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
                    var roomAvailability = roomsAvailability.FirstOrDefault(r => r.Date == date);
                    var isAvailable = roomAvailability != null && roomAvailability.Availability;

                    monthDays.Add(new
                    {
                        Day = day,
                        CssClass = isAvailable ? "available" : "unavailable"
                    });
                }

                calendarDays.Add(monthDays);
            }

            return calendarDays;
        }

        public IActionResult Reserve(int year, int month, int day)
        {
            var selectedDate = new DateTime(year, month, day);

            var availableRoomIds = DbContext.RoomAvailabilities
                                             .Where(r => r.Date == selectedDate && r.Availability)
                                             .Select(r => r.RoomId)
                                             .ToList();

            var rooms = DbContext.Rooms
                                 .Where(r => availableRoomIds.Contains(r.Id))
                                 .ToList();

            var viewModel = new ReservationViewModel
            {
                SelectedDate = selectedDate,
                AvailableRooms = rooms
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReservation(int roomId, DateTime selectedDate)
        {
            var userId = User.Identity.IsAuthenticated ? int.Parse(User.Identity.Name) : 0;

            var roomAvailability = await DbContext.RoomAvailabilities
                                                  .FirstOrDefaultAsync(r => r.RoomId == roomId && r.Date == selectedDate);

            if (roomAvailability == null || !roomAvailability.Availability)
            {
                ModelState.AddModelError("", "Pokój jest niedostępny w wybranym dniu.");
                return View();
            }

            var reservation = new Reservation
            {
                RoomId = roomId,
                StartDate = selectedDate,
                EndDate = selectedDate.AddDays(1),
                UserId = userId,
                User = DbContext.Users.FirstOrDefault(u => u.Id == userId)
            };

            DbContext.Reservations.Add(reservation);
            roomAvailability.Availability = false;

            await DbContext.SaveChangesAsync();

            return RedirectToAction("ReservationConfirmation", new { reservationId = reservation.Id });
        }
    }
}
