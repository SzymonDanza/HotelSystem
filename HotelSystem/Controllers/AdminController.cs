using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelSystem.Data;
using HotelSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Controllers
{
    [Authorize] // Tylko zalogowani użytkownicyac
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        // Lista użytkowników
        public IActionResult Users()
        {
            
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = _dbContext.Users.ToList();
            return View(users);
        }

        // Lista rezerwacji
        public IActionResult Reservations()
        {
            
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var reservations = _dbContext.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .ToList();
            return View(reservations);
        }

        // Usuwanie rezerwacji
        [HttpPost]
        [HttpPost]
        public IActionResult DeleteReservation(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var reservation = _dbContext.Reservations
                                        .Include(r => r.Room)
                                        .FirstOrDefault(r => r.Id == id);
            if (reservation != null)
            {
                
                var roomAvailabilities = _dbContext.RoomAvailabilities
                                                   .Where(ra => ra.RoomId == reservation.RoomId &&
                                                                ra.Date >= reservation.StartDate &&
                                                                ra.Date < reservation.EndDate)
                                                   .ToList();

                foreach (var availability in roomAvailabilities)
                {
                    availability.Availability = true;
                }

                
                _dbContext.Reservations.Remove(reservation);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Reservations");
        }

       
        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var user = _dbContext.Users.Find(userId);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Users");
        }

        // Lista pokoi
        public IActionResult Rooms()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var rooms = _dbContext.Rooms.ToList();
            return View(rooms);
        }

        
        public IActionResult AddRoom()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddRoom(Room room)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                _dbContext.Rooms.Add(room);
                _dbContext.SaveChanges();
                return RedirectToAction("Rooms");
            }
            return View(room);
        }

        
        public IActionResult EditRoom(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var room = _dbContext.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost]
        public IActionResult EditRoom(Room room)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                _dbContext.Rooms.Update(room);
                _dbContext.SaveChanges();
                return RedirectToAction("Rooms");
            }
            return View(room);
        }

        
        [HttpPost]
        public IActionResult DeleteRoom(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (isAdmin != "True")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var room = _dbContext.Rooms.Find(id);
            if (room != null)
            {
                _dbContext.Rooms.Remove(room);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Rooms");
        }
    }
}
