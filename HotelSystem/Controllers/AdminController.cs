using HotelSystem.Data;
using HotelSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Wyświetlanie dashboardu
        public IActionResult Dashboard()
        {
            var rooms = _context.Rooms.ToList();
            var reservations = _context.Reservations.Include(r => r.Room).ToList();
            var users = _context.Users.ToList();

            ViewBag.Rooms = rooms;
            ViewBag.Reservations = reservations;
            ViewBag.Users = users;

            return View();
        }

        // Dodawanie pokoju
        [HttpPost]
        public IActionResult AddRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("Dashboard");
        }

        // Usuwanie pokoju
        public IActionResult DeleteRoom(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
            }
            return RedirectToAction("Dashboard");
        }

        // Podobne akcje dla Reservations i Users...
    }
}
