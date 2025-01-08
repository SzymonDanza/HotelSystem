using Microsoft.AspNetCore.Mvc;
using HotelSystem.Data;
using HotelSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace HotelSystem.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Wyświetlenie listy pokoi
        public IActionResult Index()
        {
            var rooms = _context.Rooms.ToList();
            return View(rooms);
        }

        // Formularz do dodawania nowego pokoju (tylko dla admina)
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Obsługa formularza dodawania nowego pokoju
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Room room)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Rooms.Add(room);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(room);
        }

        // Szczegóły pokoju
        public IActionResult Details(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // Edycja pokoju (tylko dla admina)
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Room room)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                _context.Rooms.Update(room);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(room);
        }

        // Usunięcie pokoju (tylko dla admina)
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
