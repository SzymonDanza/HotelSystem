using HotelSystem.Data;
using HotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var isAvailable = roomsAvailability.Any(r => r.Date == date && r.Availability);

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
    public async Task<IActionResult> ConfirmReservation(int roomId, DateTime startDate, DateTime endDate)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account"); 
        }

        var userId = int.Parse(User.Identity.Name);

        
        if (endDate < startDate)
        {
            ModelState.AddModelError("", "Zła data rezerwacji. Data zakończenia musi być późniejsza niż data rozpoczęcia.");
            return View(); 
        }

        
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            ModelState.AddModelError("", "Użytkownik nie został znaleziony.");
            return View(); 
        }

        
        var roomAvailabilities = await DbContext.RoomAvailabilities
                                                .Where(r => r.RoomId == roomId && r.Date >= startDate && r.Date <= endDate)
                                                .ToListAsync();

        
        if (roomAvailabilities.Any(r => !r.Availability))
        {
            ModelState.AddModelError("", "Pokój jest niedostępny w wybranym okresie.");
            return View(); 
        }

        
        var reservation = new Reservation
        {
            RoomId = roomId,
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            User = user 
        };

        DbContext.Reservations.Add(reservation);

        
        foreach (var roomAvailability in roomAvailabilities)
        {
            roomAvailability.Availability = false;
        }

        await DbContext.SaveChangesAsync();

        
        return RedirectToAction("ReservationConfirmation", new { reservationId = reservation.Id });
    }



    private void UpdateRoomAvailabilityAfterReservation(int roomId, DateTime selectedDate)
    {
       
        var availabilityEntry = DbContext.RoomAvailabilities
                                        .FirstOrDefault(r => r.RoomId == roomId && r.Date == selectedDate);

        if (availabilityEntry != null)
        {
            availabilityEntry.Availability = false;
            DbContext.SaveChanges();
        }
    }

    
    public IActionResult ReservationConfirmation(int reservationId)
    {
        var reservation = DbContext.Reservations
                                   .Include(r => r.Room)
                                   .Include(r => r.User)
                                   .FirstOrDefault(r => r.Id == reservationId);

        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    public async Task<IActionResult> MyReservations()
    {
       
        var userId = User.Identity.IsAuthenticated ? int.Parse(User.Identity.Name) : 0;

        
        var reservations = await DbContext.Reservations
                                           .Where(r => r.UserId == userId)
                                           .Include(r => r.Room)
                                           .ToListAsync();

        return View(reservations);
    }

    
    [HttpPost]
    public async Task<IActionResult> DeleteReservation(int reservationId)
    {
        
        var reservation = await DbContext.Reservations
                                          .Include(r => r.Room)
                                          .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
        {
            return NotFound(); 
        }

        
        var userId = User.Identity.IsAuthenticated ? int.Parse(User.Identity.Name) : 0;

        if (reservation.UserId != userId)
        {
            return Forbid(); 
        }

        
        DbContext.Reservations.Remove(reservation);

        
        var roomAvailabilities = await DbContext.RoomAvailabilities
                                                 .Where(r => r.RoomId == reservation.RoomId && r.Date >= reservation.StartDate && r.Date <= reservation.EndDate)
                                                 .ToListAsync();

        foreach (var roomAvailability in roomAvailabilities)
        {
            roomAvailability.Availability = true; 
        }

        
        await DbContext.SaveChangesAsync();

        
        return RedirectToAction(nameof(MyReservations));
    }



}
