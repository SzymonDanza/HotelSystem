using HotelSystem.Data;
using HotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class UserController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Wyświetlanie rezerwacji użytkownika
    public IActionResult MyReservations()
    {
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var reservations = _context.Reservations
            .Include(r => r.Room) // Wczytanie danych o pokoju
            .Where(r => r.GuestName == username) // Filtruj na podstawie zalogowanego użytkownika
            .ToList();

        return View(reservations);
    }

    // Wyświetlanie dostępnych pokoi
    public IActionResult AvailableRooms()
    {
        var rooms = _context.Rooms.ToList(); // Pobierz wszystkie pokoje z bazy danych
        return View(rooms);
    }

    // Tworzenie rezerwacji z walidacją dostępności pokoju
    [HttpPost]
    [ValidateAntiForgeryToken]



    public IActionResult CreateReservation(int roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        if (roomId <= 0 || checkInDate == default || checkOutDate == default || checkInDate >= checkOutDate)
        {
            ViewBag.Error = "Nieprawidłowe dane rezerwacji.";
            return RedirectToAction("AvailableRooms");
        }

        // Sprawdzenie kolidujących rezerwacji
        var overlappingReservations = _context.Reservations
            .Where(r => r.RoomId == roomId &&
                        ((checkInDate >= r.CheckInDate && checkInDate < r.CheckOutDate) ||
                         (checkOutDate > r.CheckInDate && checkOutDate <= r.CheckOutDate) ||
                         (checkInDate <= r.CheckInDate && checkOutDate >= r.CheckOutDate)))
            .ToList();

        if (overlappingReservations.Any())
        {
            // Komunikat o niedostępności terminu
            ViewBag.Error = "Wybrana data jest niedostępna. Proszę wybrać inny termin.";
            return View("AvailableRooms", _context.Rooms.ToList());
        }

        var reservation = new Reservation
        {
            RoomId = roomId,
            GuestName = username,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate
        };

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        return RedirectToAction("MyReservations");
    }



    // Anulowanie rezerwacji
    public IActionResult CancelReservation(int reservationId)
    {
        var reservation = _context.Reservations.FirstOrDefault(r => r.Id == reservationId);

        if (reservation != null)
        {
            _context.Reservations.Remove(reservation);
            _context.SaveChanges();
        }

        return RedirectToAction("MyReservations");
    }
}
