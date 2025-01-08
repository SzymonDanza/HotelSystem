using Microsoft.AspNetCore.Mvc;
using HotelSystem.Data;
using HotelSystem.Models;
using System.Linq;

namespace HotelSystem.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rooms = _context.Rooms.ToList();
            return View(rooms);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddRoom(Room room)
        {
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser != null && currentUser.IsAdmin)
            {
                _context.Rooms.Add(room);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return Unauthorized();
        }
        public IActionResult AdminOnlyAction()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            // Logika tylko dla administratora
            return View();
        }
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


    }
}