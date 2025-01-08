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

    // Tworzenie rezerwacji
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateReservation(int roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login", "Account");
        }

        var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);

        if (room == null)
        {
            return NotFound();
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
