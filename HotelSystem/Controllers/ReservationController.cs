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
            return RedirectToAction("Login", "Account"); // Redirect to login page
        }

        var userId = int.Parse(User.Identity.Name);

        // Validate if the end date is before or equal to the start date
        if (endDate < startDate)
        {
            ModelState.AddModelError("", "Zła data rezerwacji. Data zakończenia musi być późniejsza niż data rozpoczęcia.");
            return View(); // Return the same view with the error
        }

        // Check if the user exists
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            ModelState.AddModelError("", "Użytkownik nie został znaleziony.");
            return View(); // Return the view with error
        }

        // Check the availability of the room for the selected date range
        var roomAvailabilities = await DbContext.RoomAvailabilities
                                                .Where(r => r.RoomId == roomId && r.Date >= startDate && r.Date <= endDate)
                                                .ToListAsync();

        // If any day within the range is unavailable, display an error message
        if (roomAvailabilities.Any(r => !r.Availability))
        {
            ModelState.AddModelError("", "Pokój jest niedostępny w wybranym okresie.");
            return View(); // Return the view with error
        }

        // Create the reservation
        var reservation = new Reservation
        {
            RoomId = roomId,
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            User = user // Assign the user
        };

        // Add the reservation to the database
        DbContext.Reservations.Add(reservation);

        // Mark the room as unavailable for the selected dates
        foreach (var roomAvailability in roomAvailabilities)
        {
            roomAvailability.Availability = false;
        }

        await DbContext.SaveChangesAsync();

        // Redirect to the reservation confirmation page
        return RedirectToAction("ReservationConfirmation", new { reservationId = reservation.Id });
    }



    private void UpdateRoomAvailabilityAfterReservation(int roomId, DateTime selectedDate)
    {
        // Zmieniamy dostępność pokoju na 'false' dla wybranego dnia
        var availabilityEntry = DbContext.RoomAvailabilities
                                        .FirstOrDefault(r => r.RoomId == roomId && r.Date == selectedDate);

        if (availabilityEntry != null)
        {
            availabilityEntry.Availability = false;
            DbContext.SaveChanges();
        }
    }

    // Opcjonalnie, jeśli masz metodę potwierdzenia rezerwacji:
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
        // Sprawdzamy, czy użytkownik jest zalogowany
        var userId = User.Identity.IsAuthenticated ? int.Parse(User.Identity.Name) : 0;

        // Pobieramy rezerwacje zalogowanego użytkownika
        var reservations = await DbContext.Reservations
                                           .Where(r => r.UserId == userId)
                                           .Include(r => r.Room)
                                           .ToListAsync();

        return View(reservations);
    }

    
    [HttpPost]
    public async Task<IActionResult> DeleteReservation(int reservationId)
    {
        // Pobieramy rezerwację
        var reservation = await DbContext.Reservations
                                          .Include(r => r.Room)
                                          .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
        {
            return NotFound(); // Zwracamy stronę 404, jeśli rezerwacja nie istnieje
        }

        // Sprawdzamy, czy użytkownik jest właścicielem rezerwacji
        var userId = User.Identity.IsAuthenticated ? int.Parse(User.Identity.Name) : 0;

        if (reservation.UserId != userId)
        {
            return Forbid(); // Brak dostępu
        }

        // Usuwamy rezerwację
        DbContext.Reservations.Remove(reservation);

        // Zwalniamy wszystkie dni od rozpoczęcia do zakończenia rezerwacji
        var roomAvailabilities = await DbContext.RoomAvailabilities
                                                 .Where(r => r.RoomId == reservation.RoomId && r.Date >= reservation.StartDate && r.Date <= reservation.EndDate)
                                                 .ToListAsync();

        foreach (var roomAvailability in roomAvailabilities)
        {
            roomAvailability.Availability = true; // Ustawiamy dostępność na true (zwalniamy dzień)
        }

        // Zapisujemy zmiany w bazie danych
        await DbContext.SaveChangesAsync();

        // Przekierowanie z powrotem do listy rezerwacji
        return RedirectToAction(nameof(MyReservations));
    }



}
