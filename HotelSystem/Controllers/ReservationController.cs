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
    public async Task<IActionResult> ConfirmReservation(int roomId, DateTime selectedDate)
    {
        // Sprawdzenie, czy użytkownik jest zalogowany
        var userId = User.Identity.IsAuthenticated ? int.Parse(User.Identity.Name) : 0;

        // Sprawdzamy, czy użytkownik istnieje
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            ModelState.AddModelError("", "Użytkownik nie został znaleziony.");
            return View(); // Możesz przekazać odpowiedni widok z komunikatem o błędzie
        }

        // Sprawdzamy dostępność pokoju na wybraną datę
        var roomAvailability = await DbContext.RoomAvailabilities
                                               .FirstOrDefaultAsync(r => r.RoomId == roomId && r.Date == selectedDate);

        if (roomAvailability == null || !roomAvailability.Availability)
        {
            ModelState.AddModelError("", "Pokój jest niedostępny w wybranym dniu.");

            // Tworzymy model widoku, aby przekazać go do widoku
            var availableRooms = DbContext.RoomAvailabilities
                                          .Where(r => r.Date == selectedDate && r.Availability)
                                          .Select(r => r.Room)
                                          .ToList();

            var viewModel = new ReservationViewModel
            {
                SelectedDate = selectedDate,
                AvailableRooms = availableRooms
            };

            // Zwracamy widok z odpowiednim modelem
            return View(viewModel);
        }

        // Sprawdzamy, czy pokój istnieje
        var room = await DbContext.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null)
        {
            ModelState.AddModelError("", "Pokój nie został znaleziony.");
            return View(); // Możesz przekazać odpowiedni widok z komunikatem o błędzie
        }

        // Tworzymy rezerwację
        var reservation = new Reservation
        {
            RoomId = roomId,
            StartDate = selectedDate,
            EndDate = selectedDate.AddDays(1), // Zakładamy, że rezerwacja jest na 1 dzień
            UserId = userId,
            User = user  // Przypisujemy użytkownika
        };

        // Dodajemy rezerwację do bazy danych
        DbContext.Reservations.Add(reservation);

        // Zmieniamy dostępność pokoju na niedostępny
        roomAvailability.Availability = false;

        // Zapisujemy zmiany w bazie danych
        await DbContext.SaveChangesAsync();

        // Przekierowanie do strony z potwierdzeniem rezerwacji
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

        // Oznaczamy pokój jako dostępny
        var roomAvailability = await DbContext.RoomAvailabilities
                                               .FirstOrDefaultAsync(r => r.RoomId == reservation.RoomId && r.Date == reservation.StartDate);

        if (roomAvailability != null)
        {
            roomAvailability.Availability = true;
        }

        await DbContext.SaveChangesAsync();

        // Przekierowanie z powrotem do listy rezerwacji
        return RedirectToAction(nameof(MyReservations));
    }


}
